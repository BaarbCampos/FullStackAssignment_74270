using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportsStore.Models;
using SportsStore.Services.Payments;

namespace SportsStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository repository;
        private readonly Cart cart;
        private readonly IStripePaymentService stripeService;
        private readonly ILogger<OrderController> logger;

        private const string PendingOrderSessionKey = "PendingOrderJson";

        public OrderController(
            IOrderRepository repoService,
            Cart cartService,
            IStripePaymentService stripeService,
            ILogger<OrderController> logger)
        {
            repository = repoService;
            cart = cartService;
            this.stripeService = stripeService;
            this.logger = logger;
        }

        // GET: /Order/Checkout
        [HttpGet]
        public ViewResult Checkout()
        {
            logger.LogInformation("Checkout page opened. CartLines={LineCount}", cart.Lines.Count());
            return View(new Order());
        }

        // POST: /Order/Checkout
        // Starts Stripe Checkout (does NOT save order yet)
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var lineCount = cart.Lines.Count();
            var cartTotal = cart.ComputeTotalValue();

            logger.LogInformation(
                "Checkout POST started. CartLines={LineCount} CartTotal={CartTotal}",
                lineCount,
                cartTotal);

            if (!cart.Lines.Any())
            {
                logger.LogWarning("Checkout blocked: cart is empty.");
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Checkout blocked: invalid model state. Errors={ErrorCount}",
                    ModelState.ErrorCount);

                return View(order);
            }

            // Attach cart lines to order (not saved yet)
            order.Lines = cart.Lines.ToArray();

            // Save pending order in session
            try
            {
                var json = JsonSerializer.Serialize(order);
                HttpContext.Session.SetString(PendingOrderSessionKey, json);

                logger.LogInformation(
                    "Pending order stored in session. SessionKey={SessionKey} Lines={LineCount} Total={CartTotal}",
                    PendingOrderSessionKey,
                    lineCount,
                    cartTotal);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to serialize/store pending order in session.");
                return RedirectToAction(nameof(PaymentFailed));
            }

            // Build URLs for Stripe redirect
            var successUrl = Url.Action(nameof(PaymentSuccess), "Order", null, Request.Scheme)
                             + "?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = Url.Action(nameof(PaymentCancelled), "Order", null, Request.Scheme);

            logger.LogInformation(
                "Starting Stripe checkout. SuccessUrl={SuccessUrl} CancelUrl={CancelUrl}",
                successUrl,
                cancelUrl);

            try
            {
                var checkoutUrl = await stripeService.CreateCheckoutUrlAsync(cart, successUrl!, cancelUrl!);

                logger.LogInformation(
                    "Stripe checkout created. Redirecting user. CheckoutUrl={CheckoutUrl}",
                    checkoutUrl);

                return Redirect(checkoutUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Stripe checkout creation failed.");
                return RedirectToAction(nameof(PaymentFailed));
            }
        }

        // GET: /Order/PaymentSuccess?session_id=...
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string session_id)
        {
            logger.LogInformation("PaymentSuccess called. StripeSessionId={StripeSessionId}", session_id);

            if (string.IsNullOrWhiteSpace(session_id))
            {
                logger.LogWarning("PaymentSuccess called without session_id.");
                return RedirectToAction(nameof(PaymentFailed));
            }

            var pendingJson = HttpContext.Session.GetString(PendingOrderSessionKey);
            if (string.IsNullOrWhiteSpace(pendingJson))
            {
                logger.LogWarning(
                    "No pending order found in session during PaymentSuccess. SessionKey={SessionKey}",
                    PendingOrderSessionKey);

                return RedirectToAction(nameof(PaymentFailed));
            }

            Order? pendingOrder;
            try
            {
                pendingOrder = JsonSerializer.Deserialize<Order>(pendingJson);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to deserialize pending order from session.");
                return RedirectToAction(nameof(PaymentFailed));
            }

            if (pendingOrder is null)
            {
                logger.LogWarning("Pending order deserialized as null.");
                return RedirectToAction(nameof(PaymentFailed));
            }

            // Verify Stripe payment
            try
            {
                var (paid, paymentIntentId, status) = await stripeService.VerifySessionPaidAsync(session_id);

                logger.LogInformation(
                    "Stripe session verified. Paid={Paid} Status={Status} PaymentIntentId={PaymentIntentId} StripeSessionId={StripeSessionId}",
                    paid,
                    status,
                    paymentIntentId,
                    session_id);

                pendingOrder.StripeSessionId = session_id;
                pendingOrder.StripePaymentIntentId = paymentIntentId;
                pendingOrder.StripePaymentStatus = status;

                if (!paid)
                {
                    logger.LogWarning(
                        "Payment not confirmed. Redirecting to PaymentFailed. Status={Status} StripeSessionId={StripeSessionId}",
                        status,
                        session_id);

                    return RedirectToAction(nameof(PaymentFailed));
                }

                // Save order only after payment confirmation
                logger.LogInformation(
                    "Saving order after successful payment. StripeSessionId={StripeSessionId} Total={OrderTotal} Lines={LineCount}",
                    session_id,
                    pendingOrder.Lines?.Sum(l => l.Quantity * l.Product.Price) ?? 0m,
                    pendingOrder.Lines?.Count ?? 0);

                repository.SaveOrder(pendingOrder);

                logger.LogInformation(
                    "Order saved successfully. OrderId={OrderId} StripeSessionId={StripeSessionId}",
                    pendingOrder.OrderID,
                    session_id);

                cart.Clear();
                HttpContext.Session.Remove(PendingOrderSessionKey);

                logger.LogInformation(
                    "Cart cleared and pending session removed. SessionKey={SessionKey}",
                    PendingOrderSessionKey);

                return RedirectToAction(nameof(Completed), new { orderId = pendingOrder.OrderID });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Stripe verification or order save failed. StripeSessionId={StripeSessionId}", session_id);
                return RedirectToAction(nameof(PaymentFailed));
            }
        }

        [HttpGet]
        public IActionResult PaymentCancelled()
        {
            logger.LogInformation("Payment cancelled by user.");

            // optional: clear pending order since user cancelled
            HttpContext.Session.Remove(PendingOrderSessionKey);
            logger.LogInformation("Pending order removed due to cancellation. SessionKey={SessionKey}", PendingOrderSessionKey);

            return View();
        }

        [HttpGet]
        public IActionResult PaymentFailed()
        {
            logger.LogWarning("Payment failed.");

            // optional: keep pending order for retry, OR clear it
            // If you prefer to allow retry without re-entering details, comment out the Remove.
            HttpContext.Session.Remove(PendingOrderSessionKey);
            logger.LogInformation("Pending order removed due to payment failure. SessionKey={SessionKey}", PendingOrderSessionKey);

            return View();
        }

        // GET: /Order/Completed?orderId=123
        [HttpGet]
        public IActionResult Completed(int orderId)
        {
            logger.LogInformation("Completed page opened. OrderId={OrderId}", orderId);

            ViewBag.OrderId = orderId;
            return View();
        }
    }
}
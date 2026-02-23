using SportsStore.Models;
using Stripe.Checkout;

namespace SportsStore.Services.Payments
{
    public class StripePaymentService : IStripePaymentService
    {
        public async Task<string> CreateCheckoutUrlAsync(Cart cart, string successUrl, string cancelUrl)
        {
            if (cart.Lines is null || !cart.Lines.Any())
                throw new InvalidOperationException("Cart is empty.");

            var lineItems = cart.Lines.Select(l => new SessionLineItemOptions
            {
                Quantity = l.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "brl",
                    UnitAmount = (long)(l.Product.Price * 100m),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = l.Product.Name
                    }
                }
            }).ToList();

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                LineItems = lineItems
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            if (string.IsNullOrWhiteSpace(session.Url))
                throw new InvalidOperationException("Stripe session URL not returned.");

            return session.Url;
        }

        public async Task<(bool Paid, string? PaymentIntentId, string Status)> VerifySessionPaidAsync(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            var paid = string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase);

            return (paid, session.PaymentIntentId, session.PaymentStatus ?? "unknown");
        }
    }
}
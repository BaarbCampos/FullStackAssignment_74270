using SportsStore.Models;

namespace SportsStore.Services.Payments
{
    public interface IStripePaymentService
    {
        Task<string> CreateCheckoutUrlAsync(Cart cart, string successUrl, string cancelUrl);

        Task<(bool Paid, string? PaymentIntentId, string Status)> VerifySessionPaidAsync(string sessionId);
    }
}
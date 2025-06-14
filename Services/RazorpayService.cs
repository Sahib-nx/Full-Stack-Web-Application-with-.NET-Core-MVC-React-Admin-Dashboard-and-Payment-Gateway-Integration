using Razorpay.Api;

namespace CRM.Services
{
    public class RazorpayService
    {
        private readonly string key = "rzp_test_RgG9AwfnysizZu";
        private readonly string secret = "1CaqfrrZdfeKKOIdPypjADfr";

        public Order? CreateOrder(decimal amount, string currency, Guid orderId)
        {
            try
            {
                RazorpayClient client = new(key, secret);

                // Convert amount to paise (multiply by 100 for INR)
                int amountInPaise = (int)(amount * 100);

                Dictionary<string, object> options = new Dictionary<string, object>
                {
                    {"amount", amountInPaise}, // Amount in paise
                    {"currency", currency},
                    {"receipt", orderId.ToString()},
                    {"payment_capture", 1}
                };

                Order order = client.Order.Create(options);
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Razorpay Error: {ex.Message}");
                return null;
            }
        }
    }
}
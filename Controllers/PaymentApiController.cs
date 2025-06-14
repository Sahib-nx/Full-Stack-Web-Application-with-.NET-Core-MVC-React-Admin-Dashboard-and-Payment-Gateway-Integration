using CRM.Data;
using CRM.Interfaces;
using CRM.Models.ViewModel;
using CRM.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentApiController : ControllerBase
    {
        private readonly SqlDbContext _dbcontext;
        private readonly ITokenService _tokenService;
        private readonly RazorpayService _razorpayService;

        public PaymentApiController(SqlDbContext dbContext, ITokenService tokenService)
        {
            _dbcontext = dbContext;
            _tokenService = tokenService;
            _razorpayService = new RazorpayService();
        }

        [HttpPost("create/paymentIntent")]
        public IActionResult CreatePaymentIntentO([FromBody] CreatePaymentIntentModel model)
        {
            if (model == null || model.Amount <= 0 || string.IsNullOrEmpty(model.Currency))
            {
                return BadRequest("Invalid payment details.");
            }

            try
            {
                var order = _razorpayService.CreateOrder(model.Amount, model.Currency, model.OrderId);

                if (order == null)
                {
                    return StatusCode(500, "Failed to create payment order.");
                }

                return Ok(new
                {
                    orderId = order["id"].ToString(),
                    entity = order["entity"].ToString(),
                    amount = order["amount"], // This will be in paise
                    amountPaid = order["amount_paid"],
                    amountDue = order["amount_due"],
                    currency = order["currency"].ToString(),
                    receipt = order["receipt"].ToString(),
                    status = order["status"].ToString(),
                    attempts = order["attempts"],
                    createdAt = order["created_at"]
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while creating payment intent: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        public class CreatePaymentIntentModel
        {
            public decimal Amount { get; set; } // Changed from int to decimal
            public string? Currency { get; set; }
            public Guid OrderId { get; set; }
        }
    }
}
using CRM.Data;
using CRM.Interfaces;
using CRM.Models;
using CRM.Models.JunctionModel;
using CRM.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly SqlDbContext _dbcontext;
        private readonly ITokenService _tokenService;
        private readonly HybridModel _viewModel;

        public OrderController(SqlDbContext dbContext, ITokenService tokenService)
        {

            _dbcontext = dbContext;
            _tokenService = tokenService;
            _viewModel = new HybridModel
            {
                Navbar = new NavbarModel { UserRole = Types.Role.Buyer, Isloggedin = true },

            };

        }

        [HttpGet]
        public async Task<IActionResult> Checkout(Guid CartId)
        {

            var token = Request.Cookies["JusTryingToDo"];
            if (token == null)
            {
                return RedirectToAction("Login", "User");
            }

            var userId = _tokenService.VerifyTokenGetId(token);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }
            var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }




            var cart = await _dbcontext.Carts.Include(c => c.CartProducts).FirstOrDefaultAsync(c => c.CartId == CartId);
            if (cart == null)
            {
                ViewBag.cartEmpty = "Cart is Empty";
                return View(_viewModel);     // have to watch it in future if there are no items in cart .. 
            }

            var address = await _dbcontext.Addresses.FirstOrDefaultAsync(a => a.BuyerId.ToString() == userId);

            var cartproducts = await _dbcontext.CartProducts.Include(cp => cp.Product).Where(cp => cp.CartId == cart.CartId).ToListAsync();

            _viewModel.CartProducts = cartproducts;
            _viewModel.Cart = cart;
            _viewModel.Address = address;
            return View(_viewModel);


        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = Request.Cookies["JusTryingToDo"];
            if (token == null)
            {
                return RedirectToAction("Login", "User");
            }

            var userId = _tokenService.VerifyTokenGetId(token);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }
            var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }




            var cart = await _dbcontext.Carts.Include(c => c.CartProducts)
            .ThenInclude(cp => cp.Product)
            .FirstOrDefaultAsync(c => c.BuyerId.ToString() == userId);

            if(cart?.CartValue == 0) {
               
               ViewBag.errorMessage = "Your Cart is Empty!!Proceed Directly to the order's section";
               return View("error" , _viewModel);
            }

            //Converting CartProduct to OrderProduct
            var orderProducts = cart?.CartProducts.Select(cp => new OrderProduct {
                ProductId = cp.ProductId,
                Quantity = cp.Quantity,
            }).ToList();

            var order = new Order {
                OrderStatus = Types.Status.Pending,
                OrderPrice = cart.CartValue,
                BuyerId = Guid.Parse(userId),
                OrderProducts = orderProducts
            };

            var createOrder = await _dbcontext.Orders.AddAsync(order);

            _dbcontext.CartProducts.RemoveRange(cart.CartProducts);
            cart.CartValue = 0;
            await _dbcontext.SaveChangesAsync();
            return RedirectToAction("Payment" , new { order.OrderId });

        }


        [HttpGet]
        public async Task<IActionResult> Payment(Guid OrderId) {

                 var token = Request.Cookies["JusTryingToDo"];
            if (token == null)
            {
                return RedirectToAction("Login", "User");
            }

            var userId = _tokenService.VerifyTokenGetId(token);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }
            var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }



            var order = await _dbcontext.Orders.FirstOrDefaultAsync(o => o.OrderId == OrderId);

            if(order == null) {
                ViewBag.cartEmpty = "No Recent Orders";
                return View(_viewModel);
            }

            var orderProducts = await _dbcontext.OrderProducts
            .Include(op => op.Product)
            .Where(op => op.OrderId == OrderId).ToListAsync();

            var address = await _dbcontext.Addresses.FirstOrDefaultAsync(a => a.BuyerId.ToString() == userId);

            _viewModel.OrderProducts = orderProducts;
            _viewModel.Order = order;
            _viewModel.Address = address;
            
            return View(_viewModel);

        }


    }
}

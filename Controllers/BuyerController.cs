using System.Threading.Tasks;
using CRM.Data;
using CRM.Interfaces;
using CRM.Models;
using CRM.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CRM.Controllers
{
    [Authorize]
    public class BuyerController : Controller
    {
        private readonly SqlDbContext _dbcontext;
        private readonly ITokenService _tokenService;
        private readonly HybridModel _viewModel;

        public BuyerController(SqlDbContext dbContext, ITokenService tokenService)
        {
            _dbcontext = dbContext;
            _tokenService = tokenService;
            _viewModel = new HybridModel
            {
                Navbar = new NavbarModel { UserRole = Types.Role.Buyer, Isloggedin = true },
            };
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BuyerDashboard()
        {
            var token = Request.Cookies["JusTryingToDo"];
            if (token == null)
            {
                return RedirectToAction("Login");
            }

            var userId = _tokenService.VerifyTokenGetId(token);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            ViewBag.UserName = user.UserName;
            ViewBag.UserEmail = user.Email;
            return View(_viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProductFromCart(Guid productId)
        {
            var token = Request.Cookies["JusTryingToDo"];
            if (token == null)
            {
                return Json(new { success = false, message = "Authentication required" });
            }

            var userId = _tokenService.VerifyTokenGetId(token);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Json(new { success = false, message = "Invalid user" });
            }

            try
            {
                var cart = await _dbcontext.Carts
                    .Include(c => c.CartProducts)
                    .FirstOrDefaultAsync(c => c.BuyerId == userGuid);

                if (cart == null)
                {
                    return Json(new { success = false, message = "Cart not found" });
                }

                var cartProduct = await _dbcontext.CartProducts
                    .FirstOrDefaultAsync(cp => cp.CartId == cart.CartId && cp.ProductId == productId);

                if (cartProduct == null)
                {
                    return Json(new { success = false, message = "Product not found in cart" });
                }

                _dbcontext.CartProducts.Remove(cartProduct);
                
                // Recalculate cart value
                var remainingProducts = await _dbcontext.CartProducts
                    .Include(cp => cp.Product)
                    .Where(cp => cp.CartId == cart.CartId && cp.ProductId != productId)
                    .ToListAsync();
                
                cart.CartValue = remainingProducts.Sum(cp => cp.Quantity * cp.Product.Price);
                
                await _dbcontext.SaveChangesAsync();

                return Json(new { success = true, message = "Product removed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error removing product" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartQuantity(Guid productId, int quantity)
        {
            var token = Request.Cookies["JusTryingToDo"];
            if (token == null)
            {
                return Json(new { success = false, message = "Authentication required" });
            }

            var userId = _tokenService.VerifyTokenGetId(token);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Json(new { success = false, message = "Invalid user" });
            }

            if (quantity < 1)
            {
                return Json(new { success = false, message = "Quantity must be at least 1" });
            }

            try
            {
                var cart = await _dbcontext.Carts
                    .Include(c => c.CartProducts)
                    .FirstOrDefaultAsync(c => c.BuyerId == userGuid);

                if (cart == null)
                {
                    return Json(new { success = false, message = "Cart not found" });
                }

                var cartProduct = await _dbcontext.CartProducts
                    .Include(cp => cp.Product)
                    .FirstOrDefaultAsync(cp => cp.CartId == cart.CartId && cp.ProductId == productId);

                if (cartProduct == null)
                {
                    return Json(new { success = false, message = "Product not found in cart" });
                }

                cartProduct.Quantity = quantity;

                // Recalculate cart value
                var allCartProducts = await _dbcontext.CartProducts
                    .Include(cp => cp.Product)
                    .Where(cp => cp.CartId == cart.CartId)
                    .ToListAsync();
                
                cart.CartValue = allCartProducts.Sum(cp => cp.Quantity * cp.Product.Price);

                await _dbcontext.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Quantity updated successfully",
                    newTotal = quantity * cartProduct.Product.Price,
                    cartTotal = cart.CartValue
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating quantity" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
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

            var userGuid = Guid.Parse(userId);
            var cart = await _dbcontext.Carts
                .Include(c => c.CartProducts)
                .FirstOrDefaultAsync(c => c.BuyerId == userGuid);

            if (cart == null || cart.CartProducts.Count == 0)
            {
                ViewBag.cartEmpty = "Cart is Empty";
                return View(_viewModel);
            }

            var cartProducts = await _dbcontext.CartProducts
                .Include(cp => cp.Product)
                .Where(cp => cp.CartId == cart.CartId)
                .ToListAsync();

            _viewModel.CartProducts = cartProducts;
            _viewModel.Cart = cart;

            return View(_viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress(Address address)
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

            var availableAddress = await _dbcontext.Addresses.FirstOrDefaultAsync(a => a.BuyerId.ToString() == userId);

            if (ModelState.IsValid)
            {
                address.BuyerId = Guid.Parse(userId);
                await _dbcontext.Addresses.AddAsync(address);
                await _dbcontext.SaveChangesAsync();
                return RedirectToAction("CheckOut", "Order");
            }

            ViewBag.ErrorMessage = "There was some problem in updating the address!";
            return View("CheckOut", _viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
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

            var orders = await _dbcontext.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(o => o.BuyerId.ToString() == userId)
                .ToListAsync();

            if (orders.Count == 0)
            {
                ViewBag.errorMessage = "No Order Found";
                return View(_viewModel);
            }

            _viewModel.Orders = orders;
            return View(_viewModel);
        }
    }
}
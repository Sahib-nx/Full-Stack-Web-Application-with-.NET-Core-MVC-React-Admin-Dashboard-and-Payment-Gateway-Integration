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
            //    Guid? userId = HttpContext.Items["UserId"] as Guid?;

            // try
            // {
                var token = Request.Cookies["JusTryingToDo"];
                if (token == null)
                {
                    return RedirectToAction("Login");
                }

                var userId = _tokenService.VerifyTokenGetId(token);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login"); // Ensure token verification returns valid userId
                }

            var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null)
            {
                return RedirectToAction("Login" , "User");
            }
            // 💡 Send data to view
            ViewBag.UserName = user.UserName;   
            ViewBag.UserEmail = user.Email;
            return View(_viewModel);


            //     if (user.Role == Types.Role.Buyer)
            //     {
            //         _viewModel.Navbar.UserRole = Types.Role.Buyer;
            //         _viewModel.Navbar.Isloggedin = true;
            //         return View(_viewModel);
            //     }
            //     else
            //     {
            //         return RedirectToAction("Login", "User");
            //     }




            // }
            // catch (Exception ex)
            // {

            //     ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            //     return View();// Return the view with the error message
            // }
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

            // Guid? userId = HttpContext.Items["UserId"] as Guid?;


            var cart = await _dbcontext.Carts.Include(c => c.CartProducts).FirstOrDefaultAsync(c => c.BuyerId.ToString() == userId);

            if (cart == null || cart.CartProducts.Count == 0)
            {
                ViewBag.cartEmpty = "Cart is Empty";
                return View(_viewModel);
            }


            //Now lets Find CartProducts 
            var cartProducts = await _dbcontext.CartProducts.Include(cp => cp.Product).Where(cp => cp.CartId == cart.CartId).ToListAsync();
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

            var availableAdress = await _dbcontext.Addresses.FirstOrDefaultAsync(a => a.BuyerId.ToString() == userId);

            if (ModelState.IsValid)
            {
                address.BuyerId = Guid.Parse(userId);
                await _dbcontext.Addresses.AddAsync(address);
                await _dbcontext.SaveChangesAsync();
                return RedirectToAction("CheckOut", "Order");

            }

            ViewBag.ErrorMessage = "There Was Some problem in updation of Address!! ";
            return View("CheckOut", _viewModel);
        }


        [HttpPost]
       
        public async Task<IActionResult> RemoveItem(int productId)
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


            var cart = await _dbcontext.Carts.Include(c => c.CartProducts).FirstOrDefaultAsync(c => c.BuyerId.ToString() == userId);
            if (cart == null)
                return Json(new { success = false, message = "Cart not found." });

            var cartProducts = await _dbcontext.CartProducts.Include(cp => cp.Product).Where(cp => cp.CartId == cart.CartId).FirstOrDefaultAsync();
            if (cartProducts == null)
                return Json(new { success = false, message = "Item not in cart." });

            _dbcontext.CartProducts.Remove(cartProducts);
            _dbcontext.SaveChanges();
            return View(_viewModel);

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
            .Where(o => o.BuyerId.ToString() == userId).ToListAsync();

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

using ChieuT4_Nhom05_WebQLCF.Data;
using ChieuT4_Nhom05_WebQLCF.Helper;
using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Repositories;
using ChieuT4_Nhom05_WebQLCF.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChieuT4_Nhom05_WebQLCF.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private IMomoService _momoService;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository, IMomoService momoService)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _momoService = momoService;
        }

        public int GetTotalQuantity(ShoppingCart cart)
        {
            int totalQuantity = 0;
            foreach (var item in cart.Items)
            {
                totalQuantity += item.Quantity;
            }
            return totalQuantity;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
            Product product = await GetProductFromDatabaseAsync(productId);
            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity
            };
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            // Set thông báo vào TempData

            int totalQuantity = GetTotalQuantity(cart); // Đếm số lượng giỏ hàng
            ViewBag.CartTotalQuantity = totalQuantity;

            TempData["SuccessMessage"] = "Product added to cart successfully!";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            return View(new Order());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, string PaymentMethod, string PayUrl = null)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống...
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");

            if (PaymentMethod == "Cash")
            {
                // Xử lý thanh toán tiền mặt ở đây
                return RedirectToAction("CashPaymentConfirmation");
            }
            else if (PaymentMethod == "Online")
            {
                // Tạo yêu cầu thanh toán Momo
                var orderInfo = new OrderInfoModel
                {
                    OrderId = Convert.ToString(order.Id), // ID của đơn hàng
                    Amount = Convert.ToDouble(order.TotalPrice), // Tổng số tiền thanh toán
                    OrderInfo = "Thanh toán đơn hàng #" + order.Id // Thông tin đơn hàng
                };
                var response = await _momoService.CreatePaymentAsync(orderInfo);

                // Kiểm tra nếu response.PayUrl không rỗng hoặc null
                if (!string.IsNullOrEmpty(response.PayUrl))
                {
                    // Chuyển hướng người dùng đến trang thanh toán Momo
                    return Redirect(response.PayUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Không thể tạo yêu cầu thanh toán. Vui lòng thử lại sau.");
                    return View(order);
                }
            }
            return View(order);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
                if (cartItem != null)
                {
                    // Kiểm tra nếu số lượng mới là hợp lệ (lớn hơn 0)
                    if (quantity > 0)
                    {
                        cartItem.Quantity = quantity;
                        HttpContext.Session.SetObjectAsJson("Cart", cart);
                    }
                    else
                    {
                        // Nếu số lượng là không hợp lệ, xóa sản phẩm khỏi giỏ hàng
                        cart.RemoveItem(productId);
                        HttpContext.Session.SetObjectAsJson("Cart", cart);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult CashPaymentConfirmation()
        {
            // Hiển thị trang xác nhận thanh toán tiền mặt
            return View();
        }

       /* [HttpPost]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model, string PayUrl = null)
        {
            var response = await _momoService.CreatePaymentAsync(model);
            return Redirect(response.PayUrl);
        }

        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }*/

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        // Các actions khác...
        private async Task<Product> GetProductFromDatabaseAsync(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            return await _productRepository.GetByIdAsync(productId);
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                // Tìm sản phẩm trong giỏ hàng và xóa nó
                var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
                if (cartItem != null)
                {
                    cart.RemoveItem(productId);
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                }
            }

            return RedirectToAction("Index"); // Hoặc chuyển hướng đến trang giỏ hàng hoặc trang nào đó khác
        }
    }
}

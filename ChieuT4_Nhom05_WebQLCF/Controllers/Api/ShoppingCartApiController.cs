using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ChieuT4_Nhom05_WebQLCF.Data;
using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Services;
using System.Linq;
using Azure;
using ChieuT4_Nhom05_WebQLCF.Helper;
using ChieuT4_Nhom05_WebQLCF.Repositories;

namespace ChieuT4_Nhom05_WebQLCF.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMomoService _momoService;
        private readonly IProductRepository _productRepository;

        public ShoppingCartApiController(ApplicationDbContext context, IProductRepository productRepository, IMomoService momoService)
        {
            _context = context;
            _productRepository = productRepository;
            _momoService = momoService;
        }

        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(int cartItemId, int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            var cartItem = new CartItem
            {
                //IdCartItem = cartItemId, //chú ý
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity
            };

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Ok(new { message = "Product added to cart successfully!", cart });
        }

        [HttpPost("RemoveFromCart")]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null)
            {
                return NotFound("Cart not found");
            }

            cart.RemoveItem(productId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Ok(new { message = "Product removed from cart successfully!", cart });

        }

        [HttpPost("UpdateQuantity")]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null)
            {
                return NotFound("Cart not found");
            }

            var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    cart.RemoveItem(productId);
                }
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return Ok(new { cart });
        }

        [HttpGet("GetCart")]
        public IActionResult GetCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return Ok(cart);
        }

        [HttpPost("CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);

            if (response != null && !string.IsNullOrEmpty(response.PayUrl))
            {
                return Ok(new { payUrl = response.PayUrl });
            }

            return StatusCode(500, "Unable to create payment URL. Please try again later.");
        }

        [HttpGet("PaymentCallBack")]
        public IActionResult PaymentCallBack()
        {
            // Thực hiện hàm xử lý với dữ liệu truy vấn
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

            // Kiểm tra xem phản hồi có tồn tại và có dữ liệu cần thiết không
            if (response == null)
            {
                return BadRequest(new { message = "No response from payment service." });
            }

            // Kiểm tra nội dung phản hồi để xác định thành công
            if (string.IsNullOrEmpty(response.OrderId))
            {
                return BadRequest(new { message = "Invalid response from payment service." });
            }

            // Trả về phản hồi JSON nếu thành công
            return Ok(new { message = "Payment callback successful.", data = response });
        }

    }
}


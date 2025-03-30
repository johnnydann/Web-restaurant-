using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Text.Json;
using X.PagedList;
using Microsoft.EntityFrameworkCore;
using ChieuT4_Nhom05_WebQLCF.Data;

namespace ChieuT4_Nhom05_WebQLCF.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;

        public ProductApiController(IProductRepository productRepository, ICategoryRepository categoryRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }

        [HttpGet("getall", Name = "GetAllProducts")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts(int pageNumber = 1, int pageSize = 8)
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                var activeProducts = products.Where(p => p.IsActive).ToList();

                foreach (var product in activeProducts)
                {
                    if (product.CategoryId != 0)
                    {
                        product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                        // Ensure that product.Category does not contain the list of products to avoid reference cycles
                        if (product.Category != null)
                        {
                            product.Category.Products = null;
                        }
                    }
                }

                var paginatedProducts = activeProducts
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(paginatedProducts);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("getproductbyid/{id}", Name = "GetProductById")]
        public async Task<IActionResult> GetProductById(int id)
        {

            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                if (product.CategoryId != 0)
                {
                    product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                    if (product.Category != null)
                    {
                        product.Category.Products = null; // Tránh chu kỳ tham chiếu
                    }
                }

                return Ok(product);
            }
            catch (Exception ex)

            {
                return NotFound();
            }
           
        }

        [HttpPost("add")]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        public async Task<IActionResult> AddProduct([FromForm] Product product, IFormFile imageFile)
        {
            try
            {
                if (product == null)
                {
                    return BadRequest("Dữ liệu sản phẩm bị thiếu");
                }

                // Xử lý việc tải ảnh lên
                if (imageFile != null)
                {
                    product.ImageUrl = await SaveImage(imageFile);
                }

                // Lưu sản phẩm vào repository
                await _productRepository.AddAsync(product);

                // Trả về thông tin sản phẩm đã được tạo
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi nếu có logger (không được hiển thị trong đoạn mã này)
                return StatusCode(500, "Lỗi máy chủ nội bộ: " + ex.Message);

            }
        }


        [HttpPut("update/{id}", Name = "UpdateProduct")]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Product product, IFormFile imageFile)
        {

            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            product.Id = existingProduct.Id;

            if (imageFile != null)
            {
                product.ImageUrl = await SaveImage(imageFile);
            }
            else
            {
                product.ImageUrl = existingProduct.ImageUrl;
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.ImageUrl = product.ImageUrl;

            await _productRepository.UpdateAsync(existingProduct);
            return NoContent();
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var savePath = Path.Combine("wwwroot/productImages", imageFile.FileName); // Adjust the path according to your configuration
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return "/productImages/" + imageFile.FileName; // Return relative path
        }

        [HttpDelete("delete/{id}", Name = "DeleteProduct")]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _productRepository.DeleteAsync(id);
            return Ok(new
            {
                success = true,
                message = "Delete successful",
            });
        }
        
        [HttpPost("activate")]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        public async Task<ActionResult> ActivateProduct(int id)
        {
            await _productRepository.ActivateAsync(id);
            return Ok(new { success = true, message = "Product activated successfully!" });
        }

        [HttpGet("inactive")]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        public async Task<IActionResult> GetInactiveProducts()
        {
            var inactiveProducts = await _productRepository.GetInactiveProductsAsync();

            foreach (var product in inactiveProducts)
            {
                if (product.CategoryId != 0)
                {
                    product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                    // Đảm bảo rằng product.Category không chứa danh sách sản phẩm để tránh chu kỳ tham chiếu
                    if (product.Category != null)
                    {
                        product.Category.Products = null;
                    }
                }
            }
            return Ok(inactiveProducts);
        }
    }
}




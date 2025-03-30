using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Repositories;

namespace ChieuT4_Nhom05_WebQLCF.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }


        // Hi?n th? danh sách s?n ph?m
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            foreach (var product in products)
            {
                if (product.CategoryId != null)
                {
                    products = products.Where(p => p.IsActive).ToList(); // Chỉ lấy sản phẩm đang hoạt động
                    product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                }
            }
            return View(products);
        }
        // Hi?n th? form thêm s?n ph?m m?i
        public async Task<IActionResult> AddAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        // X? lý thêm s?n ph?m m?i
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl, List<IFormFile> imageUrls)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    // Luu hình ?nh d?i di?n
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                await _productRepository.AddAsync(product);
                return RedirectToAction("Index");
            }
            // N?u ModelState không h?p l?, hi?n th? form v?i d? li?u dã nh?p
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/productImages", image.FileName); // Thay d?i du?ng d?n theo c?u hình c?a b?n
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/productImages/" + image.FileName; // Tr? v? du?ng d?n tuong d?i
        }


        // Hi?n th? thông tin chi ti?t s?n ph?m
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Hi?n th? form c?p nh?t s?n ph?m
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // X? lý c?p nh?t s?n ph?m
        [HttpPost]
        public async Task<IActionResult> Update(int id, [FromForm] Product product, IFormFile ImageUrl)
        {
            ModelState.Remove("ImageUrl");
            if (id != product.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                if (ImageUrl != null)
                {
                    // Luu hình ?nh d?i di?n
                    product.ImageUrl = await SaveImage(ImageUrl);
                }
                //Edit không m?t hình ?nh
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (ImageUrl == null)
                {
                    product.ImageUrl = existingProduct?.ImageUrl;
                }
                else
                {
                    product.ImageUrl = await SaveImage(ImageUrl);
                }
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;
                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // Hi?n th? form xác nh?n xóa s?n ph?m
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // X? lư xóa s?n ph?m
        // xoa mem 
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        // hien thi danh sach xoa mem
        public async Task<IActionResult> Activate(int id)
        {
            await _productRepository.ActivateAsync(id);
            return Json(new { success = true, message = "Product activated successfully!" });
        }

        public async Task<IActionResult> Inactive()
        {
            var products = await _productRepository.GetAllAsync();
            var inactiveProducts = await _productRepository.GetInactiveProductsAsync();
            foreach (var product in products)
            {
                if (product.CategoryId != null)
                {

                    product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                }
            }
            return View(inactiveProducts);
        }
    }
}

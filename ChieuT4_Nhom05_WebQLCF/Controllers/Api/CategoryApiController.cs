using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChieuT4_Nhom05_WebQLCF.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryApiController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet("getall", Name = "GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return Ok(categories); // Trả về mã 200 với danh sách danh mục
        }

        [HttpGet("getbyId", Name = "GetCategoryById")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" }); // Trả về 404
            }
            return Ok(category);
        }

        [HttpPost("add")]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            if (category == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid category data" }); // Trả về 400
            }

            await _categoryRepository.AddAsync(category);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category); // Trả về 201
        }

        [HttpPut("update", Name = "UpdateCategory")]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            try
            {
                if (id != category.Id)
                {
                    return BadRequest();
                }
                await _categoryRepository.UpdateAsync(category);
                return Ok(new
                {
                    success = true,
                    message = "Update successful",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete", Name = "DeleteCategory")]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" }); // Trả về 404
            }

            await _categoryRepository.DeleteAsync(id);
            return Ok(new
            {
                success = true,
                message = "Delete successful",
            }); // Trả về 204 sau khi xóa thành công
        }
    }
}

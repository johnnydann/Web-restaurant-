using ChieuT4_Nhom05_WebQLCF.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChieuT4_Nhom05_WebQLCF.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public SearchController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Please provide a search query.");
            }

            var products = await _productRepository.SearchByNameAsync(query);
            return Ok(products);
        }

        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Please provide a search term.");
            }

            var products = await _productRepository.SearchByNameAsync(term);
            var suggestions = products.Select(p => p.Name).Distinct().ToList();
            return Ok(suggestions);
        }
    }
}

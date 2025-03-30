using ChieuT4_Nhom05_WebQLCF.Data;
using ChieuT4_Nhom05_WebQLCF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Text.Json;


namespace ChieuT4_Nhom05_WebQLCF.Controllers.Api
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
    public class QLDHApiController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        public QLDHApiController(ApplicationDbContext dbContext)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext)); // Ensure dbContext is not null
            }
            db = dbContext;
        }

        [HttpGet("getOrders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(int? pageSize = 10)
        {
            try
            {
                if (db == null)
                {
                    return NotFound("Database context is null.");
                }

                int defaultPageSize = 10;
                int size = pageSize ?? defaultPageSize;

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve // Duy trì các chuỗi tham chiếu để tránh vòng lặp
                };

                var orders = await db.Orders
                                     .Include(o => o.OrderDetails)
                                     .ThenInclude(od => od.Product)
                                     .OrderByDescending(o => o.Id)
                                     .Take(size)
                                     .ToListAsync();

                if (orders == null || !orders.Any())
                {
                    return NotFound("No orders found.");
                }

                return Ok(JsonSerializer.Serialize(orders, options)); // Serialize đối tượng với ReferenceHandler.Preserve
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

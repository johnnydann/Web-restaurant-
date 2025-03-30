using ChieuT4_Nhom05_WebQLCF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ChieuT4_Nhom05_WebQLCF.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> CreateAdminAccount()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            var user = new ApplicationUser
            {
                UserName = "namdang@gmail.com",
                Email = "namdang@gmail.com"
            };
            var result = await _userManager.CreateAsync(user, "Nam123@");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return Content("Admin account created successfully");
            }
            return BadRequest("Faile to create admin account");
        }

        public async Task<IActionResult> listUser()
        {
            var listUsers = await _userManager.Users.ToListAsync();
            return View(listUsers); // Sửa thành truyền danh sách người dùng vào view
        }
        public async Task<IActionResult> BannedUser(string userId)
        {
            var bannedUser = await _userManager.FindByIdAsync(userId);
            if (bannedUser != null)
            {
                // Khóa tài khoản vĩnh viễn
                await _userManager.SetLockoutEndDateAsync(bannedUser, DateTimeOffset.MaxValue);
                TempData["Message"] = $"Tài khoản {bannedUser.Email} đã bị khóa.";
                return RedirectToAction("listUser"); // Chuyển hướng lại danh sách người dùng
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> UnbanUser(string userId)
        {
            var unbannedUser = await _userManager.FindByIdAsync(userId);
            if (unbannedUser != null)
            {
                // Gỡ khóa tài khoản
                await _userManager.SetLockoutEndDateAsync(unbannedUser, null);
                TempData["Message"] = $"Tài khoản {unbannedUser.Email} đã được mở khóa.";
                return RedirectToAction("listUser"); // Chuyển hướng lại danh sách người dùng
            }
            return NotFound();
        }

    }
}

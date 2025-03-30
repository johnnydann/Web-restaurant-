using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChieuT4_Nhom05_WebQLCF.Models;

[Route("api/[controller]")]
[ApiController]
public class AuthApiController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public AuthApiController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
    {
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtKey = configuration["Jwt:Key"];
        _jwtIssuer = configuration["Jwt:Issuer"];
        _jwtAudience = configuration["Jwt:Audience"];
    }

    /* [HttpPost("register")]
     public async Task<IActionResult> Register([FromBody] Register model)
     {
         if (model == null)
         {
             return BadRequest("Model data is missing.");
         }

         var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
         var result = await _userManager.CreateAsync(user, model.Password);

         if (!result.Succeeded)
         {
             return BadRequest(new
             {
                 success = false,
                 message = "Registration failed",
                 errors = result.Errors.Select(e => e.Description).ToArray() // Chi tiết lỗi
             });
         }

         return Ok(new
         {
             success = true,
             message = "Registration successful",
             data = user
         });
     }*/

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Register model)
    {
        if (model == null)
        {
            return BadRequest("Model data is missing.");
        }

        // Kiểm tra Password và ConfirmPassword
        if (model.Password != model.ConfirmPassword)
        {
            return BadRequest(new
            {
                success = false,
                message = "Password and Confirm Password do not match."
            });
        }

        var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                success = false,
                message = "Registration failed",
                errors = result.Errors.Select(e => e.Description).ToArray()
            });
        }

        return Ok(new
        {
            success = true,
            message = "Registration successful",
            data = user
        });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login model)
    {
        if (model == null)
        {
            return BadRequest("Login data is missing.");
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Username, model.Password, isPersistent: false, lockoutOnFailure: false
        );

        if (!result.Succeeded)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Login failed. Invalid username or password.",
                reasons = result.RequiresTwoFactor ? "Requires two-factor authentication." : null
            });
        }

        var user = await _userManager.FindByNameAsync(model.Username);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            success = true,
            message = "Login successful",
            token = tokenString
        });
    }
}

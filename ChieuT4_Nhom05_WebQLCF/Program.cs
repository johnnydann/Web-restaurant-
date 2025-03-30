using ChieuT4_Nhom05_WebQLCF.Data;
using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Models.Momo;
using ChieuT4_Nhom05_WebQLCF.Repositories;
using ChieuT4_Nhom05_WebQLCF.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using ChieuT4_Nhom05_WebQLCF.Hubs;
using System.Configuration;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


// Cấu hình session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Helper.SessionExtensions";
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Xác thực JWT
var jwtIssuer = configuration["Jwt:Issuer"];
var jwtAudience = configuration["Jwt:Audience"];
var jwtKey = configuration["Jwt:Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        //ClockSkew = TimeSpan.Zero //Hết token đúng hạn
    };
});

// Google Login
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["GoogleLogin:Authentication:Google:ClientId"];
    googleOptions.ClientSecret = configuration["GoogleLogin:Authentication:Google:ClientSecret"];
    googleOptions.CallbackPath = "/signin-google"; // Đảm bảo CallbackPath khớp với cấu hình Google OAuth
});

// Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

/*builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
*/

/*builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.JsonSerializerOptions.MaxDepth = 10; // Đặt giới hạn về độ sâu
});*/


// Đăng ký repositories
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

// Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Cấu hình chính sách bảo mật
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnlyPolicy", policy =>
        policy.RequireRole("Admin"));
});

// Đăng ký Razor Pages và MVC
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Cấu hình các dịch vụ từ file cấu hình
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();


// Cấu hình Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your Api", Version = "v1" });

    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securitySchema);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securitySchema, new[] { "Bearer" } }
    });

});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:5241/")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

// Cấu hình Redis với ConnectionString từ file appsettings.json hoặc cấu hình cứng
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
//    options.InstanceName = "RedisInstance";  // Tùy chọn để định danh
//});

// Add SignalR
builder.Services.AddSignalR();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


//api
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your Api V1");
});



app.UseCors("AllowAllOrigins");

builder.Services.AddSession();
// Cấu hình session
app.UseSession();

// Xử lý ngoại lệ và bảo mật cho ứng dụng
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your Api V1");
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your Api V1");
    });
}

// Cấu hình HTTP
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    var authorizationHeader = context.Request.Headers["Authorization"].ToString();

    if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
    {
        Console.WriteLine("JWT token is missing or does not start with 'Bearer '");

    }
    else
    {
        var token = authorizationHeader.Substring("Bearer ".Length);
        Console.WriteLine("JWT token found: " + token);

        // Bạn có thể kiểm tra xem token có hợp lệ không
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null)
            {
                Console.WriteLine("Invalid JWT token format");
                //new v2
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
            else
            {
                Console.WriteLine("JWT token is valid");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Invalid JWT token: " + ex.Message);
        }
    }

    await next(); // Tiếp tục qua middleware tiếp theo
});

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Định tuyến cho các Area và các route cơ bản
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "Admin",
        pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}"
    );

    endpoints.MapControllerRoute(
        name: "Customer",
        pattern: "{area:exists}/{controller=Home}/{action=Search}/{query?}"
    );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Map SignalR hub
    endpoints.MapHub<CallHub>("/callhub");

    endpoints.MapControllers();

});




app.Run();
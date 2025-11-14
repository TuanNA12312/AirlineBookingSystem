using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Cookie Authentication (để MVC "nhớ" đăng nhập)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Chuyển đến trang Login nếu chưa đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Trang báo lỗi 403 (khi User vào trang Admin)
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

// 2. Đăng ký Dịch vụ để truy cập HttpContext (để đọc Session/Token)
builder.Services.AddHttpContextAccessor();

// 3. (RẤT QUAN TRỌNG) Cấu hình HttpClientFactory
// Tự động gắn Token (JWT) vào header cho MỌI request
builder.Services.AddHttpClient("ApiClient", (serviceProvider, client) =>
{
    // Lấy URL của API từ file appsettings.json
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["ApiSettings:ApiBaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    // --- PHẦN TỰ ĐỘNG GẮN TOKEN (CODE MỚI) ---
    // (Lấy Token từ Session và tự động gắn vào mọi request)
    var contextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var token = contextAccessor.HttpContext?.Session.GetString("JWToken");
    if (!string.IsNullOrEmpty(token))
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
});

// 4. Đăng ký Session (để lưu Token)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 5. Thêm MVC (Nâng cấp)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => // (Thêm phần này để xử lý JSON)
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // <-- Bật Session (Phải trước UseAuth)

// 6. Bật Authentication & Authorization (CODE MỚI)
app.UseAuthentication(); // 1. Xác thực (Cookie)
app.UseAuthorization();  // 2. Phân quyền (Role)

// 7. Cấu hình Route cho Admin Area (CODE MỚI)
app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
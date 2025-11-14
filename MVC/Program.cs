namespace MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 2. Đăng ký HttpClientFactory
            // Nó sẽ tạo ra các "client" để gọi API
            builder.Services.AddHttpClient("ApiClient", (serviceProvider, client) =>
            {
                // Lấy URL của API từ file appsettings.json
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(config["ApiSettings:ApiBaseUrl"]!);

                // (Chúng ta sẽ thêm Token vào đây ở các Controller sau)
            });

            // 3. (Rất quan trọng) Đăng ký Dịch vụ Session
            // Session sẽ được dùng để LƯU LẠI "Token" (JWT)
            // và thông tin User sau khi đăng nhập thành công
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian chờ
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

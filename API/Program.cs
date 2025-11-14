using API.Services;
using DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories.Implementations;
using Repositories.Interfaces;
using System.Text;
using System.Text.Json.Serialization;


namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAirportRepository, AirportRepository>();
            builder.Services.AddScoped<IAirlineRepository, AirlineRepository>();
            builder.Services.AddScoped<IFlightRepository, FlightRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IPassengerRepository, PassengerRepository>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();
            builder.Services.AddScoped<ISeatClassRepository, SeatClassRepository>();
            builder.Services.AddScoped<IFlightPriceRepository, FlightPriceRepository>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            // 4. Đăng ký dịch vụ Xác thực (Authentication)
            // (Dạy cho API cách đọc và hiểu JWT)
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Cấu hình cách API xác thực Token
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, // Phải xác thực Issuer
                        ValidateAudience = true, // Phải xác thực Audience
                        ValidateLifetime = true, // Phải xác thực thời gian sống (chống token hết hạn)
                        ValidateIssuerSigningKey = true, // Phải xác thực chữ ký (key bí mật)

                        // Lấy thông tin từ file appsettings.json
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                        )
                    };
                });

            // 5. Đăng ký dịch vụ Phân quyền (Authorization)
            // (Dịch vụ này dùng để kiểm tra [Authorize(Roles = "Admin")])
            builder.Services.AddAuthorization();

            // 6. Đăng ký Controllers
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                // Thêm dòng này để "phá vỡ" vòng lặp vô tận
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            // 7. Đăng ký Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 8. Đăng ký CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Cấu hình tiêu đề cho Swagger UI
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "AirlineBooking API", Version = "v1" });

                // --- ĐÂY LÀ PHẦN "TẠO NÚT BEARER" ---
                // 1. Định nghĩa Security Scheme (Cách Swagger nhận token)
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization", // Tên của HTTP Header
                    Type = SecuritySchemeType.Http, // Kiểu
                    Scheme = "Bearer", // Lược đồ (phải là "Bearer")
                    BearerFormat = "JWT", // Định dạng
                    In = ParameterLocation.Header, // Nơi đặt token (trong Header)
                    Description = "Nhập 'Bearer [dấu cách] token' vào ô 'Value' bên dưới."
                });

                // 2. Yêu cầu Swagger phải sử dụng Scheme này
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer" // ID phải khớp với tên ở trên
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll"); // (Phải đặt trước UseAuthentication/UseAuthorization)

            // 9. Bật 2 dịch vụ bảo mật
            // Phải đặt ĐÚNG THỨ TỰ này:
            app.UseAuthentication(); // 1. Xác thực (Bạn là ai? Token có hợp lệ không?)
            app.UseAuthorization();  // 2. Phân quyền (Bạn có được phép làm điều này không?)

            // 10. Map Controllers
            app.MapControllers();

            // 11. Chạy ứng dụng
            app.Run();
        }
    }
}

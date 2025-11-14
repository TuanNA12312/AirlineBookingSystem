using BusinessObject;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<SeatClass> SeatClasses { get; set; }
        public DbSet<FlightPrice> FlightPrices { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Bắt buộc gọi hàm base
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình User
            modelBuilder.Entity<User>(entity =>
            {
                // Đảm bảo Email là duy nhất (không trùng)
                // Quan trọng cho việc đăng nhập và đăng ký
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // 2. Cấu hình Airport (Sân bay)
            modelBuilder.Entity<Airport>(entity =>
            {
                // Khóa chính là AirportCode (kiểu string)
                entity.HasKey(a => a.AirportCode);
            });

            // 3. Cấu hình Airline (Hãng bay)
            modelBuilder.Entity<Airline>(entity =>
            {
                // Khóa chính là AirlineCode (kiểu string)
                entity.HasKey(a => a.AirlineCode);
            });

            // 4. Cấu hình Flight (Chuyến bay)
            modelBuilder.Entity<Flight>(entity =>
            {
                // Mối quan hệ với Airport (Điểm đi)
                entity.HasOne(f => f.DepartureAirport)
                    .WithMany() // Một Airport có thể có nhiều chuyến bay
                    .HasForeignKey(f => f.DepartureAirportCode)
                    .OnDelete(DeleteBehavior.Restrict); // Hạn chế (báo lỗi) khi xoá Airport

                // Mối quan hệ với Airport (Điểm đến)
                entity.HasOne(f => f.ArrivalAirport)
                    .WithMany()
                    .HasForeignKey(f => f.ArrivalAirportCode)
                    .OnDelete(DeleteBehavior.Restrict); // Hạn chế

                // Mối quan hệ với Airline (Hãng bay)
                entity.HasOne(f => f.Airline)
                    .WithMany(a => a.Flights) // Mối quan hệ 1-Nhiều
                    .HasForeignKey(f => f.AirlineCode)
                    .OnDelete(DeleteBehavior.Restrict); // Hạn chế
            });

            // 5. Cấu hình Booking (Đơn đặt chỗ)
            modelBuilder.Entity<Booking>(entity =>
            {
                // Mối quan hệ với User
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bookings) // 1 User có nhiều Booking
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Khi xoá User, xoá luôn Booking
            });

            // 6. Cấu hình Ticket (Vé)
            modelBuilder.Entity<Ticket>(entity =>
            {
                // Mối quan hệ với Booking
                entity.HasOne(t => t.Booking)
                    .WithMany(b => b.Tickets) // 1 Booking có nhiều Ticket
                    .HasForeignKey(t => t.BookingId)
                    .OnDelete(DeleteBehavior.Cascade); // Khi xoá Booking, xoá luôn Ticket

                // Mối quan hệ với Flight
                entity.HasOne(t => t.Flight)
                    .WithMany() // 1 Flight có thể có nhiều Ticket
                    .HasForeignKey(t => t.FlightId)
                    .OnDelete(DeleteBehavior.Restrict); // Hạn chế xoá Flight nếu đã có vé

                // Mối quan hệ với Passenger
                entity.HasOne(t => t.Passenger)
                    .WithMany(p => p.Tickets) // 1 Passenger có nhiều Ticket
                    .HasForeignKey(t => t.PassengerId)
                    .OnDelete(DeleteBehavior.Cascade); // Khi xoá Passenger, xoá luôn Ticket
            });

            // 7. Cấu hình FlightPrice (Giá vé)
            modelBuilder.Entity<FlightPrice>(entity =>
            {
                // Mối quan hệ với Flight
                entity.HasOne(p => p.Flight)
                    .WithMany(f => f.Prices) // 1 Flight có nhiều mức giá
                    .HasForeignKey(p => p.FlightId)
                    .OnDelete(DeleteBehavior.Cascade); // Xoá Flight thì xoá luôn giá
            });

            modelBuilder.Entity<SeatClass>().HasData(
                new SeatClass { SeatClassId = 1, ClassName = "Economy" },
                new SeatClass { SeatClassId = 2, ClassName = "Business" }
            );

            // 2. Seed Sân bay (Airport)
            modelBuilder.Entity<Airport>().HasData(
                new Airport { AirportCode = "SGN", AirportName = "Tân Sơn Nhất", City = "Hồ Chí Minh" },
                new Airport { AirportCode = "HAN", AirportName = "Nội Bài", City = "Hà Nội" }
            );

            // 3. Seed Hãng bay (Airline)
            modelBuilder.Entity<Airline>().HasData(
                new Airline { AirlineCode = "VN", AirlineName = "Vietnam Airlines" },
                new Airline { AirlineCode = "VJ", AirlineName = "VietJet Air" }
            );

            // 4. Seed User (Admin và User)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1, // Gán ID thủ công
                    Email = "admin@gmail.com",
                    FullName = "Admin Quản Trị",
                    PasswordHash = "admin123", // Mật khẩu (theo yêu cầu là không băm)
                    IsAdmin = true
                },
                new User
                {
                    UserId = 2, // Gán ID thủ công
                    Email = "user@gmail.com",
                    FullName = "Người Dùng Thường",
                    PasswordHash = "user123",
                    IsAdmin = false
                }
            );

            // 5. Seed Chuyến bay (Flight)
            modelBuilder.Entity<Flight>().HasData(
                new Flight
                {
                    FlightId = 1, // Gán ID thủ công
                    FlightNumber = "VN208",
                    DepartureTime = new DateTime(2025, 12, 20, 10, 30, 0),
                    ArrivalTime = new DateTime(2025, 12, 20, 12, 30, 0),
                    DepartureAirportCode = "SGN",
                    ArrivalAirportCode = "HAN",
                    AirlineCode = "VN",
                    TotalSeats = 150,
                    AvailableSeats = 150
                },
                new Flight
                {
                    FlightId = 2, // Gán ID thủ công
                    FlightNumber = "VJ101",
                    DepartureTime = new DateTime(2025, 12, 20, 11, 00, 0),
                    ArrivalTime = new DateTime(2025, 12, 20, 13, 00, 0),
                    DepartureAirportCode = "SGN",
                    ArrivalAirportCode = "HAN",
                    AirlineCode = "VJ",
                    TotalSeats = 180,
                    AvailableSeats = 180
                }
            );

            // 6. Seed Giá vé (FlightPrice)
            modelBuilder.Entity<FlightPrice>().HasData(
                // Giá cho Flight 1 (VN208)
                new FlightPrice { FlightPriceId = 1, FlightId = 1, SeatClassId = 1, Price = 2500000 }, // Eco
                new FlightPrice { FlightPriceId = 2, FlightId = 1, SeatClassId = 2, Price = 4500000 }, // Bus

                // Giá cho Flight 2 (VJ101)
                new FlightPrice { FlightPriceId = 3, FlightId = 2, SeatClassId = 1, Price = 1800000 }  // Eco
            );  
        }
    }
}

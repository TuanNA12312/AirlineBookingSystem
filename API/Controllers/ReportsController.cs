using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IUserRepository _userRepo;
        private readonly IFlightRepository _flightRepo;

        public ReportsController(IBookingRepository b, IUserRepository u, IFlightRepository f)
        {
            _bookingRepo = b; _userRepo = u; _flightRepo = f;
        }

        // POST: /api/reports/revenue
        [HttpPost("revenue")]
        public async Task<ActionResult<decimal>> GetRevenueReport(ReportRequestDto request)
        {
            var revenue = await _bookingRepo.GetRevenueReportAsync(request.FromDate, request.ToDate);
            return Ok(new { TotalRevenue = revenue });
        }

        // GET: /api/reports/dashboard-stats
        [HttpGet("dashboard-stats")]
        public async Task<ActionResult> GetDashboardStats()
        {
            var totalUsers = await _userRepo.GetRegisteredUsersCountAsync();
            var totalBookings = await _bookingRepo.GetTotalBookingsCountAsync();
            var totalFlights = await _flightRepo.GetTotalFlightsCountAsync();

            return Ok(new
            {
                TotalUsers = totalUsers,
                TotalBookings = totalBookings,
                TotalFlights = totalFlights
            });
        }
    }
}

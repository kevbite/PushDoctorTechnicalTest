using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly PatientBookingContext _context;

        public BookingController(PatientBookingContext context)
        {
            _context = context;
        }

        [HttpGet("patient/{identificationNumber}/next")]
        public IActionResult GetPatientNextAppointment(long identificationNumber)
        {
            var bockings = _context.Order.OrderBy(x => x.StartTime).ToList();

            if (bockings.Where(x => x.Patient.Id == identificationNumber).Count() == 0)
            {
                return StatusCode(502);
            }
            else
            {
                var bookings2 = bockings.Where(x => x.PatientId == identificationNumber);
                if (bookings2.Where(x => x.StartTime > DateTime.Now).Count() == 0)
                {
                    return StatusCode(502);
                }
                else
                {
                    var bookings3 = bookings2.Where(x => x.StartTime > DateTime.Now);
                    return Ok(new
                    {
                        bookings3.First().Id,
                        bookings3.First().DoctorId,
                        bookings3.First().StartTime,
                        bookings3.First().EndTime
                    });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddBooking(NewBooking newBooking)
        {
            var doctor = await _context.Doctor.FirstOrDefaultAsync(x => x.Id == newBooking.DoctorId);

            if (!doctor.IsAvailable(newBooking.StartTime, newBooking.EndTime))
            {
                return ValidationProblem(new ValidationProblemDetails
                {
                    Detail = "Doctor is not available for booking"
                });
            }

            var patient = await _context.Patient.FirstOrDefaultAsync(x => x.Id == newBooking.PatientId);
            var myBooking = new Order
            {
                Id = new Guid(),
                StartTime = newBooking.StartTime,
                EndTime = newBooking.EndTime,
                PatientId = newBooking.PatientId,
                DoctorId = newBooking.DoctorId,
                Patient = patient,
                Doctor = doctor,
                SurgeryType = (int) patient.Clinic.SurgeryType
            };

            await _context.Order.AddAsync(myBooking);
            await _context.SaveChangesAsync();

            return Ok(new {myBooking.Id});
        }

        [HttpPut("{bookingId}/status")]
        public async Task<IActionResult> ChangeStatus([FromRoute] Guid bookingId, [FromBody]OrderStatus status)
        {
            var order = await _context.Order.SingleOrDefaultAsync(x => x.Id == bookingId);
            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class NewBooking
        {
            public Guid Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
        }
    }
}
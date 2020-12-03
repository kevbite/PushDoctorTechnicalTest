using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public IActionResult GetPatientNextAppointnemtn(long identificationNumber)
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

        [HttpPost()]
        public IActionResult AddBooking(NewBooking newBooking)
        {
            var doctor = _context.Doctor.FirstOrDefault(x => x.Id == newBooking.DoctorId);
            
            if (doctor.IsAvailable(newBooking.StartTime, newBooking.EndTime))
            {
                return ValidationProblem(new ValidationProblemDetails
                {
                    Detail = "Doctor is not available for booking"
                });
            }

            var patient = _context.Patient.FirstOrDefault(x => x.Id == newBooking.PatientId);
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

            _context.Order.AddRange(new List<Order> {myBooking});
            _context.SaveChanges();

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

        private static MyOrderResult UpdateLatestBooking(List<Order> bookings2, int i)
        {
            MyOrderResult latestBooking;
            latestBooking = new MyOrderResult();
            latestBooking.Id = bookings2[i].Id;
            latestBooking.DoctorId = bookings2[i].DoctorId;
            latestBooking.StartTime = bookings2[i].StartTime;
            latestBooking.EndTime = bookings2[i].EndTime;
            latestBooking.PatientId = bookings2[i].PatientId;
            latestBooking.SurgeryType = (int) bookings2[i].GetSurgeryType();

            return latestBooking;
        }

        private class MyOrderResult
        {
            public Guid Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
            public int SurgeryType { get; set; }
        }
    }
}
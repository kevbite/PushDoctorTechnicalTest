using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace PDR.PatientBookingApi.FunctionalTests.Bookings
{
    public class CreatingABookingTests
    {
        private PatientBookingHarness _harness;

        [SetUp]
        public void Setup()
        {
            _harness = new PatientBookingHarness();
        }

        [Test]
        public async Task ShouldReturn200OkResponseCode()
        {
            var patient = await _harness.CreatePatient();
            var doctor = await _harness.CreateDoctor();
            var booking = await _harness.CreateBooking(patient, doctor, DateTime.UtcNow, DateTime.UtcNow);
            
            booking.statusCode
                .Should().Be(StatusCodes.Status200OK);
        }
        
        
        [Test]
        public async Task ShouldCreateAnOrder()
        {
            var patient = await _harness.CreatePatient();
            var doctor = await _harness.CreateDoctor();
            var startTime = DateTime.UtcNow;
            var endTime = DateTime.UtcNow;
            await _harness.CreateBooking(patient, doctor, startTime, endTime);

            var order = await _harness.GetOrder(patient, doctor);
            
            order.Should().BeEquivalentTo(new
            {
                StartTime = startTime,
                EndTime = endTime,
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                SurgeryType = (int)patient.Clinic.SurgeryType
            });
        }
    }
}
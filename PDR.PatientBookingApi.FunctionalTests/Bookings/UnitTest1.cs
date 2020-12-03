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
            var booking = await _harness.CreateBooking(patient, doctor);
            
            booking.statusCode
                .Should().Be(StatusCodes.Status200OK);
        }
    }
}
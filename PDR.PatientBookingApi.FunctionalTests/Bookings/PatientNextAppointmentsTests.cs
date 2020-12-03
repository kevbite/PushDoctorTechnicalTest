using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace PDR.PatientBookingApi.FunctionalTests.Bookings
{
    public class PatientNextAppointmentsTests
    {
        private PatientBookingHarness _harness;

        [SetUp]
        public void Setup()
        {
            _harness = new PatientBookingHarness();
        }

        [TearDown]
        public void TearDown()
        {
            _harness.Dispose();
        }

        [Test]
        public async Task ShouldReturn502BadGatewayForPatientWithNoBookings()
        {
            var (statusCode, _) = await _harness.GetNextAppointments(123123);

            statusCode.Should().Be(StatusCodes.Status502BadGateway);
        }
    }
}
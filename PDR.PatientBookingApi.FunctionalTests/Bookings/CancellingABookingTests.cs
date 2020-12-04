using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using PDR.PatientBooking.Data.Models;

namespace PDR.PatientBookingApi.FunctionalTests.Bookings
{
    public class CancellingABookingTests
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
        public async Task ShouldReturn200OkResponseCode()
        {
            var (_, bookingId) = await _harness.CreateBooking(await _harness.CreatePatient(), await _harness.CreateDoctor(), DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(1).AddHours(1));
            
            var statusCode = await _harness.CancelBooking(bookingId);
            
            statusCode.Should().Be(StatusCodes.Status200OK);
        }
        
        [Test]
        public async Task ShouldReturn404NotFoundForBookingsThatDoNotExist()
        {
            var statusCode = await _harness.CancelBooking(Guid.NewGuid());
            
            statusCode.Should().Be(StatusCodes.Status404NotFound);
        }
        
        [Test]
        public async Task ShouldCancelBooking()
        {
            var (_, bookingId) = await _harness.CreateBooking(await _harness.CreatePatient(), await _harness.CreateDoctor(), DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(1).AddHours(1));
            
            await _harness.CancelBooking(bookingId);

            var booking = await _harness.GetOrder(bookingId);

            booking.Status.Should().Be(OrderStatus.Cancelled);
        }

    }
}
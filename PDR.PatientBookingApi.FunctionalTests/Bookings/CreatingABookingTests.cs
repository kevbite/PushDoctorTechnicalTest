using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using PDR.PatientBooking.Data.Models;

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

        [TearDown]
        public void TearDown()
        {
            _harness.Dispose();
        }
        
        [Test]
        public async Task ShouldReturn200OkResponseCode()
        {
            var patient = await _harness.CreatePatient();
            var doctor = await _harness.CreateDoctor();
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);
            var booking = await _harness.CreateBooking(patient, doctor, startTime, endTime);
            
            booking.statusCode
                .Should().Be(StatusCodes.Status200OK);
        }
        
        
        [Test]
        public async Task ShouldCreateAnOrder()
        {
            var patient = await _harness.CreatePatient();
            var doctor = await _harness.CreateDoctor();
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);
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
        
        [Test]
        public async Task ShouldAcceptMultipleBookingsForSameDoctorThatDoNotCrossOver()
        {
            var doctor = await _harness.CreateDoctor();
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);

            var (statusCode1, _) = await _harness.CreateBooking(await _harness.CreatePatient(), doctor, startTime, endTime);

            var startTime2 = endTime.AddHours(1);
            var endTime2 = startTime2.AddHours(1);
            var (statusCode2, _) = await _harness.CreateBooking(await _harness.CreatePatient(), doctor, startTime2, endTime2);

            using var _ = new AssertionScope();
            statusCode1
                .Should().Be(StatusCodes.Status200OK);
            statusCode2
                .Should().Be(StatusCodes.Status200OK);
        }
                
        [Test]
        public async Task ShouldReturn400BadRequestResponseCodeForSameDoctorAtSameTime()
        {
            var doctor = await _harness.CreateDoctor();
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);
            await _harness.CreateBooking(await _harness.CreatePatient(), doctor, startTime, endTime);
            
            var booking = await _harness.CreateBooking(await _harness.CreatePatient(), doctor, startTime, endTime);

            booking.statusCode
                .Should().Be(StatusCodes.Status400BadRequest);
        }
        
                        
        [Test]
        public async Task ShouldReturn400BadRequestResponseCodeForSameDoctorWhereStartTimeOverlaps()
        {
            var doctor = await _harness.CreateDoctor();
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);
            await _harness.CreateBooking(await _harness.CreatePatient(), doctor, startTime, endTime);

            var startTime2 = startTime.AddHours(-0.5);
            var endTime2 = startTime.AddHours(0.5);
            var booking = await _harness.CreateBooking(await _harness.CreatePatient(), doctor, startTime2, endTime2);

            booking.statusCode
                .Should().Be(StatusCodes.Status400BadRequest);
        }
        
        [Test]
        public async Task ShouldReturn400BadRequestResponseCodeForSameDoctorWhereEndTimeOverlaps()
        {
            var doctor = await _harness.CreateDoctor();
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);
            await _harness.CreateBooking(await _harness.CreatePatient(), doctor, startTime, endTime);

            var startTime2 = endTime.AddHours(-0.5);
            var endTime2 = endTime.AddHours(0.5);
            var booking = await _harness.CreateBooking(await _harness.CreatePatient(), doctor, startTime2, endTime2);

            booking.statusCode
                .Should().Be(StatusCodes.Status400BadRequest);
        }
        
        [Test]
        public async Task ShouldReturn400BadRequestResponseCodeForBookingDoctorInThePast()
        {
            var doctor = await _harness.CreateDoctor();
            var startTime = DateTime.UtcNow.AddSeconds(-2);
            var endTime = startTime.AddSeconds(1);
            var (statusCode, _) = await _harness.CreateBooking(await _harness.CreatePatient(), doctor, startTime, endTime);

            statusCode
                .Should().Be(StatusCodes.Status400BadRequest);
        }
    }
}
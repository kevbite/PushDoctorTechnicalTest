using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
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

        [Test]
        public async Task ShouldReturn502BadGatewayForPatientWithBookingsInThePast()
        {
            var patient = await _harness.CreatePatient();
            var startTime = DateTime.Now;
            var endTime = DateTime.Now;
            await _harness.CreateOrder(patient, startTime, endTime, await _harness.CreateDoctor());

            var (statusCode, _) = await _harness.GetNextAppointments(patient.Id);

            statusCode.Should().Be(StatusCodes.Status502BadGateway);
        }
        
        [Test]
        public async Task ShouldReturnBooking()
        {
            var patient = await _harness.CreatePatient();
            var startTime = DateTime.Now.AddHours(1);
            var endTime = DateTime.Now.AddHours(1.2);
            var doctor = await _harness.CreateDoctor();
            var order = await _harness.CreateOrder(patient, startTime, endTime, doctor);

            var (statusCode, body) = await _harness.GetNextAppointments(patient.Id);

            using var _ = new AssertionScope();
            statusCode.Should().Be(StatusCodes.Status200OK);
            body.Value<string>("id").Should().Be(order.Id.ToString());
            body.Value<long>("doctorId").Should().Be(doctor.Id);
            body.Value<DateTime>("startTime").Should().Be(startTime);
            body.Value<DateTime>("endTime").Should().Be(endTime);
        }


        [Test]
        public async Task ShouldReturnClosestBooking()
        {
            var patient = await _harness.CreatePatient();
            var doctor1 = await _harness.CreateDoctor();
            var doctor2 = await _harness.CreateDoctor();
            var order1 = await _harness.CreateOrder(patient,
                DateTime.Now.AddHours(2), DateTime.Now.AddHours(2.2), doctor1);
            var expectedStartTime = DateTime.Now.AddHours(1);
            var expectedEndTime = DateTime.Now.AddHours(1.2);
            var order2 = await _harness.CreateOrder(patient,
                expectedStartTime, expectedEndTime, doctor2);

            var (statusCode, body) = await _harness.GetNextAppointments(patient.Id);

            using var _ = new AssertionScope();
            statusCode.Should().Be(StatusCodes.Status200OK);
            body.Value<string>("id").Should().Be(order2.Id.ToString());
            body.Value<long>("doctorId").Should().Be(doctor2.Id);
            body.Value<DateTime>("startTime").Should().Be(expectedStartTime);
            body.Value<DateTime>("endTime").Should().Be(expectedEndTime);
        }
        
        [Test]
        public async Task ShouldReturnClosestBookingForPatient()
        {
            var patient1 = await _harness.CreatePatient();
            var patient2 = await _harness.CreatePatient();
            var doctor1 = await _harness.CreateDoctor();
            var doctor2 = await _harness.CreateDoctor();
            var order1 = await _harness.CreateOrder(patient1,
                DateTime.Now.AddHours(1), DateTime.Now.AddHours(1.2), doctor1);
            var expectedStartTime = DateTime.Now.AddHours(2);
            var expectedEndTime = DateTime.Now.AddHours(2.2);
            var order2 = await _harness.CreateOrder(patient2,
                expectedStartTime, expectedEndTime, doctor2);

            var (statusCode, body) = await _harness.GetNextAppointments(patient2.Id);

            using var _ = new AssertionScope();
            statusCode.Should().Be(StatusCodes.Status200OK);
            body.Value<string>("id").Should().Be(order2.Id.ToString());
        }
        
        [Test]
        public async Task ShouldReturnClosestActiveBooking()
        {
            var patient = await _harness.CreatePatient();
            var doctor1 = await _harness.CreateDoctor();
            var doctor2 = await _harness.CreateDoctor();
            var order1 = await _harness.CreateOrder(patient,
                DateTime.Now.AddHours(2), DateTime.Now.AddHours(2.2), doctor1);
            var expectedStartTime = DateTime.Now.AddHours(1);
            var expectedEndTime = DateTime.Now.AddHours(1.2);
            var order2 = await _harness.CreateOrder(patient,
                expectedStartTime, expectedEndTime, doctor2);

            await _harness.CancelBooking(order2.Id);
            
            var (statusCode, body) = await _harness.GetNextAppointments(patient.Id);

            using var _ = new AssertionScope();
            statusCode.Should().Be(StatusCodes.Status200OK);
            body.Value<string>("id").Should().Be(order1.Id.ToString());
        }
    }
}
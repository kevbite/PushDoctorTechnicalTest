using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;

namespace PDR.PatientBookingApi.FunctionalTests.Bookings
{
    public class PatientBookingHarness : IDisposable
    {
        private WebApplicationFactory<Startup> _factory
            = new WebApplicationFactory<Startup>();

        public async Task<Patient> CreatePatient()
        {
            using var scope = _factory.Services.CreateScope();
            await using var setupContext = scope.ServiceProvider.GetRequiredService<PatientBookingContext>();
            var entity = await setupContext.Patient.AddAsync(new Patient
            {
                Id = UniqueSequence.Next(),
                Clinic = new Clinic
                {
                    SurgeryType = SurgeryType.SystemTwo
                }
            });
            await setupContext.SaveChangesAsync();

            return entity.Entity;
        }

        public async Task<Doctor> CreateDoctor()
        {
            using var scope = _factory.Services.CreateScope();
            await using var setupContext = scope.ServiceProvider.GetRequiredService<PatientBookingContext>();
            var entity = await setupContext.Doctor.AddAsync(new Doctor
            {
                Id = UniqueSequence.Next()
            });
            await setupContext.SaveChangesAsync();

            return entity.Entity;
        }
        
        public async Task<Order> CreateOrder(Patient patient,
            DateTime startTime, DateTime endTime, Doctor doctor)
        {
            using var scope = _factory.Services.CreateScope();
            await using var setupContext = scope.ServiceProvider.GetRequiredService<PatientBookingContext>();
            var entity = await setupContext.Order.AddAsync(new Order
            {
                Id = Guid.NewGuid(),
                StartTime = startTime,
                EndTime = endTime,
                PatientId = patient.Id,
                DoctorId = doctor.Id
            });
            await setupContext.SaveChangesAsync();

            return entity.Entity;
        }

        public async Task<(HttpStatusCode statusCode, Guid bookingId)> CreateBooking(Patient patient, Doctor doctor,
            DateTime startTime, DateTime endTime)
        {
            using var client = _factory.CreateClient();
            var jsonRequest = $@"{{
  ""id"": ""{Guid.NewGuid()}"",
  ""startTime"": ""{startTime:O}"",
  ""endTime"": ""{endTime:O}"",
  ""patientId"": {patient.Id},
  ""doctorId"": {doctor.Id}
}}";
            using var responseMessage = await client.PostAsync("api/booking",
                new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            var jsonBody = await responseMessage.Content.ReadAsStringAsync();

            Guid.TryParse(JToken.Parse(jsonBody).Value<string>("id"), out var bookingId);
            return (responseMessage.StatusCode, bookingId);
        }

        public async Task<Order> GetOrder(Patient patient, Doctor doctor)
        {
            using var scope = _factory.Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<PatientBookingContext>();

            return await context.Order.FirstOrDefaultAsync(
                x => x.PatientId == patient.Id
                     && x.DoctorId == doctor.Id);
        }

        public async Task<Order> GetOrder(Guid bookingId)
        {
            using var scope = _factory.Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<PatientBookingContext>();

            return await context.Order.FirstOrDefaultAsync(x => x.Id == bookingId);
        }

        public void Dispose()
        {
            _factory.Dispose();
        }

        public async Task<HttpStatusCode> CancelBooking(Guid bookingId)
        {
            using var client = _factory.CreateClient();

            using var responseMessage = await client.PutAsync($"api/booking/{bookingId}/status",
                new StringContent(@"""Cancelled""", Encoding.UTF8, "application/json"));

            return responseMessage.StatusCode;
        }

        public async Task<(HttpStatusCode statusCode, JToken body)> GetNextAppointments(long patientId)
        {
            using var client = _factory.CreateClient();

            using var responseMessage = await client.GetAsync($"api/booking/patient/{patientId}/next");

            var json = await responseMessage.Content.ReadAsStringAsync();
            return (responseMessage.StatusCode, JToken.Parse(json));
        }
    }
}
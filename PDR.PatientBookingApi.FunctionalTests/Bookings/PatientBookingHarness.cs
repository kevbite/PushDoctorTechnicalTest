using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;

namespace PDR.PatientBookingApi.FunctionalTests.Bookings
{
    public class PatientBookingHarness
    {
        private WebApplicationFactory<Startup> _webApplicationFactory
            = new WebApplicationFactory<Startup>();

        public async Task<Patient> CreatePatient()
        {
            using var scope = _webApplicationFactory.Services.CreateScope();
            await using var setupContext = scope.ServiceProvider.GetRequiredService<PatientBookingContext>();
            var entity = await setupContext.Patient.AddAsync(new Patient
            {
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
            using var scope = _webApplicationFactory.Services.CreateScope();
            await using var setupContext = scope.ServiceProvider.GetRequiredService<PatientBookingContext>();
            var entity = await setupContext.Doctor.AddAsync(new Doctor());
            await setupContext.SaveChangesAsync();

            return entity.Entity;
        }

        public async Task<(HttpStatusCode statusCode, string body)> CreateBooking(Patient patient, Doctor doctor)
        {
            using var client = _webApplicationFactory.CreateClient();
            var json = $@"{{
  ""id"": ""{Guid.NewGuid()}"",
  ""startTime"": ""{DateTime.UtcNow:O}"",
  ""endTime"": ""{DateTime.UtcNow:O}"",
  ""patientId"": {patient.Id},
  ""doctorId"": {doctor.Id}
}}";
            using var responseMessage = await client.PostAsync("api/booking", new StringContent(json, Encoding.UTF8, "application/json"));

            return (responseMessage.StatusCode, await responseMessage.Content.ReadAsStringAsync());
        }
    }
}
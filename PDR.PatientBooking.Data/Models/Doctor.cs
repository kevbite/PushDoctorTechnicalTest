using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PDR.PatientBooking.Data.Models
{
    public class Doctor
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Gender { get; set; }
        public string Email { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public DateTime Created { get; set; }

        public bool IsAvailable(DateTime startTime, DateTime endTime)
        {
            if (startTime <= DateTime.UtcNow)
                return false;
            
            return !Orders.Any(x => x.StartTime >= startTime && x.StartTime <= endTime
                                   || x.EndTime >= startTime && x.EndTime <= endTime);
        }
    }
}

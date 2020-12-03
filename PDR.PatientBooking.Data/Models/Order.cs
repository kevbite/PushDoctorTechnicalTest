using System;

namespace PDR.PatientBooking.Data.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int SurgeryType { get; set; }
        public virtual long PatientId { get; set; }
        public virtual long DoctorId { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Active;
    }

    public enum OrderStatus
    {
        Active = 1,
        Cancelled = 2,
    }
}

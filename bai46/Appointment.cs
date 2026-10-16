using System;

namespace bai49
{
    public class Appointment
    {
        public string AppointmentId { get; set; }
        public string PatientName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Doctor { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Chờ khám, Đang khám, Hoàn tất, Đã hủy
    }
}
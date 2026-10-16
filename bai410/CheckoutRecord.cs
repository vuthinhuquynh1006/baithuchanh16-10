using System;

namespace bai410
{
    public class CheckoutRecord
    {
        public string TicketId { get; set; }       // Mã phiếu
        public string StudentId { get; set; }      // Mã sinh viên
        public string StudentName { get; set; }    // Họ tên sinh viên
        public string EquipmentName { get; set; }  // Thiết bị
        public int Quantity { get; set; }          // Số lượng
        public DateTime BorrowDate { get; set; }   // Ngày mượn
        public DateTime DueDate { get; set; }      // Hạn trả
        public DateTime? ReturnDate { get; set; }  // Ngày trả thực tế (có thể null nếu chưa trả)
        public string Status { get; set; }         // Đang mượn, Đã trả, Quá hạn
    }
}
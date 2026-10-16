using System;

namespace bai411
{
    public class ServiceOrder
    {
        public string OrderID { get; set; }          // Mã đơn
        public string CustomerName { get; set; }     // Tên khách hàng
        public string Phone { get; set; }            // Số điện thoại
        public string DeviceType { get; set; }       // Loại thiết bị
        public string ErrorDescription { get; set; } // Mô tả lỗi
        public DateTime ReceiveDate { get; set; }    // Ngày tiếp nhận
        public decimal EstimatedCost { get; set; }   // Chi phí dự kiến
        public string Status { get; set; }           // Trạng thái
        public string ImagePath { get; set; }        // Đường dẫn ảnh thiết bị
    }
}
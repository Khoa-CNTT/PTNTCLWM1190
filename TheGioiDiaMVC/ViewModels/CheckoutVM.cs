﻿namespace TheGioiDiaMVC.ViewModels
{
    public class CheckoutVM
    {
        public bool GiongKhachHang { get; set; }
        public string? HoTen { get; set; }
        public string? DiaChi { get; set; }
        public string? DienThoai { get; set; }
        public string? GhiChu { get; set; }
        public string? CouponCode { get; set; } // Mã giảm giá
    }
}

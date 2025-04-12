namespace TheGioiDiaMVC.ViewModels
{
    public class HoaDonVM
    {
        public int MaHd { get; set; } 
        public DateTime NgayDat { get; set; } 
        public string HoTen { get; set; }
        public string DiaChi { get; set; }
        public string CachThanhToan { get; set; }

        public string DienThoai { get; set; }

        public int MaTrangThai { get; set; }
        public string TenTrangThai { get; set; }
        public double ThanhTien { get; set; } 
    }
}

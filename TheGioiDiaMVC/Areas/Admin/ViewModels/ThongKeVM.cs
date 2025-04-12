namespace TheGioiDiaMVC.Areas.Admin.ViewModels
{
    public class ThongKeVM
    {
        public double DoanhThuHomNay { get; set; }
        public int SoDonHoanThanhHomNay { get; set; }
        public int SoLuongSanPhamBanHomNay { get; set; }
        public double DoanhThuThangNay { get; set; }
        public int SoDonHoanThanhThangNay { get; set; }
        public int SoLuongSanPhamBanThangNay { get; set; }
        public List<ThongKeThangVM> ThongKeTheoThang { get; set; } = new List<ThongKeThangVM>();
        public List<SanPhamBanChayVM> TopSanPhamBanChay { get; set; }
    }
}

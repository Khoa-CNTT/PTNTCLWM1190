namespace TheGioiDiaMVC.ViewModels
{
    public class DuyetDiaVM
    {
        public int MaGuiDia { get; set; }
        public string TenDia { get; set; }
        public float Gia { get; set; }
        public string HinhAnh { get; set; }
        public int TinhTrang { get; set; }
        public string TenNguoiGui { get; set; }
        public byte TrangThai { get; set; } = 0; // Mặc định là 0 (Chưa duyệt)
    }
}

using System.ComponentModel.DataAnnotations;

namespace TheGioiDiaMVC.ViewModels
{
    public class DiaHiemVM
    {
        public int MaGuiDia { get; set; }
        public string TenDia { get; set; }
        public float Gia { get; set; }
        public string HinhAnh { get; set; }
        public int TinhTrang { get; set; }
        public string TenNguoiGui { get; set; }
        public byte TrangThai { get; set; } = 0; // Mặc định là 0 (Chưa duyệt)
    }
    public class ThemDiaHiemVM
    {
        [Required]
        public string TenDia { get; set; }

        [Required]
        [Range(1000, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 1000 VNĐ")]
        public float Gia { get; set; }

        public string MoTa { get; set; }

        [Required]
        public IFormFile HinhAnh { get; set; }

        public IFormFile? HinhMoTa { get; set; }

        [Required]
        public int TinhTrang { get; set; }
    }
}

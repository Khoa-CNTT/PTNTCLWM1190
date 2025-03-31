using System;
using System.ComponentModel.DataAnnotations;

namespace TheGioiDiaMVC.ViewModels
{
    public class ChiTietDiaHiemVM
    {
        public int MaGuiDia { get; set; }

        [Required]
        [MaxLength(255)]
        public string TenDia { get; set; }

        [Required]
        public float Gia { get; set; }

        public string MoTa { get; set; }

        public string HinhAnh { get; set; }
        public string HinhMoTa { get; set; }

        public int TinhTrang { get; set; }

        public DateTime NgayGui { get; set; }

        // Thông tin người gửi
        public string MaKH { get; set; } // Mã khách hàng
        public string TenNguoiGui { get; set; } // Tên khách hàng
        public string EmailNguoiGui { get; set; }
        public string SoDienThoaiNguoiGui { get; set; }
        public string DiaChiNguoiGui { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGioiDiaMVC.ViewModels
{
    public class HangHoaVM
    {
        [Key]
        public int MaHh { get; set; }
        public string TenHH { get; set; }
        public string Hinh { get; set; }
        public double DonGia { get; set; }
        public string MoTaNgan { get; set; }
        public string MoTa { get; set; } 
        public string TenLoai { get; set; }
        public int MaLoai { get; set; }
        public string MaNcc { get; set; }

        public DateTime NgaySX { get; set; } 
        public double GiamGia { get; set; } = 0; 
        public int SoLanXem { get; set; } = 0; 
    }

    public class ChiTietHangHoaVM
    {
        public int MaHh { get; set; }
        public string TenHH { get; set; }
        public string Hinh { get; set; }
        public string Hinh2 { get; set; }
        public string Hinh3 { get; set; }
        public double DonGia { get; set; }
        public string MoTaNgan { get; set; }
        public string TenLoai { get; set; }
        public string ChiTiet { get; set; }
        public int DiemDanhGia { get; set; }
        public int SoLuongTon { get; set; }
        public string? SpotifyUri { get; set; }

        public List<HangHoaVM> SanPhamLienQuan { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGioiDiaMVC.Data;

public class HangHoa
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaHh { get; set; }

    [Required(ErrorMessage = "Tên hàng hóa không được để trống")]
    public string TenHh { get; set; } = null!;

    public string? TenAlias { get; set; }

    [Required(ErrorMessage = "Mã loại không được để trống")]
    [ForeignKey("MaLoaiNavigation")]  // 🔹 Thêm ForeignKey
    public int MaLoai { get; set; }

    public string? MoTaDonVi { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải là số dương")]
    public double? DonGia { get; set; }

    public string? Hinh { get; set; }

    public DateTime NgaySx { get; set; }

    public double GiamGia { get; set; }

    public int SoLanXem { get; set; }

    public string? MoTa { get; set; }

    [Required(ErrorMessage = "Mã NCC không được để trống")]
    [ForeignKey("MaNccNavigation")]  // 🔹 Thêm ForeignKey
    public string MaNcc { get; set; } = null!;

    public string? SpotifyUri { get; set; }
    public string? Hinh2 { get; set; }
    public string? Hinh3 { get; set; }

    public virtual ICollection<ChiTietHd> ChiTietHds { get; set; } = new List<ChiTietHd>();

    // 🚀 Không để Required trên Navigation Property
    public virtual Loai? MaLoaiNavigation { get; set; }

    public virtual NhaCungCap? MaNccNavigation { get; set; }

}

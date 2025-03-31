using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TheGioiDiaMVC.Data
{
    [Table("GuiDiaHiems")]
    public class GuiDiaHiem
    {
        [Key]
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


        [Required]
        public string MaKH { get; set; } // Phải trùng kiểu dữ liệu với SQL Server

        public DateTime NgayGui { get; set; } = DateTime.Now;

        public byte TrangThai { get; set; } = 0;

        // Khóa ngoại
        [ForeignKey("MaKH")]
        public virtual KhachHang KhachHang { get; set; }
    }

}

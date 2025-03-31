using System.ComponentModel.DataAnnotations;

namespace TheGioiDiaMVC.ViewModels
{
    public class DangNhapVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [MaxLength(20,ErrorMessage ="Tối đa 20 kí tự")]
        public string TenDangNhap { get; set; }
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }
    }
}

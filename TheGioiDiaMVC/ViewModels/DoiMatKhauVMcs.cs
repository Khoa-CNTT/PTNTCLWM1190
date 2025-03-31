using System.ComponentModel.DataAnnotations;

namespace TheGioiDiaMVC.ViewModels
{
    public class DoiMatKhauVM
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        public string MatKhauCu { get; set; } // Đổi tên từ MatKhauHienTai thành MatKhauCu cho thống nhất với View

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        public string MatKhauMoi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        public string XacNhanMatKhauMoi { get; set; } // Đổi tên từ NhapLaiMatKhauMoi thành XacNhanMatKhauMoi
    }
}

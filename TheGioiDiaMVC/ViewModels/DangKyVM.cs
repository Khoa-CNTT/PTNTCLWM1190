using System.ComponentModel.DataAnnotations;

namespace TheGioiDiaMVC.ViewModels
{
    public class DangKyVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Mã khách hàng không được để trống")]
        [MaxLength(20, ErrorMessage = "Mã khách hàng không được quá 20 ký tự")]
        public string MaKh { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]

        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        public string NhapLaiMatKhau { get; set; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(50, ErrorMessage = "Họ tên không được quá 50 ký tự")]
        public string HoTen { get; set; }


        [Display(Name = "Giới tính")]
        public bool GioiTinh { get; set; } = true;
        [DataType(DataType.Date)]       
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [Display(Name = "Địa chỉ")]
        [MaxLength(60, ErrorMessage = "Địa chỉ không được quá 60 ký tự")]
        public string DiaChi { get; set; }

        [Display(Name = "Điện thoại")]
        [Required(ErrorMessage = "Điện thoại không được để trống")]
        public string DienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Display(Name = "Hình")]
        public string? Hinh { get; set; }

    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace TheGioiDiaMVC.ViewModels
{
    public class HoSoVM
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string DienThoai { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public string? Hinh { get; set; } 

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; } 

        [DataType(DataType.Password)]
        public string? MatKhau { get; set; } 
    }
}

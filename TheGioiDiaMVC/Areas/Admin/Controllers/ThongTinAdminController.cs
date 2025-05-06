using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.Helpers;
using TheGioiDiaMVC.ViewModels;

namespace TheGioiDiaMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "NhanVien,Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class ThongTinAdminController : Controller
    {
        private readonly TheGioiDiaContext db;
        private readonly IMapper _mapper;

        public ThongTinAdminController(TheGioiDiaContext context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }

        #region xem thông tin cá nhân
        public IActionResult Index()
        {
            if (User.IsInRole("KhachHang"))
            {
                return RedirectToAction("Index", "HangHoa");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                ModelState.AddModelError("", "Không tìm thấy email người dùng.");
                return RedirectToAction("DangNhap");
            }

            var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.Email == userEmail);

            if (khachHang == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin người dùng với email này.");
                return RedirectToAction("DangNhap");
            }

            var model = _mapper.Map<HoSoVM>(khachHang);
            return View(model);
        }
        #endregion

        #region Hồ sơ Admin
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> HoSoAdmin()
        {
            if (User.IsInRole("KhachHang"))
            {
                return RedirectToAction("Index", "HangHoa");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                ModelState.AddModelError("", "Không tìm thấy email người dùng.");
                return RedirectToAction("DangNhap", "KhachHang");
            }

            var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.Email == userEmail);

            if (khachHang == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin người dùng với email này.");
                return RedirectToAction("DangNhap", "KhachHang");
            }

            var model = _mapper.Map<HoSoVM>(khachHang);
            return View(model);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> HoSoAdmin(HoSoVM model, IFormFile? Hinh, string? MatKhauHienTai)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                ModelState.AddModelError("", "Không tìm thấy email người dùng.");
                return RedirectToAction("DangNhap", "KhachHang");
            }

            var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.Email == userEmail);

            if (khachHang == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin người dùng với email này.");
                return RedirectToAction("DangNhap", "KhachHang");
            }

            bool thayDoiEmail = model.Email != khachHang.Email;
            bool thayDoiDienThoai = model.DienThoai != khachHang.DienThoai;

            if ((thayDoiEmail || thayDoiDienThoai) && string.IsNullOrEmpty(MatKhauHienTai))
            {
                ModelState.AddModelError("", "Vui lòng nhập mật khẩu hiện tại để thay đổi Email hoặc Số điện thoại.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(MatKhauHienTai) && khachHang.MatKhau != MatKhauHienTai.ToMd5Hash(khachHang.RandomKey))
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không chính xác.");
                return View(model);
            }

            if (thayDoiEmail)
            {
                khachHang.Email = model.Email;
            }

            if (ModelState.IsValid)
            {
                khachHang.HoTen = model.HoTen;
                khachHang.DienThoai = model.DienThoai;
                khachHang.DiaChi = model.DiaChi;

                if (Hinh != null)
                {
                    khachHang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                }

                db.SaveChanges();

                if (thayDoiEmail)
                {
                    var identity = (ClaimsIdentity)User.Identity;
                    identity.RemoveClaim(identity.FindFirst(ClaimTypes.Email)); // Xóa claim cũ
                    identity.AddClaim(new Claim(ClaimTypes.Email, model.Email)); // Thêm claim mới

                    // Cập nhật lại user email trong session
                    HttpContext.Session.SetString("UserEmail", model.Email);
                }

                return RedirectToAction("Index", "ThongTinAdmin", new { thongBao = "capnhatthanhcong" });
            }

            return View(model);
        }


        #endregion

        #region Đổi mật khẩu Admin
        [Authorize]
        [HttpGet]
        public IActionResult DoiMatKhauAdmin()
        {
            return View(new DoiMatKhauVM());
        }

        [Authorize]
        [HttpPost]
        public IActionResult DoiMatKhauAdmin(DoiMatKhauVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.Email == userEmail);

            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản. Vui lòng đăng nhập lại.";
                return RedirectToAction("DangNhap");
            }

            if (khachHang.MatKhau != model.MatKhauCu.ToMd5Hash(khachHang.RandomKey))
            {
                ModelState.AddModelError("MatKhauCu", "Mật khẩu hiện tại không chính xác.");
                return View(model);
            }

            if (model.MatKhauMoi != model.XacNhanMatKhauMoi)
            {
                ModelState.AddModelError("XacNhanMatKhauMoi", "Mật khẩu mới nhập lại không khớp.");
                return View(model);
            }

            khachHang.MatKhau = model.MatKhauMoi.ToMd5Hash(khachHang.RandomKey);
            db.SaveChanges();

            return RedirectToAction("Index", "ThongTinAdmin", new { thongBao = "doimatkhauthanhcong" });

        }
        #endregion

    }
}

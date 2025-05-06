using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.Helpers;
using TheGioiDiaMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TheGioiDiaMVC.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly TheGioiDiaContext db;
        private readonly IMapper _mapper;

        public KhachHangController(TheGioiDiaContext context ,IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }
        #region Đăng ký
        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangKy(DangKyVM model, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                if (model.MatKhau != model.NhapLaiMatKhau)
                {
                    ModelState.AddModelError("NhapLaiMatKhau", "Mật khẩu nhập lại không khớp.");
                    return View(model);
                }

                try
                {
                    var khachhang = _mapper.Map<KhachHang>(model);
                    khachhang.RandomKey = MyUtil.GenerateRamDomKey();
                    khachhang.MatKhau = model.MatKhau.ToMd5Hash(khachhang.RandomKey);
                    khachhang.HieuLuc = true;
                    khachhang.VaiTro = 0;

                    if (Hinh != null)
                    {
                        khachhang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                    }

                    db.Add(khachhang);
                    db.SaveChanges();

                    return RedirectToAction("Index", "HangHoa", new { thongBao = "DangKyThanhCong" });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            return View(model);
        }

        #endregion


        #region Đăng nhập
        [HttpGet]
        public IActionResult DangNhap(string? duongDanTroLai)
        {
            // Nếu đã đăng nhập và là Quản trị viên, chuyển hướng về trang quản trị
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
            }

            ViewBag.DuongDanTroLai = duongDanTroLai;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(DangNhapVM model, string? duongDanTroLai)
        {
            ViewBag.DuongDanTroLai = duongDanTroLai;
            if (ModelState.IsValid)
            {
                var nguoiDung = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.TenDangNhap);
                if (nguoiDung == null)
                {
                    ModelState.AddModelError("Lỗi", "Không tìm thấy tài khoản!");
                }
                else
                {
                    if (!nguoiDung.HieuLuc)
                    {
                        ModelState.AddModelError("Lỗi", "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                    }
                    else
                    {
                        if (nguoiDung.MatKhau != model.MatKhau.ToMd5Hash(nguoiDung.RandomKey))
                        {
                            ModelState.AddModelError("Lỗi", "Sai tên đăng nhập hoặc mật khẩu.");
                        }
                        else
                        {
                            var claims = new List<Claim> {
                        new Claim(ClaimTypes.Email, nguoiDung.Email),
                        new Claim(ClaimTypes.Name, nguoiDung.HoTen),
                        new Claim(MySetting.CLAIM_CUSTOMERID, nguoiDung.MaKh)
                    };

                            // Gán vai trò theo loại tài khoản
                            if (nguoiDung.VaiTro == 1)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                            }
                            else if (nguoiDung.VaiTro == 2)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "NhanVien"));
                            }
                            else
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "KhachHang"));
                            }

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                               TempData["DangNhapThanhCong"] = "Đăng nhập thành công! Chào mừng bạn đến với CD LOOP.";


                            if (Url.IsLocalUrl(duongDanTroLai))
                            {
                                return Redirect(duongDanTroLai);
                            }
                            else
                            {
                                if (nguoiDung.VaiTro == 1)
                                {
                                    return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
                                }
                                else if (nguoiDung.VaiTro == 2)
                                {
                                    return RedirectToAction("Index", "DanhSachDonHang", new { area = "Admin" });
                                }
                                return RedirectToAction("Index", "HangHoa", new { thongBao = "DangNhapThanhCong" });
                            }
                        }
                    }
                }
            }
            return View();
        }
        #endregion

        #region xem thông tin cá nhân
        [Authorize(Roles = "KhachHang")]
        public IActionResult Thongtincanhan()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.Email == userEmail);

            if (khachHang == null) return RedirectToAction("DangNhap");

            var model = _mapper.Map<HoSoVM>(khachHang);
            return View(model);
        }
        #endregion


        #region Hồ sơ
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> HoSo()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.Email == userEmail);

            if (khachHang == null) return RedirectToAction("DangNhap");

            var model = _mapper.Map<HoSoVM>(khachHang);
            return View(model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> HoSo(HoSoVM model, IFormFile? Hinh, string? MatKhauHienTai)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.Email == userEmail);

            if (khachHang == null) return RedirectToAction("DangNhap");

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

            if (ModelState.IsValid)
            {
                khachHang.HoTen = model.HoTen;
                khachHang.DienThoai = model.DienThoai;
                khachHang.DiaChi = model.DiaChi;
                khachHang.NgaySinh = model.NgaySinh;


                if (thayDoiEmail) khachHang.Email = model.Email;

                if (Hinh != null)
                {
                    khachHang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                }

                db.SaveChanges();
                return RedirectToAction("Thongtincanhan", "KhachHang", new { thongBao = "capnhatthanhcong" });
                return View(model);

            }

            return View(model);
        }

        #endregion

        #region Đổi mật khẩu
        [Authorize]
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            return View(new DoiMatKhauVM());
        }

        [Authorize]
        [HttpPost]
        public IActionResult DoiMatKhau(DoiMatKhauVM model)
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

            return RedirectToAction("Thongtincanhan", "KhachHang", new { thongBao = "doimatkhauthanhcong" });

        }
        #endregion


        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["DangXuatThanhCong"] = "Bạn đã đăng xuất thành công.";

            return RedirectToAction("DangNhap", "KhachHang");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}

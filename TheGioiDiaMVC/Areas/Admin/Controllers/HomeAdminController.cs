using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace TheGioiDiaMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin")]
    [Route("Admin/HomeAdmin")]
    public class HomeAdminController : Controller
    {
        private readonly TheGioiDiaContext db;

        public HomeAdminController(TheGioiDiaContext context)
        {
            db = context;
        }

        [Route("")]
        [Route("Index")]
        [Authorize(Roles = "NhanVien,Admin")]
        public IActionResult Index()
        {
            var homNay = DateTime.Today;
            var thangNay = homNay.Month;
            var namNay = homNay.Year;

            // ---------- Hôm nay ----------
            var hoaDonHomNay = db.HoaDons
                .Include(h => h.ChiTietHds)
                .Where(h => h.NgayDat.Date == homNay && h.MaTrangThai == 3)
                .ToList();

            double doanhThuHomNay = 0;
            int tongSoSanPhamHomNay = 0;

            foreach (var hd in hoaDonHomNay)
            {
                foreach (var ct in hd.ChiTietHds)
                {
                    double thanhTien = (ct.DonGia * ct.SoLuong) * (1 - ct.GiamGia);
                    doanhThuHomNay += thanhTien;
                    tongSoSanPhamHomNay += ct.SoLuong;
                }
            }

            // ---------- Tháng hiện tại ----------
            var hoaDonThangNay = db.HoaDons
                .Include(h => h.ChiTietHds)
                .Where(h => h.NgayDat.Month == thangNay && h.NgayDat.Year == namNay && h.MaTrangThai == 3)
                .ToList();

            double doanhThuThangNay = 0;
            int tongSoSanPhamThangNay = 0;

            foreach (var hd in hoaDonThangNay)
            {
                foreach (var ct in hd.ChiTietHds)
                {
                    double thanhTien = (ct.DonGia * ct.SoLuong) * (1 - ct.GiamGia);
                    doanhThuThangNay += thanhTien;
                    tongSoSanPhamThangNay += ct.SoLuong;
                }
            }

            // ---------- Tổng hợp theo tháng ----------
            var thongKeThangList = db.HoaDons
                .Include(h => h.ChiTietHds)
                .Where(h => h.MaTrangThai == 3)
                .AsEnumerable()
                .GroupBy(h => new { h.NgayDat.Year, h.NgayDat.Month })
                .Select(g =>
                {
                    double tongDoanhThu = 0;
                    int tongSoLuong = 0;

                    foreach (var hd in g)
                    {
                        foreach (var ct in hd.ChiTietHds)
                        {
                            tongDoanhThu += (ct.DonGia * ct.SoLuong) * (1 - ct.GiamGia);
                            tongSoLuong += ct.SoLuong;
                        }
                    }

                    return new ThongKeThangVM
                    {
                        Nam = g.Key.Year,
                        Thang = g.Key.Month,
                        DoanhThu = tongDoanhThu,
                        SoDonHoanThanh = g.Count(),
                        SoLuongSanPhamBan = tongSoLuong
                    };
                })
                .OrderByDescending(x => x.Nam)
                .ThenByDescending(x => x.Thang)
                .ToList();

            // ---------- Top 5 sản phẩm bán chạy ----------
            var topSanPham = db.ChiTietHds
                .Include(c => c.MaHhNavigation)
                .Include(c => c.MaHdNavigation)
                .Where(c => c.MaHdNavigation.MaTrangThai == 3)
                .GroupBy(c => new
                {
                    c.MaHh,
                    c.MaHhNavigation.TenHh,
                    c.MaHhNavigation.Hinh
                })
                .Select(g => new SanPhamBanChayVM
                {
                    MaSp = g.Key.MaHh,
                    TenSp = g.Key.TenHh,
                    HinhAnh = g.Key.Hinh ?? "noimage.jpg",
                    TongSoLuongBan = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.TongSoLuongBan)
                .Take(5)
                .ToList();

            var viewModel = new ThongKeVM
            {
                DoanhThuHomNay = doanhThuHomNay,
                SoDonHoanThanhHomNay = hoaDonHomNay.Count,
                SoLuongSanPhamBanHomNay = tongSoSanPhamHomNay,

                DoanhThuThangNay = doanhThuThangNay,
                SoDonHoanThanhThangNay = hoaDonThangNay.Count,
                SoLuongSanPhamBanThangNay = tongSoSanPhamThangNay,

                ThongKeTheoThang = thongKeThangList,
                TopSanPhamBanChay = topSanPham
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("DangNhap", "KhachHang");
        }
    }
}

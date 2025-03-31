using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;

namespace TheGioiDiaMVC.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly TheGioiDiaContext db;

        public HangHoaController(TheGioiDiaContext context) 
        {
            db = context;
        }
        public IActionResult Index(int? loai, int page = 1, int pageSize = 9, int? maxPrice = null)
        {
            try
            {
                var hangHoas = db.HangHoas.AsQueryable();

                if (loai.HasValue)
                {
                    hangHoas = hangHoas.Where(p => p.MaLoai == loai.Value);
                }

                if (maxPrice.HasValue)
                {
                    hangHoas = hangHoas.Where(p => p.DonGia <= maxPrice.Value);
                }

                var result = hangHoas.Select(p => new HangHoaVM
                {
                    MaHh = p.MaHh,
                    TenHH = p.TenHh,
                    DonGia = p.DonGia ?? 0,
                    Hinh = p.Hinh ?? "",
                    MoTaNgan = p.MoTaDonVi ?? "",
                    TenLoai = p.MaLoaiNavigation.TenLoai
                }).ToPagedList(page, pageSize);

                return View(result);
            }
            catch (Exception ex)
            {
                return Content($"Lỗi: {ex.Message}");
            }
        }
        #region Thanh tìm kiếm
        public IActionResult Search(string query)
        {
            var hangHoas = db.HangHoas.AsQueryable();
            if (query != null)
            {
                hangHoas = hangHoas.Where(p => p.TenHh.Contains(query));
            }
            var result = hangHoas.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai
            });
            return View(result);
        }
        #endregion

        #region Trang chi tiết
        public IActionResult Detail(int id)
        {
            var data = db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .SingleOrDefault(p => p.MaHh == id);

            if (data == null)
            {
                TempData["Message"] = $"Không tìm thấy hàng hóa {id}";
                return Redirect("/404");
            }

            var chiTiet = data.MoTa?.Replace("\n", "<br />") ?? string.Empty;

            // Lấy danh sách sản phẩm cùng loại (trừ sản phẩm hiện tại)
            var relatedProducts = db.HangHoas
                .Where(p => p.MaLoai == data.MaLoai && p.MaHh != id)
                .Take(10) // Giới hạn số sản phẩm
                .Select(p => new HangHoaVM
                {
                    MaHh = p.MaHh,
                    TenHH = p.TenHh,
                    Hinh = p.Hinh ?? string.Empty,
                    DonGia = p.DonGia ?? 0,
                    MoTaNgan = p.MoTaDonVi ?? string.Empty
                })
                .ToList();

            var result = new ChiTietHangHoaVM
            {
                MaHh = data.MaHh,
                TenHH = data.TenHh,
                DonGia = data.DonGia ?? 0,
                Hinh = data.Hinh ?? string.Empty,
                Hinh2 = data.Hinh2 ?? string.Empty,
                Hinh3 = data.Hinh3 ?? string.Empty,
                MoTaNgan = data.MoTaDonVi ?? string.Empty,
                TenLoai = data.MaLoaiNavigation.TenLoai,
                ChiTiet = chiTiet,
                SoLuongTon = 10, // Chưa xong
                SpotifyUri = data.SpotifyUri, // ✅ Thêm dòng này
                SanPhamLienQuan = relatedProducts
            };

            return View(result);
        }
        #endregion
        #region Tìm kiếm theo giá
        public IActionResult SearchGia(int? page, float? giaMin, float? giaMax)
        {
            int pageSize = 9; // Số lượng sản phẩm hiển thị mỗi trang
            int pageNumber = page ?? 1;

            var hangHoas = db.HangHoas.AsQueryable();

            // Lọc theo giá nếu có nhập
            if (giaMin.HasValue)
            {
                hangHoas = hangHoas.Where(h => h.DonGia >= giaMin.Value);
            }
            if (giaMax.HasValue)
            {
                hangHoas = hangHoas.Where(h => h.DonGia <= giaMax.Value);
            }

            var result = hangHoas
                .Select(h => new HangHoaVM
                {
                    MaHh = h.MaHh,
                    TenHH = h.TenHh,
                    DonGia = h.DonGia ?? 0,
                    Hinh = h.Hinh ?? "",
                    MoTaNgan = h.MoTaDonVi ?? "",
                    TenLoai = h.MaLoaiNavigation.TenLoai
                })
                .OrderBy(h => h.DonGia) // Từ giá thấp đến giá cao
                .ToPagedList(pageNumber, pageSize);

            ViewBag.GiaMin = giaMin;
            ViewBag.GiaMax = giaMax;

            return View("Index", result);
        }

        #endregion


    }
}



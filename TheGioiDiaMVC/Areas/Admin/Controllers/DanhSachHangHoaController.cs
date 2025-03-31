using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TheGioiDiaMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class DanhSachHangHoaController : Controller
    {
        private readonly TheGioiDiaContext db;

        public DanhSachHangHoaController(TheGioiDiaContext context)
        {
            db = context;
        }
        [Authorize]
        public IActionResult Index(int? page)
        {
            int pageSize = 7; 
            int pageNumber = page ?? 1; 

            var HangHoa = db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .Select(p => new HangHoaVM
                {
                    MaHh = p.MaHh,
                    TenHH = p.TenHh,
                    DonGia = p.DonGia ?? 0,
                    Hinh = p.Hinh ?? "",
                    MoTaNgan = p.MoTaDonVi ?? "",
                    TenLoai = p.MaLoaiNavigation.TenLoai,
                    MaLoai = p.MaLoai // Thêm MaLoai vào ViewModel
                })
                .OrderBy(p => p.MaHh) 
                .ToPagedList(pageNumber, pageSize); 

            return View(HangHoa);
        }
        #region Search
        public IActionResult Search(string query, int? page)
        {
            int pageSize = 7; 
            int pageNumber = page ?? 1;

            var HangHoa = db.HangHoas.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                HangHoa = HangHoa.Where(p =>
                    p.TenHh.Contains(query) ||                     
                    p.MaHh.ToString().Contains(query) ||           
                    p.MaLoaiNavigation.TenLoai.Contains(query));
            }

            var result = HangHoa.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai,
                MaLoai = p.MaLoai // Thêm MaLoai vào ViewModel

            });

            return View("Index", result.ToPagedList(pageNumber, pageSize));
        }
        #endregion

        #region Danh sách Loại
        public IActionResult Loai(int? page)
        {
            int pageSize = 6;
            int pageNumber = page ?? 1;

            var lo = db.Loais
                .OrderByDescending(lo => lo.MaLoai)
                .ToPagedList(pageNumber, pageSize);

            return View(lo);
        }
        #endregion


        #region Thêm sản phẩm
        [HttpGet]
        public IActionResult Them()
        {
            ViewBag.MaLoai = new SelectList(db.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Them(HangHoa hh)
        {
            if (!ModelState.IsValid)
            {
                // Debug lỗi ModelState
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Lỗi ModelState: {error}");
                }

                ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai", hh.MaLoai);
                ViewBag.MaNcc = new SelectList(db.NhaCungCaps, "MaNcc", "TenCongTy", hh.MaNcc);
                return View(hh);
            }

            try
            {
                // 🚀 Đảm bảo không bind Navigation Property
                hh.MaLoaiNavigation = null;
                hh.MaNccNavigation = null;

                db.HangHoas.Add(hh);
                db.SaveChanges();

                TempData["ThemThanhCong"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu dữ liệu: {ex.Message}");
                TempData["ThemThatBai"] = "Lỗi khi thêm sản phẩm. Vui lòng thử lại!";
            }

            ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai", hh.MaLoai);
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps, "MaNcc", "TenCongTy", hh.MaNcc);
            return View(hh);
        }
        #endregion


        #region Sửa sản phẩm
        [HttpGet]
        public IActionResult Sua(string MaHh)
        {
            ViewBag.MaLoai = new SelectList(db.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");

            if (int.TryParse(MaHh, out int maHhInt))
            {
                var HangHoa = db.HangHoas.Find(maHhInt);
                if (HangHoa == null)
                {
                    return NotFound();
                }
                return View(HangHoa);
            }
            else
            {
                return BadRequest("Invalid MaHh value.");
            }
        }
        [HttpPost]
        public IActionResult Sua(HangHoa HangHoa)
        {
            var existingHangHoa = db.HangHoas.Find(HangHoa.MaHh);
            if (existingHangHoa != null)
            {
                db.Entry(existingHangHoa).CurrentValues.SetValues(HangHoa);
                db.SaveChanges();
                TempData["CapNhatThanhCong"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }

        }
        #endregion

        #region xoá sản phẩm
        public IActionResult Delete(int id)
        {
            var product = db.HangHoas.Find(id);
            if (product != null)
            {
                db.HangHoas.Remove(product);
                db.SaveChanges();
                TempData["XoaThanhCong"] = "Xoá sản phẩm thành công!";
            }
            return RedirectToAction("Index");
        }
        #endregion
    }
}

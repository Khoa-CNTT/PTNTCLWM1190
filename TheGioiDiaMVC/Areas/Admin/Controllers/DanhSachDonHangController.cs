using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.Data;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using X.PagedList.Extensions;
using Microsoft.EntityFrameworkCore;

namespace TheGioiDiaMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "NhanVien,Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class DanhSachDonHangController : Controller
    {
        private readonly TheGioiDiaContext db;

        public DanhSachDonHangController(TheGioiDiaContext context)
        {
            db = context;
        }

        [Authorize]
        public IActionResult Index(int? page)
        {
            int pageSize = 7;
            int pageNumber = page.GetValueOrDefault(1);

            var allowedTrangThai = new[] { -2, -1, 0, 1, 2, 3 ,5};

            var hoaDonList = db.HoaDons
                .Include(hd => hd.MaTrangThaiNavigation)
                .Where(hd => allowedTrangThai.Contains(hd.MaTrangThai))
                .OrderByDescending(hd => hd.NgayDat)
                .ToPagedList(pageNumber, pageSize);

            return View(hoaDonList);
        }


        #region chuyển trạng thái
        [HttpGet]
        public IActionResult CapNhatTrangThai(int id, int trangThai, int page = 1)
        {
            var donHang = db.HoaDons.Find(id);
            if (donHang != null)
            {
                donHang.MaTrangThai = trangThai;
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { page = page });
        }
        #endregion

        #region xem chi tiết
        public IActionResult ChiTiet(int MaHd, int page = 1)
        {
            var dsChiTiet = db.ChiTietHds
                              .Include(ct => ct.MaHdNavigation)
                              .Include(ct => ct.MaHhNavigation)
                              .Where(ct => ct.MaHd == MaHd)
                              .ToList();

            if (dsChiTiet == null || dsChiTiet.Count == 0)
            {
   
                return NotFound();
            }

            ViewBag.Page = page;

            return View(dsChiTiet);
        }
        #endregion

        public IActionResult DonHangDaHoanTat(int? page)
        {
            int pageSize = 7;
            int pageNumber = page ?? 1;

            var hoaDonHoanTat = db.HoaDons
                .Include(hd => hd.MaTrangThaiNavigation)
                .Where(hd => hd.MaTrangThai == 4)
                .OrderByDescending(hd => hd.NgayDat)
                .ToPagedList(pageNumber, pageSize);

            return View("DonHangDaHoanTat", hoaDonHoanTat);
        }

        #region Xoá đơn hàng
        [HttpPost, ActionName("XoaDonHang")]
        public IActionResult XoaDonHangConfirmed(int id)
        {
            var hoaDon = db.HoaDons.Find(id);
            if (hoaDon != null)
            {
                db.HoaDons.Remove(hoaDon);
                db.SaveChanges();
                TempData["Message"] = "Đơn hàng đã được xoá thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
            }
            return RedirectToAction("Index");
        }
        #endregion


    }
}

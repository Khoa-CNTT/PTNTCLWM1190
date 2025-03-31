using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.Data;
using X.PagedList.Extensions;
using X.PagedList;
using TheGioiDiaMVC.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace TheGioiDiaMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class DanhSachKhachHangController : Controller
    {

        private readonly TheGioiDiaContext db;

        public DanhSachKhachHangController(TheGioiDiaContext context)
        {
            db = context;
        }
        public IActionResult Index(int? page)
        {
            int pageSize = 7; 
            int pageNumber = page ?? 1; 

            var kh = db.KhachHangs
                .Where(kh => kh.VaiTro == 0) 
                .OrderByDescending(kh => kh.MaKh)
                .ToPagedList(pageNumber, pageSize);

            return View(kh);
        }


        #region đổi Hiệu lực
        public IActionResult HieuLuc(string id, bool hieuLuc) // Đổi int thành string
        {
            var khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            khachHang.HieuLuc = hieuLuc; 
            db.SaveChanges();
            TempData["CapNhatHieuLuc"] = "Cập nhật hiệu lực thành công!";

            return RedirectToAction("Index");
        }
        #endregion

        //public IActionResult Delete(string id)
        //{
        //    var kh = db.KhachHangs.Find(id);
        //    if (kh != null)
        //    {
        //        db.KhachHangs.Remove(kh);
        //        db.SaveChanges();
        //    }
        //    return RedirectToAction("Index");
        //}


    }
}

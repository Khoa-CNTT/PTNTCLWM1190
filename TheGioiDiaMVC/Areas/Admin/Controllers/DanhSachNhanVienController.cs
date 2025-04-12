using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.Helpers;
using X.PagedList.Extensions;

namespace TheGioiDiaMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class DanhSachNhanVienController : Controller
    {
        private readonly TheGioiDiaContext db;

        public DanhSachNhanVienController(TheGioiDiaContext context)
        {
            db = context;
        }
        public IActionResult Index(int? page)
        {
            int pageSize = 7;
            int pageNumber = page ?? 1;

            var kh = db.KhachHangs
                .Where(kh => kh.VaiTro == 1 || kh.VaiTro == 2)
                .OrderByDescending(kh => kh.MaKh)
                .ToPagedList(pageNumber, pageSize);

            return View(kh);
        }


        #region Thêm
        [HttpGet]
        public IActionResult Them()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Them(KhachHang kh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    kh.RandomKey = MyUtil.GenerateRamDomKey();
                    kh.MatKhau = kh.MatKhau.ToMd5Hash(kh.RandomKey);
                    db.KhachHangs.Add(kh);
                    db.SaveChanges();
                    return RedirectToAction("Index", new { thongBao = "ThemNhanSu" });

                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.ToLower().Contains("duplicate"))
                    {
                        ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu.");
                    }
                }
            }
            return View(kh);
        }
        #endregion

        #region Sửa 
        [HttpGet]
        public IActionResult Sua(string MaKh)
        {
            if (string.IsNullOrEmpty(MaKh))
            {
                return BadRequest("Invalid MaKh value.");
            }

            var khachHang = db.KhachHangs.Find(MaKh);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Sua(KhachHang kh)
        {
            if (ModelState.IsValid)
            {
                var existingKhachHang = db.KhachHangs.AsNoTracking().FirstOrDefault(k => k.MaKh == kh.MaKh);
                if (existingKhachHang == null)
                {
                    return NotFound();
                }
                kh.RandomKey = existingKhachHang.RandomKey;

                if (string.IsNullOrWhiteSpace(kh.MatKhau))
                {
                    kh.MatKhau = existingKhachHang.MatKhau;
                }
                else
                {
                    kh.MatKhau = kh.MatKhau.ToMd5Hash(kh.RandomKey);
                }

                db.Entry(kh).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { thongBao = "SuaNhanSu" });

                return RedirectToAction("Index");
            }
            return View(kh);
        }
        #endregion


        #region xoá nhân sự
        public IActionResult Delete(string id)
        {
            var nv = db.KhachHangs.Find(id);
            if (nv != null)
            {
                db.KhachHangs.Remove(nv);
                db.SaveChanges();
                return RedirectToAction("Index", new { thongBao = "XoaNhanSu" });
            }
            return RedirectToAction("Index");
        }
        #endregion
    }
}

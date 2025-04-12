using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.Data;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace TheGioiDiaMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class DanhSachKhoController : Controller
    {
        private readonly TheGioiDiaContext db;

        public DanhSachKhoController(TheGioiDiaContext context)
        {
            db = context;
        }

        public IActionResult Index(int? page)
        {
            int pageSize = 7;
            int pageNumber = page ?? 1;

            var hh = db.HangHoas
                .OrderBy(p => p.MaHh)
                .ToPagedList(pageNumber, pageSize);

            return View(hh);
        }

        #region sửa số lượng
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null) return NotFound();

            return View(hangHoa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] int soLuongMoi)
        {
            var hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null) return NotFound();

            hangHoa.SoLanXem = soLuongMoi; // Cập nhật số lượng
            await db.SaveChangesAsync();

            return RedirectToAction("Index", new { thongBao = "CapNhatSoLuong" });
            return RedirectToAction(nameof(Index));
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
                return RedirectToAction("Index", new { thongBao = "XoaSanPham" }); ;
            }
            return RedirectToAction("Index","DanhSachKho");
        }
        #endregion
    }
}

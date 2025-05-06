using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.Data;
using X.PagedList.Extensions;

namespace TheGioiDiaMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "NhanVien,Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class DanhSachGuiPhanHoiController : Controller
    {
        private readonly TheGioiDiaContext db;

        public DanhSachGuiPhanHoiController(TheGioiDiaContext context)
        {
            db = context;
        }
        public IActionResult Index(int? page)
        {
            int pageSize = 7;
            int pageNumber = page ?? 1;

            var gy = db.Gopies
                .OrderByDescending(gy => gy.NgayGy)
                .ToPagedList(pageNumber, pageSize);

            return View(gy);
        }
        public IActionResult ChiTiet(string MaGy)
        {
            var gopY = db.Gopies.FirstOrDefault(g => g.MaGy == MaGy);  // Sửa từ id thành MaGy
            if (gopY == null)
            {
                return NotFound();
            }
            return View(gopY);
        }
    }
}

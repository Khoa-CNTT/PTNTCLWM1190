using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.ViewModels;

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
            return View();
        }





        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Lưu thông báo vào TempData
            TempData["SuccessMessage"] = "Bạn đã đăng xuất thành công.";

            return RedirectToAction("DangNhap", "KhachHang");
        }

    }

}

using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.ViewModels;
using System.Linq;

namespace TheGioiDiaMVC.Controllers
{
    public class NhaCungCapController : Controller
    {
        private readonly TheGioiDiaContext db;

        public NhaCungCapController(TheGioiDiaContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            var nhaCungCaps = db.NhaCungCaps
                .Select(ncc => new NhaCungCapVM
                {
                    MaNCC = ncc.MaNcc,
                    TenNCC = ncc.TenCongTy,
                    Logo = ncc.Logo
                })
                .ToList();

            return View(nhaCungCaps);
        }
    }
}

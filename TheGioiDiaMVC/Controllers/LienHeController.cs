using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.ViewModels;
using TheGioiDiaMVC.Data;
using System;

namespace TheGioiDiaMVC.Controllers
{
    public class LienHeController : Controller
    {
        private readonly TheGioiDiaContext _context;

        public LienHeController(TheGioiDiaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new LienHeVM());
        }

        [HttpPost]
        public IActionResult Submit(LienHeVM model)
        {
            if (string.IsNullOrEmpty(model.MaLH))
            {
                model.MaLH = Guid.NewGuid().ToString();

            }

            if (ModelState.IsValid)
            {
                var feedback = new GopY
                {
                    MaGy = model.MaLH,
                    NoiDung = model.NoiDung,
                    HoTen = model.HoTen,
                    Email = model.Email,
                    NgayGy = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Gopies.Add(feedback);
                _context.SaveChanges();

                return RedirectToAction("Index", new { thongBao = "GuiPhanHoiThanhCong" });

            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                TempData["ErrorMessage"] = error.ErrorMessage;
            }

            return View("Index", model);
        }


    }
}

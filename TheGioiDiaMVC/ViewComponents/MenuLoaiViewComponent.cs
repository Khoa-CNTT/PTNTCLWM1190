using TheGioiDiaMVC.Data;
using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.ViewModels;

namespace TheGioiDiaMVC.ViewComponents
{
    public class MenuLoaiViewComponent : ViewComponent
    {
        private readonly TheGioiDiaContext db;

        public MenuLoaiViewComponent(TheGioiDiaContext context) => db = context;

        public IViewComponentResult Invoke()
        {
            var data = db.Loais.Select(lo => new MenuLoaiVM
            {
                MaLoai=lo.MaLoai,
                TenLoai = lo.TenLoai,
                SoLuong = lo.HangHoas.Count
            }) .OrderBy(p => p.MaLoai);

            return View(data);

        }
    }
}

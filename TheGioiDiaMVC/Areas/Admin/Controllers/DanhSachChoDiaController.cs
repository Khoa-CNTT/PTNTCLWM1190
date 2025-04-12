using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.ViewModels;
using X.PagedList.Extensions;

namespace TheGioiDiaMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "NhanVien,Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class DanhSachChoDiaController : Controller
    {
        private readonly TheGioiDiaContext db;

        public DanhSachChoDiaController(TheGioiDiaContext context)
        {
            db = context;
        }
        public IActionResult Index(int? page)
        {
            int pageSize = 6;
            int pageNumber = page ?? 1;

            var gd = db.GuiDiaHiems
                .Include(g => g.KhachHang)
                .Select(g => new DuyetDiaVM
                {
                    MaGuiDia = g.MaGuiDia,
                    TenDia = g.TenDia,
                    Gia = g.Gia,
                    HinhAnh = g.HinhAnh,
                    TinhTrang = g.TinhTrang,
                    TenNguoiGui = g.KhachHang != null ? g.KhachHang.HoTen : "Không xác định",
                    TrangThai = g.TrangThai
                })
                .OrderByDescending(g => g.MaGuiDia)
                .ToPagedList(pageNumber, pageSize);

            return View(gd);
        }


        #region Duyệt tin
        [HttpPost]
        public IActionResult Duyet(int id)
        {
            var dia = db.GuiDiaHiems.Find(id);
            if (dia != null && dia.TrangThai == 0)
            {
                dia.TrangThai = 1; // Chuyển sang Đã Duyệt
                db.SaveChanges();
                return RedirectToAction("Index", new { thongBao = "DuyetThanhCong" });
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region Từ chối tin
        [HttpPost]
        public IActionResult TuChoi(int id)
        {
            var dia = db.GuiDiaHiems.Find(id);
            if (dia != null && dia.TrangThai == 0)
            {
                dia.TrangThai = 2; // Chuyển sang Từ Chối
                db.SaveChanges();
                return RedirectToAction("Index", new { thongBao = "TuChoiDuyet" });
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region ẩn tin
        [HttpPost]
        public IActionResult AnSanPham(int id)
        {
            var dia = db.GuiDiaHiems.Find(id);
            if (dia != null && dia.TrangThai == 1) // Chỉ ẩn nếu sản phẩm đã duyệt
            {
                dia.TrangThai = 3; // Chuyển sang Đã Bán (Ẩn)
                db.SaveChanges();
                return RedirectToAction("Index", new { thongBao = "AnTinThanhCong" });
            }
            return RedirectToAction("Index");
        }
#endregion

        #region trang chi tiết
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var diaHiem = await db.GuiDiaHiems
                .Include(d => d.KhachHang) // Lấy thông tin người gửi
                .Where(d => d.MaGuiDia == id)
                .Select(d => new ChiTietDiaHiemVM
                {
                    MaGuiDia = d.MaGuiDia,
                    TenDia = d.TenDia,
                    Gia = d.Gia,
                    MoTa = d.MoTa,
                    HinhAnh = d.HinhAnh,
                    HinhMoTa = d.HinhMoTa,
                    TinhTrang = d.TinhTrang,
                    NgayGui = d.NgayGui,
                    MaKH = d.MaKH,
                    TenNguoiGui = d.KhachHang != null ? d.KhachHang.HoTen : "Không xác định",
                    EmailNguoiGui = d.KhachHang != null ? d.KhachHang.Email : "Không có email",
                    SoDienThoaiNguoiGui = d.KhachHang != null ? d.KhachHang.DienThoai : "Không có SĐT",
                    DiaChiNguoiGui = d.KhachHang != null ? d.KhachHang.DiaChi :"Không có Địa chỉ"

                })
                .FirstOrDefaultAsync();

            if (diaHiem == null)
            {
                return NotFound();
            }

            return View(diaHiem);
        }
        #endregion
    }
}

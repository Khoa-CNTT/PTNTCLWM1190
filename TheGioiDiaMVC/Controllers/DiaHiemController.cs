using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.Helpers;
using TheGioiDiaMVC.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;

namespace TheGioiDiaMVC.Controllers
{
    public class DiaHiemController : Controller
    {
        private readonly TheGioiDiaContext db;
        private readonly IWebHostEnvironment _env;

        public DiaHiemController(TheGioiDiaContext context, IWebHostEnvironment env)
        {
            db = context;
            _env = env;
        }
        public IActionResult Index(int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            var gd = db.GuiDiaHiems
                .Include(g => g.KhachHang)
                .Where(g => g.TrangThai == 1)
                .Select(g => new DiaHiemVM
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

        #region Tìm kiếm theo Tình trạng
        public IActionResult Search(int? page, int? tinhTrang)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            var gd = db.GuiDiaHiems
                .Include(g => g.KhachHang)
                .Where(g => g.TrangThai == 1);

            // Lọc theo tình trạng nếu có chọn
            if (tinhTrang.HasValue)
            {
                gd = gd.Where(g => g.TinhTrang == tinhTrang.Value);
            }

            var result = gd
                .Select(g => new DiaHiemVM
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

            return View("Index", result); // Trả về View Index
        }
        #endregion

        #region Tìm kiếm theo giá
        public IActionResult SearchGia(int? page, float? giaMin, float? giaMax)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            var gd = db.GuiDiaHiems
                .Include(g => g.KhachHang)
                .Where(g => g.TrangThai == 1);

            // Lọc theo giá nếu có nhập
            if (giaMin.HasValue)
            {
                gd = gd.Where(g => (float)g.Gia >= giaMin.Value);
            }
            if (giaMax.HasValue)
            {
                gd = gd.Where(g => (float)g.Gia <= giaMax.Value);
            }

            var result = gd
                .Select(g => new DiaHiemVM
                {
                    MaGuiDia = g.MaGuiDia,
                    TenDia = g.TenDia,
                    Gia = (float)g.Gia,
                    HinhAnh = g.HinhAnh,
                    TinhTrang = g.TinhTrang,
                    TenNguoiGui = g.KhachHang != null ? g.KhachHang.HoTen : "Không xác định",
                    TrangThai = g.TrangThai
                })
                .OrderByDescending(g => g.MaGuiDia)
                .ToPagedList(pageNumber, pageSize);

            // Lưu giá trị tìm kiếm vào ViewBag để sử dụng lại trong form
            ViewBag.GiaMin = giaMin;
            ViewBag.GiaMax = giaMax;

            return View("Index", result);
        }
        #endregion


        #region chi tiết
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var diaHiem = await db.GuiDiaHiems
                .Include(d => d.KhachHang)
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
                    DiaChiNguoiGui = d.KhachHang != null ? d.KhachHang.DiaChi : "Không có Địa chỉ"
                })
                .FirstOrDefaultAsync();

            if (diaHiem == null)
            {
                return NotFound();
            }

            // Lấy danh sách sản phẩm tương tự
            var sanPhamTuongTu = await db.GuiDiaHiems
                .Where(d => d.MaGuiDia != id && (d.TinhTrang == diaHiem.TinhTrang ||
                                                  (d.Gia >= diaHiem.Gia * 0.8 && d.Gia <= diaHiem.Gia * 1.2)))
                .OrderByDescending(d => d.NgayGui)
                .Take(4)
                .Select(d => new DiaHiemVM
                {
                    MaGuiDia = d.MaGuiDia,
                    TenDia = d.TenDia,
                    Gia = d.Gia,
                    HinhAnh = d.HinhAnh
                })
                .ToListAsync();

            ViewBag.SanPhamTuongTu = sanPhamTuongTu;

            return View(diaHiem);
        }
        #endregion



        #region Thêm
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThemDiaHiemVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Lấy MaKH từ Claims (chứ không dùng Session)
            var maKH = HttpContext.User.Claims.SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để gửi đĩa hiếm.";
                return RedirectToAction("DangNhap", "KhachHang");
            }

            string uniqueFileName = null;
            string uniqueFileMoTa = null;

            // Xử lý hình ảnh chính
            if (model.HinhAnh != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "Hinh/HangHoa");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.HinhAnh.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.HinhAnh.CopyToAsync(fileStream);
                }
            }

            // Xử lý hình ảnh mô tả
            if (model.HinhMoTa != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "Hinh/HangHoa");
                uniqueFileMoTa = Guid.NewGuid().ToString() + "_" + model.HinhMoTa.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileMoTa);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.HinhMoTa.CopyToAsync(fileStream);
                }
            }

            var guiDia = new GuiDiaHiem
            {
                TenDia = model.TenDia,
                Gia = model.Gia,
                MoTa = model.MoTa,
                HinhAnh = uniqueFileName,
                HinhMoTa = uniqueFileMoTa,
                TinhTrang = model.TinhTrang,
                TrangThai = 0,
                NgayGui = DateTime.Now,
                MaKH = maKH 
            };

            db.GuiDiaHiems.Add(guiDia);
            await db.SaveChangesAsync();
            TempData["ThemTin"] = "Thêm tin rao CD thành công! Đĩa của bạn đang chờ duyệt.";

            return RedirectToAction("Index", "DiaHiem");
        }
        #endregion


        #region Quản lý tin
        [Authorize]
        public IActionResult QuanLyTin(int? page)
        {
            // Lấy MaKH từ Claims (không dùng Session)
            var maKH = HttpContext.User.Claims.SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("DangNhap", "KhachHang"); 
            }

            int pageSize = 7;
            int pageNumber = page ?? 1;

            var gd = db.GuiDiaHiems
                .Where(g => g.MaKH == maKH) 
                .Select(g => new DuyetDiaVM
                {
                    MaGuiDia = g.MaGuiDia,
                    TenDia = g.TenDia,
                    Gia = g.Gia,
                    HinhAnh = g.HinhAnh,
                    TinhTrang = g.TinhTrang,
                    TrangThai = g.TrangThai 
                })
                .OrderByDescending(g => g.MaGuiDia)
                .ToPagedList(pageNumber, pageSize);

            return View(gd);
        }
        #endregion

        #region Ẩn tin
        [Authorize]
        public async Task<IActionResult> AnTin(int id)
        {
            var maKH = HttpContext.User.Claims.SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("DangNhap", "KhachHang");
            }

            var tin = await db.GuiDiaHiems.FirstOrDefaultAsync(g => g.MaGuiDia == id && g.MaKH == maKH);
            if (tin == null || tin.TrangThai != 1) // Chỉ ẩn nếu trạng thái là "Đã duyệt"
            {
                return NotFound();
            }

            tin.TrangThai = 3; // Chuyển sang trạng thái "Đã Bán - Ẩn"
            db.GuiDiaHiems.Update(tin);
            await db.SaveChangesAsync();

            TempData["AnThanhCong"] = "Tin đã được ẩn thành công!";
            return RedirectToAction("QuanLyTin");
        }
        #endregion


        #region Hiển Thị lại
        [Authorize]
        public IActionResult HienThiLai(int id)
        {
            var tin = db.GuiDiaHiems.FirstOrDefault(g => g.MaGuiDia == id);
            if (tin != null && tin.TrangThai == 3) 
            {
                tin.TrangThai = 1; 
                db.SaveChanges();
                TempData["HienThiThanhCong"] = "Tin đã được hiển thị lại thành công!";
            }
            return RedirectToAction("QuanLyTin");
        }
        #endregion
    }
}

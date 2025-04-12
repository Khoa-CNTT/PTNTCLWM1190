using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TheGioiDiaMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class DanhSachHangHoaController : Controller
    {
        private readonly TheGioiDiaContext db;

        public DanhSachHangHoaController(TheGioiDiaContext context)
        {
            db = context;
        }
        [Authorize]
        public IActionResult Index(int? page)
        {
            int pageSize = 7; 
            int pageNumber = page ?? 1; 

            var HangHoa = db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .Select(p => new HangHoaVM
                {
                    MaHh = p.MaHh,
                    TenHH = p.TenHh,
                    DonGia = p.DonGia ?? 0,
                    Hinh = p.Hinh ?? "",
                    MoTaNgan = p.MoTaDonVi ?? "",
                    TenLoai = p.MaLoaiNavigation.TenLoai,
                    MaLoai = p.MaLoai // Thêm MaLoai vào ViewModel
                })
                .OrderBy(p => p.MaHh) 
                .ToPagedList(pageNumber, pageSize); 

            return View(HangHoa);
        }
        #region Search
        public IActionResult Search(string query, int? page)
        {
            int pageSize = 7; 
            int pageNumber = page ?? 1;

            var HangHoa = db.HangHoas.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                HangHoa = HangHoa.Where(p =>
                    p.TenHh.Contains(query) ||                     
                    p.MaHh.ToString().Contains(query) ||           
                    p.MaLoaiNavigation.TenLoai.Contains(query));
            }

            var result = HangHoa.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai,
                MaLoai = p.MaLoai // Thêm MaLoai vào ViewModel

            });

            return View("Index", result.ToPagedList(pageNumber, pageSize));
        }
        #endregion



        #region Thêm sản phẩm
        [HttpGet]
        public IActionResult Them()
        {
            ViewBag.MaLoai = new SelectList(db.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Them(HangHoa hh)
        {
            if (!ModelState.IsValid)
            {
                // Debug lỗi ModelState
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Lỗi ModelState: {error}");
                }

                ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai", hh.MaLoai);
                ViewBag.MaNcc = new SelectList(db.NhaCungCaps, "MaNcc", "TenCongTy", hh.MaNcc);
                return View(hh);
            }

            try
            {
                // 🚀 Đảm bảo không bind Navigation Property
                hh.MaLoaiNavigation = null;
                hh.MaNccNavigation = null;

                db.HangHoas.Add(hh);
                db.SaveChanges();

                return RedirectToAction("Index", new { thongBao = "ThemThanhCong" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu dữ liệu: {ex.Message}");
                TempData["ThemThatBai"] = "Lỗi khi thêm sản phẩm. Vui lòng thử lại!";
            }

            ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai", hh.MaLoai);
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps, "MaNcc", "TenCongTy", hh.MaNcc);
            return View(hh);
        }
        #endregion


        #region Sửa sản phẩm
        [HttpGet]
        public IActionResult Sua(string MaHh)
        {
            ViewBag.MaLoai = new SelectList(db.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");

            if (int.TryParse(MaHh, out int maHhInt))
            {
                var HangHoa = db.HangHoas.Find(maHhInt);
                if (HangHoa == null)
                {
                    return NotFound();
                }
                return View(HangHoa);
            }
            else
            {
                return BadRequest("Invalid MaHh value.");
            }
        }
        [HttpPost]
        public IActionResult Sua(HangHoa HangHoa)
        {
            var existingHangHoa = db.HangHoas.Find(HangHoa.MaHh);
            if (existingHangHoa != null)
            {
                db.Entry(existingHangHoa).CurrentValues.SetValues(HangHoa);
                db.SaveChanges();
                return RedirectToAction("Index", new { thongBao = "CapNhatThanhCong" });
            }
            else
            {
                return NotFound();
            }

        }
        #endregion

        #region Xoá sản phẩm
        public IActionResult Delete(int id)
        {
            var product = db.HangHoas.Find(id);

            if (product != null)
            {
                var isInOrder = db.ChiTietHds.Any(c => c.MaHh == id);
                if (isInOrder)
                {
                    // Trả về với thông báo lỗi
                    return RedirectToAction("Index", new { thongBao = "XoaThatBai" });
                }

                db.HangHoas.Remove(product);
                db.SaveChanges();
                return RedirectToAction("Index", new { thongBao = "XoaThanhCong" });
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Danh sách Loại ,Thêm Loại ,Sửa Loại ,Xoá Loại
        public IActionResult Loai(int? page)
        {
            int pageSize = 6;
            int pageNumber = page ?? 1;

            var lo = db.Loais
                .OrderBy(lo => lo.MaLoai)
                .ToPagedList(pageNumber, pageSize);

            return View(lo);
        }

        //THÊM LOẠI
        [HttpGet]
        public IActionResult ThemLoai()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemLoai(Loai loai, IFormFile? HinhUpload)
        {
            if (ModelState.IsValid)
            {
                if (HinhUpload != null && HinhUpload.Length > 0)
                {
                    var fileName = Path.GetFileName(HinhUpload.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Hinh/Loai", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        HinhUpload.CopyTo(stream);
                    }

                    loai.Hinh = fileName;
                }

                db.Loais.Add(loai);
                db.SaveChanges();

                TempData["ThongBao"] = "Thêm loại thành công!";
                return RedirectToAction("Loai","DanhSachHangHoa");
            }

            return View(loai);
        }
        //SỬA LOẠI
        [HttpGet]
        public IActionResult SuaLoai(int id)
        {
            var loai = db.Loais.Find(id);
            if (loai == null)
            {
                return NotFound();
            }

            return View(loai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaLoai(Loai loai, IFormFile? HinhUpload)
        {
            if (ModelState.IsValid)
            {
                var loaiUpdate = db.Loais.Find(loai.MaLoai);
                if (loaiUpdate == null)
                {
                    return NotFound();
                }

                loaiUpdate.TenLoai = loai.TenLoai;

                if (HinhUpload != null && HinhUpload.Length > 0)
                {
                    var fileName = Path.GetFileName(HinhUpload.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Hinh/Loai", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        HinhUpload.CopyTo(stream);
                    }

                    loaiUpdate.Hinh = fileName;
                }

                db.SaveChanges();
                TempData["ThongBao"] = "Cập nhật loại thành công!";
                return RedirectToAction("Loai");
            }

            return View(loai);
        }


        //XOÁ LOẠI
        public IActionResult XoaLoai(int id)
        {
            var loai = db.Loais.Find(id);

            if (loai != null)
            {
                var isInUse = db.HangHoas.Any(h => h.MaLoai == id);
                if (isInUse)
                {
                    return RedirectToAction("Loai", new { thongBao = "XoaThatBai" });
                }

                db.Loais.Remove(loai);
                db.SaveChanges();
                return RedirectToAction("Loai", new { thongBao = "XoaThanhCong" });
            }

            return RedirectToAction("Loai");
        }

        #endregion


        #region Danh sách Nhà cung cấp ,chi tiết ,thêm , xoá
        public IActionResult NhaCungCap()
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

        //THÔNG TIN NHÀ CUNG CẤP
        public IActionResult ChiTietNhaCungCap(string maNCC)
        {
            var ncc = db.NhaCungCaps
                .Where(n => n.MaNcc == maNCC)
                .Select(n => new NhaCungCapVM
                {
                    MaNCC = n.MaNcc,
                    TenNCC = n.TenCongTy,
                    Logo = n.Logo,
                    NguoLienLac = n.NguoiLienLac,
                    Email = n.Email,
                    DienThoai = n.DienThoai,
                    DiaChi = n.DiaChi,
                    MoTa = n.MoTa
                })
                .FirstOrDefault();

            if (ncc == null)
            {
                return NotFound();
            }

            return View(ncc);
        }
        //THÊM NHÀ CUNG CẤP
        [HttpGet]
        public IActionResult ThemNhaCungCap()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemNhaCungCap(NhaCungCap ncc, IFormFile? LogoUpload)
        {
            if (ModelState.IsValid)
            {
                if (db.NhaCungCaps.Any(x => x.MaNcc == ncc.MaNcc))
                {
                    ModelState.AddModelError("MaNcc", "Mã nhà cung cấp đã tồn tại.");
                    return View(ncc);
                }

                if (LogoUpload != null && LogoUpload.Length > 0)
                {
                    var fileName = Path.GetFileName(LogoUpload.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Hinh/NhaCungCap", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        LogoUpload.CopyTo(stream);
                    }

                    ncc.Logo = fileName;
                }

                if (string.IsNullOrEmpty(ncc.Logo))
                {
                    ncc.Logo = "default-logo.png";
                }

                db.NhaCungCaps.Add(ncc);
                db.SaveChanges();

                TempData["ThongBao"] = "Thêm nhà cung cấp thành công!";
                return RedirectToAction("NhaCungCap");
            }

            return View(ncc);
        }
        //XOÁ NHÀ CUNG CẤP
        public IActionResult XoaNhaCungCap(string id)
        {
            var ncc = db.NhaCungCaps.Find(id);

            if (ncc != null)
            {
                var isInUse = db.HangHoas.Any(h => h.MaNcc == id);
                if (isInUse)
                {
                    return RedirectToAction("NhaCungCap", new { thongBao = "XoaThatBai" });
                }

                db.NhaCungCaps.Remove(ncc);
                db.SaveChanges();
                return RedirectToAction("NhaCungCap", new { thongBao = "XoaThanhCong" });
            }

            return RedirectToAction("NhaCungCap");
        }

        #endregion
    }
}

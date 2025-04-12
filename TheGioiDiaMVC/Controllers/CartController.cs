using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.ViewModels;
using TheGioiDiaMVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using TheGioiDiaMVC.Services;

namespace TheGioiDiaMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly TheGioiDiaContext db;
        private readonly IVnPayService _vnPayservice;

        public CartController(TheGioiDiaContext context, PaypalClient paypalClient, IVnPayService vnPayservice)
        {
            _paypalClient = paypalClient;
            db = context;
            _vnPayservice = vnPayservice;
        }
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

        public IActionResult Index()
        {
            return View(Cart);
        }

        #region Thêm vào giỏ hàng
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var hanghoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);

            if (hanghoa == null)
            {
                return RedirectToAction("Index", "HangHoa", new { thongBao = "KhongTimThay" });
            }

            if (hanghoa.SoLanXem <= 0)
            {
                return RedirectToAction("Index", "HangHoa", new { thongBao = "HetHang" });
            }

            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                if (quantity > hanghoa.SoLanXem)
                {
                    return RedirectToAction("Index", "HangHoa", new
                    {
                        thongBao = "HetHang",
                        soLanXem = hanghoa.SoLanXem
                    });
                }
                item = new CartItem
                {
                    MaHh = hanghoa.MaHh,
                    TenHh = hanghoa.TenHh,
                    DonGia = hanghoa.DonGia ?? 0,
                    Hinh = hanghoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                gioHang.Add(item);
            }
            else
            {
                if (item.SoLuong + quantity > hanghoa.SoLanXem)
                {
                    return RedirectToAction("Index", "HangHoa", new
                    {
                        thongBao = "VuotSoLuong",
                        soLanXem = hanghoa.SoLanXem
                    });
                }
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            return RedirectToAction("Index", "HangHoa", new { id = hanghoa.MaHh, thongBao = "ThemGioHang" });

        }
        #endregion

        #region Xoá san pham trong gio hang
        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }
        #endregion

        public IActionResult Success()
        {
            return View();
        }

        #region Thanh toán
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DangNhap", "KhachHang");
            }
            if (Cart.Count == 0)
            {
                return Redirect("/");
            }
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DangNhap", "KhachHang");
            }
            return View(Cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutVM model,string payment = "COD")
        {
            if (ModelState.IsValid)
            {
                if (payment == "Thanh toán VNPay")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = Cart.Sum(p => p.ThanhTien),
                        CreatedDate = DateTime.Now,
                        Description = $"{model.HoTen} {model.DienThoai}",
                        FullName = model.HoTen,
                        OrderId = new Random().Next(1000, 100000)
                    };
                    return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
                }

                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
                if (customerId == null) return RedirectToAction("DangNhap", "KhachHang");

                var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
                if (khachHang == null) return RedirectToAction("DangNhap", "KhachHang");

                var hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = model.HoTen ?? khachHang.HoTen,
                    DiaChi = model.DiaChi ?? khachHang.DiaChi,
                    DienThoai = model.DienThoai ?? khachHang.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "VietelPost",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu
                };

                db.Database.BeginTransaction();
                try
                {
                    db.Add(hoadon);
                    db.SaveChanges();

                    var cthds = new List<ChiTietHd>();
                    foreach (var item in Cart)
                    {
                        var hangHoa = db.HangHoas.SingleOrDefault(h => h.MaHh == item.MaHh);
                        if (hangHoa == null || hangHoa.SoLanXem < item.SoLuong)
                        {
                            db.Database.RollbackTransaction();
                            return RedirectToAction("Index", "HangHoa", new
                            {
                                thongBao = "HetHangg",
                                ten = item.TenHh
                            });

                        }
                        // Trừ số lượng tồn kho
                        hangHoa.SoLanXem -= item.SoLuong;

                        cthds.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            MaHh = item.MaHh,
                            GiamGia = 0
                        });
                    }
                    db.AddRange(cthds);
                    db.SaveChanges();
                    db.Database.CommitTransaction();

                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

                    return View("Success");
                }
                catch
                {
                    db.Database.RollbackTransaction();
                }
            }

            return View(Cart);
        }
        #endregion

        [Authorize]
        public IActionResult PaymentSuccess()
        {
            return View("Success");
        }

        #region Paypal payment
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            // Thông tin đơn hàng gửi qua PayPal
            var tongTien = (Cart.Sum(p => p.ThanhTien) + 30000).ToString();
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();
            double tyGiaUSD = 24000; // Giả sử tỷ giá 1 USD = 24,000 VND
            var tongTienUSD = ((Cart.Sum(p => p.ThanhTien) + 30000) / tyGiaUSD).ToString("0.00");


            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);

                if (response == null || response.status != "COMPLETED")
                {
                    return BadRequest(new { message = "Thanh toán không thành công hoặc chưa hoàn tất!" });
                }

                // Lấy thông tin khách hàng
                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
                if (customerId == null) return Unauthorized();

                var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
                if (khachHang == null) return Unauthorized();

                var hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = khachHang.HoTen,
                    DiaChi = khachHang.DiaChi,
                    DienThoai = khachHang.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = "PayPal",
                    CachVanChuyen = "ViettelPost",
                    MaTrangThai = 1, // Đã thanh toán
                    GhiChu = "Thanh toán qua PayPal"
                };

                db.Database.BeginTransaction();
                try
                {
                    db.HoaDons.Add(hoadon);
                    db.SaveChanges();

                    var cthds = new List<ChiTietHd>();
                    foreach (var item in Cart)
                    {
                        var hangHoa = db.HangHoas.SingleOrDefault(h => h.MaHh == item.MaHh);
                        if (hangHoa == null || hangHoa.SoLanXem < item.SoLuong)
                        {
                            db.Database.RollbackTransaction();
                            return BadRequest(new { message = $"Sản phẩm {item.TenHh} đã hết hàng hoặc không đủ số lượng!" });
                        }

                        // Trừ số lượng tồn kho
                        hangHoa.SoLanXem -= item.SoLuong;

                        cthds.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            MaHh = item.MaHh,
                            GiamGia = 0
                        });
                    }

                    db.ChiTietHds.AddRange(cthds);
                    db.SaveChanges();
                    db.Database.CommitTransaction();

                    // Xóa giỏ hàng sau khi thanh toán thành công
                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    db.Database.RollbackTransaction();
                    return BadRequest(new { message = "Lỗi khi lưu đơn hàng!", error = ex.GetBaseException().Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi trong quá trình thanh toán!", error = ex.GetBaseException().Message });
            }
        }


        #endregion

        #region TheoDoiDonHang
        [Authorize]
        public IActionResult TheoDoiDonHang(int? page, int? trangThai)
        {
            var maKH = HttpContext.User.Claims.SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("DangNhap", "KhachHang");
            }

            int pageSize = 7;
            int pageNumber = page ?? 1;

            var query = db.HoaDons
                .Include(hd => hd.MaTrangThaiNavigation)
                .Include(hd => hd.ChiTietHds)
                .Where(hd => hd.MaKh == maKH);

            var soLuongTatCa = query.Count();
            var soLuongChuaGiao = query.Where(hd => hd.MaTrangThai == 0 || hd.MaTrangThai == 1).Count();
            var soLuongDangVanChuyen = query.Where(hd => hd.MaTrangThai == 2).Count();
            var soLuongHoanThanh = query.Where(hd => hd.MaTrangThai == 3).Count();
            var soLuongHuy = query.Where(hd => hd.MaTrangThai == -1).Count();

            ViewBag.SoLuongTatCa = soLuongTatCa;
            ViewBag.SoLuongChuaGiao = soLuongChuaGiao;
            ViewBag.SoLuongDangVanChuyen = soLuongDangVanChuyen;
            ViewBag.SoLuongHoanThanh = soLuongHoanThanh;
            ViewBag.SoLuongHuy = soLuongHuy;

            if (trangThai == 0)
            {
                query = query.Where(hd => hd.MaTrangThai == 0 || hd.MaTrangThai == 1);
            }
            else if (trangThai.HasValue)
            {
                query = query.Where(hd => hd.MaTrangThai == trangThai);
            }

            var hoaDonList = query
                .Select(hd => new HoaDonVM
                {
                    MaHd = hd.MaHd,
                    NgayDat = hd.NgayDat,
                    CachThanhToan = hd.CachThanhToan,
                    TenTrangThai = hd.MaTrangThaiNavigation.TenTrangThai,
                    ThanhTien = hd.ChiTietHds.Sum(ct => ct.SoLuong * ct.DonGia)
                })
                .OrderByDescending(hd => hd.NgayDat)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.TrangThai = trangThai;
            return View(hoaDonList);
        }
        #endregion

        #region Huỷ đơn hàng
        public IActionResult HuyDonHang(int id)
        {
            var hoaDon = db.HoaDons.SingleOrDefault(hd => hd.MaHd == id);

            if (hoaDon == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại!";
                return RedirectToAction("TheoDoiDonHang");
            }

            // trạng thái 0 và 1 mới huỷ được
            if (hoaDon.MaTrangThai == 0 || hoaDon.MaTrangThai == 1)
            {
                //Huỷ thì chuyển về trạng thái = -1
                hoaDon.MaTrangThai = -1; 

                db.SaveChanges();

                TempData["HuyThanhCong"] = "Đơn hàng đã được huỷ thành công!";
            }

            return RedirectToAction("TheoDoiDonHang");
        }
        #endregion

        #region Hoàn thành đơn hàng 
        [HttpPost]
        public IActionResult XacNhanDaNhanHang(int id)
        {
            var donHang = db.HoaDons.Find(id);
            if (donHang == null)
            {
                return NotFound();
            }

            if (donHang.MaTrangThai == 2)
            {
                donHang.MaTrangThai = 3; 
                db.SaveChanges();
                TempData["HoanThanhDonHang"] = true;
                return RedirectToAction("TheoDoiDonHang");
            }

            return RedirectToAction("TheoDoiDonHang");
        }
        #endregion
        #region xem chi tiết
        [Authorize]
        public IActionResult ChiTietDonHang(int MaHd, int page = 1)
        {
            var dsChiTiet = db.ChiTietHds
                              .Include(ct => ct.MaHdNavigation)
                              .Include(ct => ct.MaHhNavigation)
                              .Where(ct => ct.MaHd == MaHd)
                              .ToList();

            if (dsChiTiet == null || dsChiTiet.Count == 0)
            {
                return NotFound();
            }

            ViewBag.Page = page;
            return View(dsChiTiet);
        }
        #endregion


        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }

        [Authorize]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }


            // Lưu đơn hàng vô database

            TempData["Message"] = $"Thanh toán VNPay thành công";
            return RedirectToAction("PaymentSuccess");
        }





    }
}

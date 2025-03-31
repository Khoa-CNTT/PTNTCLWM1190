using Microsoft.AspNetCore.Mvc;
using TheGioiDiaMVC.Data;
using TheGioiDiaMVC.ViewModels;
using TheGioiDiaMVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace TheGioiDiaMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly TheGioiDiaContext db;

        public CartController(TheGioiDiaContext context ,PaypalClient paypalClient)
        {
            _paypalClient = paypalClient;
            db = context;
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
                TempData["CartError"] = "Không tìm thấy hàng hóa!";
                return RedirectToAction("Index", "HangHoa");
            }

            if (hanghoa.SoLanXem <= 0)
            {
                TempData["CartError"] = "Sản phẩm đã hết hàng!";
                return RedirectToAction("Index", "HangHoa");
            }

            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                if (quantity > hanghoa.SoLanXem)
                {
                    TempData["CartError"] = $"Chỉ còn {hanghoa.SoLanXem} sản phẩm!";
                    return RedirectToAction("Index", "HangHoa");
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
                    TempData["CartError"] = $"Bạn chỉ có thể mua tối đa {hanghoa.SoLanXem} sản phẩm!";

                    return RedirectToAction("Index", "HangHoa");
                }

                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            TempData["ThemGioHang"] = "Đã thêm vào giỏ hàng!";
            return Redirect(Request.Headers["Referer"].ToString());
        }
        #endregion

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

        public IActionResult Success()
        {
            return View();
        }




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
        public IActionResult Checkout(CheckoutVM model)
        {
            if (ModelState.IsValid)
            {
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
                            TempData["HetHang"] = $"Sản phẩm {item.TenHh} đã hết hàng hoặc không đủ số lượng!";
                            return RedirectToAction("Index", "HangHoa");

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

    }
}

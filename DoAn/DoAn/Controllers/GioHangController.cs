using DoAn.common;
using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class GioHangController : Controller
    {
            RiceStoreEntities data = new RiceStoreEntities();

        public ActionResult GioHang()
        {
            ViewBag.Tongtien = TongTien();
            ViewBag.Tongsoluongsanpham = TongSoLuongSanPham();
            Session["SoLuongSP"] = TongSoLuongSanPham();
            return View((List<GioHang>)Session["cart"]);
        }

        public ActionResult AddToCart(int id, int quantity, int price)
        {
            if (Session["cart"] == null)
            {
                List<GioHang> cart = new List<GioHang>();
                cart.Add(new GioHang { SanPham = data.SanPhams.Find(id), quantity = quantity, price = price });
                Session["cart"] = cart;
                Session["count"] = 1;
            }
            else
            {
                List<GioHang> cart = (List<GioHang>)Session["cart"];
                //kiểm tra sản phẩm có tồn tại trong giỏ hàng chưa???
                int index = isExist(id);
                if (index != -1)
                {
                    //nếu sp tồn tại trong giỏ hàng thì cộng thêm số lượng
                    cart[index].quantity += quantity;
                }
                else
                {
                    //nếu không tồn tại thì thêm sản phẩm vào giỏ hàng
                    cart.Add(new GioHang { SanPham = data.SanPhams.Find(id), quantity = quantity, price = price });
                    //Tính lại số sản phẩm trong giỏ hàng
                    Session["count"] = Convert.ToInt32(Session["count"]) + 1;
                }
                Session["cart"] = cart;
            }
            
            return Json(new { Message = "Thành công", JsonRequestBehavior.AllowGet });
        }
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            List<GioHang> cart = Session["cart"] as List<GioHang>;
            if (cart != null)
            {
                // Find the corresponding item in the cart
                var cartItem = cart.FirstOrDefault(item => item.SanPham.id == id);
                if (cartItem != null)
                {
                    // Update the quantity
                    cartItem.quantity = quantity;

                    // Calculate the new price and total price (Replace with your own logic)
                    cartItem.thanhtien = (double)cartItem.SanPham.price * quantity;

                    // Calculate the total price
                    double totalPrice = TongTien();

                    // Save changes (if necessary)
                    // db.SaveChanges(); // If you need to save changes to the database

                    // Create an anonymous object with the updated values
                    var updatedValues = new
                    {
                        price = cartItem.thanhtien,
                        totalPrice = totalPrice
                    };

                    // Return the updated values as JSON
                    return Json(updatedValues, JsonRequestBehavior.AllowGet);
                }
            }

            return HttpNotFound();
        }


        private int isExist(int id)
        {
            List<GioHang> cart = (List<GioHang>)Session["cart"];
            for (int i = 0; i < cart.Count; i++)
                if (cart[i].SanPham.id.Equals(id))
                    return i;
            return -1;
        }
        public ActionResult Remove(int id)
        {
            List<GioHang> li = (List<GioHang>)Session["cart"];
                li.RemoveAll(x => x.SanPham.id == id);
                Session["cart"] = li;
                Session["count"] = Convert.ToInt32(Session["count"]) - 1;          
            return Json(new { Message = "Thành công", JsonRequestBehavior.AllowGet });
        }

        private int TongSoLuongSanPham()
        {
            int tsl = 0;
            List<GioHang> li = (List<GioHang>)Session["cart"];
            if (li != null)
            {
                tsl = li.Count;
            }
            return tsl;
        }
        private double TongTien()
        {
            List<GioHang> cart = Session["cart"] as List<GioHang>;
            if (cart != null)
            {
                double totalPrice = 0;

                foreach (var item in cart)
                {
                    totalPrice += item.thanhtien;
                }

                return totalPrice;
            }

            return 0; // Return 0 if the cart is empty or null
        }


       

        public ActionResult XoaAllGioHang()
        {
            List<GioHang> li = (List<GioHang>)Session["cart"];
            li.Clear();
            return RedirectToAction("GioHang");
        }



        public ActionResult GioHangPartial()
        {

            ViewBag.Tongtien = TongTien();
            ViewBag.Tongsoluongsanpham = TongSoLuongSanPham();
            return PartialView();
        }

        [HttpGet]
        public ActionResult DatHang()
        {

            if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
            {
                return RedirectToAction("FlatLogin", "KhachHang");
            }

            if (TongSoLuongSanPham() == 0)
            {

                return RedirectToAction("Index", "SanPham");
            }

            if (Session["Address"] == null)
            {
                return RedirectToAction("Info", "KhachHang", new { id = int.Parse(Session["IDKH"].ToString()) });

            }

            List<GioHang> li = (List<GioHang>)Session["cart"];
            ViewBag.Tongtien = TongTien();
            ViewBag.TongSoluongsanpham = TongSoLuongSanPham();

            return View(li);
        }
        [HttpPost]
        public ActionResult DatHang(FormCollection collection)
        {
            DonHang dh = new DonHang();
            KhachHang kh = (KhachHang)Session["TaiKhoan"];
            SanPham s = new SanPham();

            List<GioHang> gh = (List<GioHang>)Session["cart"];
            var ngaygiao = String.Format("{0:MM/dd/yyyy}", collection["NgayGiao"]);

            dh.KH_id = kh.id;
            dh.order_date = DateTime.Now;
            dh.status = 1;
            dh.email = kh.email;
            dh.fullname = kh.fullname;
            dh.phone_number = kh.phone_number;
            dh.address = kh.address;
            dh.total_money = (int)TongTien();
            //dh.thanhtoan = false;

            data.DonHangs.Add(dh);
            data.SaveChanges();
            foreach (var item in gh)
            {
                ChiTietDonHang ctdh = new ChiTietDonHang();
                ctdh.order_id = dh.id;
                ctdh.product_id = item.SanPham.id;
                ctdh.num = item.quantity;
                ctdh.total_money = (int)item.thanhtien;
                s = data.SanPhams.Single(n => n.id == item.SanPham.id);
                s.quantity -= ctdh.num;
                data.SaveChanges();

                data.ChiTietDonHangs.Add(ctdh);
            }

            string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/common/neworder.html"));
            content = content.Replace("{{CustomerName}}", kh.fullname);
            content = content.Replace("{{Phone}}", kh.phone_number);
            content = content.Replace("{{Email}}", kh.email);
            content = content.Replace("{{Address}}", kh.address);
            content = content.Replace("{{OrderID}}", dh.id.ToString());
            content = content.Replace("{{OrderDate}}", dh.order_date.ToString());
            content = content.Replace("{{Total}}", TongTien().ToString("#,##0.00") + "VND");
            var toEmail = kh.email;
            //new MailHelper().SendMail(kh.email, "Ricie - Web bán gạo hàng đầu Hutech.", content);

            new MailHelper().SendMail(toEmail, "Ricie - Web bán gạo hàng đầu Hutech.", content);

            data.SaveChanges();
            Session["cart"] = null;
            return RedirectToAction("Index", "SanPham");
        }

        public ActionResult PaymentVNPay()
        {





            double a = TongTien();
            double total = a * 100;

            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website ủa merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", total.ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);


            return Redirect(paymentUrl);
        }

        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                        DonHang dh = new DonHang();
                        KhachHang kh = (KhachHang)Session["TaiKhoan"];
                        SanPham s = new SanPham();

                        List<GioHang> gh = (List<GioHang>)Session["cart"];

                        dh.KH_id = kh.id;
                        dh.order_date = DateTime.Now;
                        dh.status = 0;
                        dh.email = kh.email;
                        dh.fullname = kh.fullname;
                        dh.phone_number = kh.phone_number;
                        dh.address = kh.address;
                        dh.total_money = (int)TongTien();
                        //dh.thanhtoan = false;

                        data.DonHangs.Add(dh);
                        data.SaveChanges();

                        foreach (var item in gh)
                        {
                            ChiTietDonHang ctdh = new ChiTietDonHang();
                            ctdh.order_id = dh.id;
                            ctdh.product_id = item.SanPham.id;
                            ctdh.num = item.quantity;
                            ctdh.total_money = (int)item.thanhtien;
                            s = data.SanPhams.Single(n => n.id == item.SanPham.id);
                            s.quantity -= ctdh.num;
                            data.SaveChanges();

                            data.ChiTietDonHangs.Add(ctdh);
                        }
                        string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/common/neworder.html"));
                        content = content.Replace("{{CustomerName}}", kh.fullname);
                        content = content.Replace("{{Phone}}", kh.phone_number);
                        content = content.Replace("{{Email}}", kh.email);
                        content = content.Replace("{{Address}}", kh.address);
                        content = content.Replace("{{OrderID}}", dh.id.ToString());
                        content = content.Replace("{{OrderDate}}", dh.order_date.ToString());
                        content = content.Replace("{{Total}}", TongTien().ToString("#,##0.00") + "VND");
                        var toEmail = kh.email;

                        new MailHelper().SendMail(toEmail, "Ricie - Web bán gạo hàng đầu Hutech.", content);

                        data.SaveChanges();
                        Session["cart"] = null;
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            return View();
        }

        //    // GET: GioHang
        //    public ActionResult Index()
        //    {
        //        return View();
        //    }
    }
}
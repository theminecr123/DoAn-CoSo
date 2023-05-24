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
            public List<GioHang> getGioHang()
            {
                List<GioHang> listGioHang = Session["GioHang"] as List<GioHang>;
                if (listGioHang == null)
                {
                    listGioHang = new List<GioHang>();
                    Session["GioHang"] = listGioHang;
                }
                return listGioHang;

            }

        public ActionResult addCart(int id, string strURL)
        {
            List<GioHang> listGioHang = getGioHang();
            GioHang sanpham = listGioHang.Find(n => n.id == id);
            if (sanpham == null)
            {
                sanpham = new GioHang(id);
                listGioHang.Add(sanpham);
                return Redirect(strURL);
            }
            else
            {
                sanpham.quantity++;
                return Redirect(strURL);
            }
        }


        private int TongSoLuong()
        {
            int tsl = 0;
            List<GioHang> listGioHang = Session["GioHang"] as List<GioHang>;
            if (listGioHang != null)
            {
                tsl = listGioHang.Sum(n => n.quantity);
            }
            return tsl;
        }

        private int TongSoLuongSanPham()
        {
            int tsl = 0;
            List<GioHang> listGioHang = Session["GioHang"] as List<GioHang>;
            if (listGioHang != null)
            {
                tsl = listGioHang.Count;
            }
            return tsl;
        }
        private double TongTien()
        {
            double tt = 0;
            List<GioHang> listGioHang = Session["GioHang"] as List<GioHang>;
            if (listGioHang != null)
            {
                tt = listGioHang.Sum(n => n.thanhtien);
            }
            return tt;
        }

        public ActionResult GioHang()
        {
            List<GioHang> listGioHang = getGioHang();
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            ViewBag.Tongsoluongsanpham = TongSoLuongSanPham();
            Session["SoLuongSP"] = TongSoLuongSanPham();
            return View(listGioHang);
        }

        public ActionResult GioHangPartial()
        {
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            ViewBag.Tongsoluongsanpham = TongSoLuongSanPham();
            return PartialView();
        }
        public ActionResult XoaGioHang(int id)
        {          
            List<GioHang> listGioHang = getGioHang();
            GioHang sanpham = listGioHang.SingleOrDefault(n => n.id == id);
            if (sanpham != null)
            {
                listGioHang.RemoveAll(n => n.id == id);
                return RedirectToAction("GioHang");
            }
            return RedirectToAction("GioHang");
        }

       
        public ActionResult CapNhatGioHang(int id, FormCollection collection)
        {
            List<GioHang> listGioHang = getGioHang();
            

            GioHang sanpham = listGioHang.SingleOrDefault(n => n.id == id);

            if (sanpham != null)
            {
                sanpham.quantity = int.Parse(collection["txtSoLg"].ToString());
            }

            return RedirectToAction("GioHang");
        }

        public ActionResult XoaAllGioHang()
        {
            List<GioHang> listGioHang = getGioHang();
            listGioHang.Clear();
            return RedirectToAction("GioHang");
        }


        [HttpGet]
        public ActionResult DatHang()
        {

            if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
            {
                return RedirectToAction("FlatLogin", "KhachHang");
            }

            if (TongSoLuong() == 0)
            {

                return RedirectToAction("Index", "SanPham");
            }

            if (Session["Address"] == null)
            {
                return RedirectToAction("Info", "KhachHang", new { id = int.Parse(Session["IDKH"].ToString()) });

            }

            List<GioHang> listGioHang = getGioHang();
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            ViewBag.TongSoluongsanpham = TongSoLuongSanPham();

            return View(listGioHang);
        }
        [HttpPost]
        public ActionResult DatHang(FormCollection collection)
        {
            DonHang dh = new DonHang();
            KhachHang kh = (KhachHang)Session["TaiKhoan"];
            SanPham s = new SanPham();

            List<GioHang> gh = getGioHang();
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
                ctdh.product_id = item.id;
                ctdh.num = item.quantity;
                ctdh.total_money = (int)item.thanhtien;
                s = data.SanPhams.Single(n => n.id == item.id);
                s.quantity -= ctdh.num;
                data.SaveChanges();

                data.ChiTietDonHangs.Add(ctdh);
            }

            string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/common/neworder.html"));
            content = content.Replace("{{CustomerName}}",kh.fullname);
            content = content.Replace("{{Phone}}",kh.phone_number);
            content = content.Replace("{{Email}}",kh.email);
            content = content.Replace("{{Address}}",kh.address);
            content = content.Replace("{{OrderID}}",dh.id.ToString());
            content = content.Replace("{{OrderDate}}",dh.order_date.ToString());
            content = content.Replace("{{Total}}",TongTien().ToString("#,##0.00")+"VND");
            var toEmail = kh.email;
            //new MailHelper().SendMail(kh.email, "Ricie - Web bán gạo hàng đầu Hutech.", content);

            new MailHelper().SendMail(toEmail, "Ricie - Web bán gạo hàng đầu Hutech.", content);

            data.SaveChanges();
            Session["GioHang"] = null;
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

                        List<GioHang> gh = getGioHang();

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
                            ctdh.product_id = item.id;
                            ctdh.num = item.quantity;
                            ctdh.total_money = (int)item.thanhtien;
                            s = data.SanPhams.Single(n => n.id == item.id);
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
                        Session["GioHang"] = null;
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

        // GET: GioHang
        public ActionResult Index()
        {
            return View();
        }
    }
}
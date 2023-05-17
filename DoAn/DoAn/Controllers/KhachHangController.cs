using DoAn.common;
using DoAn.Models;
using Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class KhachHangController : Controller
    {
        RiceStoreEntities data = new RiceStoreEntities();

        public bool CheckUser( string email)
        {
            var check = data.KhachHangs.Any(n => n.email == email);
            if (check == true)
                return true;

            return false;
        }
        public bool CheckPhone(string phone)
        {
            var check = data.KhachHangs.Any(n => n.phone_number == phone);
            if (check == true)
                return true;

            return false;
        }
        

        public static string HashPassword(string password)
        {
            using (var sha256 = new SHA256Managed())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
        
        [HttpGet]
        public ActionResult FlatModernSignup()
        {
            return View("FlatLogin");
        }

        [HttpPost]
        public ActionResult FlatModernSignUp(FormCollection collection, KhachHang kh)
        {
            var fullname = collection["fullname"];
            var userPass = collection["password"];
            var passConfirm = collection["passConfirm"];
            var email = collection["email"];
            var phone_number = collection["phone_number"];
            
            if (CheckUser(email) == true)
            {
                ViewData["TrungEmail"] = "Email đã tồn tại!";
                return View("FlatLogin");
                
            }
            if (CheckPhone(phone_number) == true)
            {
                ViewData["TrungPhone"] = "Số điện thoại đã tồn tại!";
                return View("FlatLogin");

            }
           
            else if (String.IsNullOrEmpty(passConfirm))
            {
                ViewData["NhappasswordXacNhan"] = "Nhập Mật khẩu xác nhận!";
            }

            else
            {
                if (!userPass.Equals(passConfirm))
                {
                    ViewData["passwordGiongNhau"] = "Mật khẩu và Mật khẩu xác nhận phải giống nhau";
                }
                else
                {
                    kh.fullname = fullname;
                    kh.password = HashPassword(userPass);
                    kh.email = email;
                    kh.phone_number = phone_number;
                    kh.role_id = 2;

                    data.KhachHangs.Add(kh);
                    data.SaveChanges();

                    return RedirectToAction("FlatLogin", "KhachHang");
                }

            }
            return View("FlatLogin");
        }


        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult FlatLogin()
        {
            return View();
        }


    public static bool VerifyPassword(string password, string hashedPassword)
    {
        using (var sha256 = new SHA256Managed())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hashedString.Equals(hashedPassword);
        }
    }
        /*Login*/
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult FlatLogin(FormCollection collection)
        {
            var email = collection["email"];
            var password = collection["password"];
            var kh = data.KhachHangs.SingleOrDefault(n => n.email == email);

           

            if (kh != null)
            {
                if(VerifyPassword(password, kh.password))
                {
                    ViewBag.ThongBao = "Đã đăng nhập!";
                    Session["TaiKhoan"] = kh;
                    Session["IDKH"] = kh.id;
                    Session["IDuser"] = kh.role_id;
                    Session["Name"] = kh.fullname;
                    Session["Address"] = kh.address;
                    return RedirectToAction("Index", "SanPham");
                }
                else
                {
                    ViewData["CheckMK"] = "Sai Thông Tin tài khoản! ";
                    return View();
                }

            }
            
            else { return View(); }

        }

            public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Info(int? id)
        {
            
            if (id == null || Session["IDKH"] == null)
                return RedirectToAction("Index", "Home");
            else if(id != int.Parse(Session["IDKH"].ToString()))
            {
                return RedirectToAction("Info", "KhachHang", new {id = int.Parse(Session["IDKH"].ToString()) });

            }
            var kh = data.KhachHangs.First(x=>x.id == id);
            return View(kh);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("FlatLogin", "KhachHang");

            }
            var kh = data.KhachHangs.First(m => m.id == id);
            return View(kh);
        }
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            

            var kh = data.KhachHangs.First(m => m.id == id);
            var fullname = collection["fullname"];
            var email = collection["email"];
            var address = collection["address"];

            
            kh.id = id;         
                kh.fullname = fullname;
                kh.address = address;
                data.SaveChanges();
            Session["TaiKhoan"] = kh;
            Session["IDKH"] = kh.id;
            Session["IDuser"] = kh.role_id;
            Session["Name"] = kh.fullname;
            Session["Address"] = kh.address;
            return RedirectToAction("Info", "KhachHang", new { id = kh.id });
           
                     



        }
        public ActionResult changePassword(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("FlatLogin", "KhachHang");

            }
            var kh = data.KhachHangs.First(m => m.id == id);
            return View(kh);
        }

        [HttpPost]
        public ActionResult changePassword(int id, FormCollection collection)
        {
            var kh = data.KhachHangs.First(m => m.id == id);
            var oldPassword = collection["oldPassword"];
            var password = collection["password"];
            var confirmPassword = collection["confirmPassword"];
            kh.id = id;
            if (!Equals(password, confirmPassword))
            {
                ViewData["passwordGiongNhau"] = "Mật khẩu và Mật khẩu xác nhận phải giống nhau";
                return View(kh);
            }

            if (VerifyPassword(oldPassword, kh.password))
            {
                kh.password = HashPassword(password);
                data.SaveChanges();
                return RedirectToAction("Info", "KhachHang", new { id = kh.id });

            }

            else
            {
                ViewData["saiMK"] = "Mật khẩu sai";
                return View(kh);
            }
        }
        public ActionResult forgotPass()
        {
            return View();
        }

        [HttpPost]
        public ActionResult forgotPass(string email)
        {
            bool kh = data.KhachHangs.Any(x=>x.email == email);
            if (kh)
            {
                Random random = new Random();

                int otp = random.Next(100000, 999999);
                Session["OTP"] = otp;
                string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/common/resetPass.html"));
                content = content.Replace("{{otp}}", otp.ToString());

                var toEmail = email;
                Session["emailReset"] = toEmail;

                new MailHelper().SendMail(toEmail, "Ricie - Web bán gạo hàng đầu Hutech.", content);

                return RedirectToAction("resetPass");
            }
            else
            {
                ViewData["SaiEmail"] = "Email không tồn tại trong hệ thống!";
                return View();
            }
          
        }
        public ActionResult resetPass()
        {
            if (Session["emailReset"] == null)
            {
                return RedirectToAction("FlatLogin");
            }
            return View();
        }
        [HttpPost]
        public ActionResult resetPass(string otp, FormCollection collection)
        {
            var password = collection["password"];
            var confirmPassword = collection["confirmPassword"];
            string emailReset = Session["emailReset"].ToString();
            var kh = data.KhachHangs.Where(m => m.email == emailReset ).FirstOrDefault();
            if (otp == Session["OTP"].ToString())
            {
                
                if (!Equals(password, confirmPassword))
                {
                    ViewData["passwordGiongNhau"] = "Mật khẩu và Mật khẩu xác nhận phải giống nhau";
                    return View(kh);
                }
                else
                {
                    kh.password = HashPassword(password);
                    data.SaveChanges();
                    return RedirectToAction("FlatLogin", "KhachHang");

                }

                
            }
            return View();
        }
        // GET: KhachHang
        public ActionResult Index()
        {
            return RedirectToAction("FlatLogin", "KhachHang");
        }

        private const string FacebookAppId = "779625872775978";
        private const string FacebookAppSecret = "78be4fb391d8aa63e712c2e56291b807";

        [AllowAnonymous]
        public ActionResult FacebookLogin()
        {
            var fb = new FacebookClient();
            var RedirectUri = "https://localhost:44333/KhachHang/FacebookCallback";
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = FacebookAppId,
                client_secret = FacebookAppSecret,
                redirect_uri = RedirectUri,
                response_type = "code",
                scope = "email"
            });

            return Redirect(loginUrl.AbsoluteUri);
        }

        [AllowAnonymous]
        public async Task<ActionResult> FacebookCallback(string code)
        {
            var fb = new FacebookClient();
            var RedirectUri = "https://localhost:44333/KhachHang/FacebookCallback";
            dynamic result = await fb.PostTaskAsync("oauth/access_token", new
            {
                client_id = FacebookAppId,
                client_secret = FacebookAppSecret,
                redirect_uri = RedirectUri,
                code = code
            });

            var accessToken = result.access_token;

            fb.AccessToken = accessToken;
            dynamic me = await fb.GetTaskAsync("me?fields=name,email");


            var name = (string)me.name;
            var email = (string)me.email;

            // Check if user already exists in your database
            var user = data.KhachHangs.SingleOrDefault(n => n.email == email);
            if (user != null)
            {
                // Log user in
                Session["IDKH"] = user.id;
                Session["TaiKhoan"] = user;
                Session["IDuser"] = user.role_id;
                Session["Name"] = user.fullname;
                return RedirectToAction("Index", "SanPham");
            }
            else
            {
                // Register new user
                var fullname = (string)me.name;

                var kh = new KhachHang
                {
                    fullname = fullname,
                    email = email,
                    role_id = 2 // Default role for new users
                };

                data.KhachHangs.Add(kh);
                data.SaveChanges();

                Session["TaiKhoan"] = kh;
                Session["IDuser"] = kh.role_id;
                Session["Name"] = kh.fullname;
                return RedirectToAction("Index", "SanPham");
            }
        }

    }
}
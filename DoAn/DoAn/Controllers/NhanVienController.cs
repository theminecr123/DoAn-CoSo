using DoAn.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace DoAn.Controllers
{
    public class NhanVienController : Controller
    {
        RiceStoreEntities data = new RiceStoreEntities();
        // GET: Admin


        public static string HashPassword(string password)
        {
            using (var sha256 = new SHA256Managed())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
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


        [AllowAnonymous]
        public ActionResult FlatLogin()
        {
            return View();
        }

        /*Login*/
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult FlatLogin(FormCollection collection)
        {
            var email = collection["email"];
            var password = collection["password"];
            var nv = data.NhanViens.SingleOrDefault(n => n.email == email );
            if (nv != null)
            {
                if(VerifyPassword(password, nv.password))
                {
                    ViewBag.ThongBao = "Đã đăng nhập!";
                    Session["TaiKhoan"] = nv;
                    Session["IDuser"] = nv.role_id;
                    Session["Name"] = nv.fullname;
                    if (nv.role_id == 1)
                    {
                        Session["RoleName"] = "Admin";
                    }

                    return RedirectToAction("Index", "NhanVien");
                }
                else
                {
                    ViewData["CheckMK"] = "Sai Thông Tin tài khoản! ";
                    return View();
                }
            }
            else
            {
                return View();
            }
        }
       
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Index()
        {

            if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
            {
                return RedirectToAction("FlatLogin", "NhanVien");
            }
            if (int.Parse(Session["IDuser"].ToString()) == 2 )
            {
                return RedirectToAction("FlatLogin", "NhanVien");
            }
            return View();
        }

       
        public string ProcessUploadGao(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return "";
            }
            file.SaveAs(Server.MapPath("~/Content/images/Gao/" + file.FileName));
            return "/Content/images/Gao/" + file.FileName;
        }
        /*KhachHang*/

        public ActionResult KhachHang()
        {
            var all_kh = (from s in data.KhachHangs select s).OrderBy(m => m.id);
            return View(all_kh);
        }

        public ActionResult CreateKH()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateKH(FormCollection collection, KhachHang kh)
        {
            var fullname = collection["fullname"];
            var email = collection["email"];
            var phone_number = collection["phone_number"];
            var address = collection["address"];
            var password = collection["password"];
          
                kh.fullname = fullname.ToString();
                kh.email = email.ToString();
                kh.phone_number = phone_number.ToString();
                kh.address = address;
                kh.password = HashPassword(password);
                data.KhachHangs.Add(kh);
                data.SaveChanges();
                return RedirectToAction("KhachHang");
            

        }       
        public ActionResult EditKH(int id)
        {
            var E_sach = data.KhachHangs.First(m => m.id == id);
            return View(E_sach);
        }
        [HttpPost]
        public ActionResult EditKH(int id, FormCollection collection, string actionName)
        {
            var kh = data.KhachHangs.First(m => m.id == id);
            var fullname = collection["fullname"];
            var email = collection["email"];
            var address = collection["address"];
            kh.id = id;

      
           
                kh.fullname = fullname;
                kh.email = email;
                kh.address = address;

                data.SaveChanges();
                return RedirectToAction("KhachHang");
            


        }



        //-----------------------------------------
        public ActionResult DeleteKH(int id)
        {
            var kh = data.KhachHangs.First(m => m.id == id);
            return View(kh);
        }
        [HttpPost]
        public ActionResult DeleteKH(int id, FormCollection collection)
        {
            var kh = data.KhachHangs.Where(m => m.id == id).First();
            data.KhachHangs.Remove(kh);
            data.SaveChanges();
            return RedirectToAction("KhachHang");
        }



        /*!KhachHang*/


        /*DonHang*/

        //-----------------------------------------
        public ActionResult DonHang()
        {
            var all_dh = (from s in data.DonHangs select s).OrderBy(m => m.id);
            return View(all_dh);
        }
        public ActionResult DeleteDH(int id)
        {
            var D_NhanVien = data.DonHangs.First(m => m.id == id);
            return View(D_NhanVien);
        }
        [HttpPost]
        public ActionResult DeleteDH(int id, FormCollection collection)
        {
            var D_NhanVien = data.DonHangs.Where(m => m.id == id).First();
            data.DonHangs.Remove(D_NhanVien);
            data.SaveChanges();
            return RedirectToAction("DonHang");
        }
        /*end DonHang*/

        /*NhanVien*/
        public ActionResult NhanVien()
        {
            var all_nv = (from s in data.NhanViens select s).OrderBy(m => m.id);
            return View(all_nv);
        }
        public ActionResult CreateNV()
        {
            if (int.Parse(Session["IDuser"].ToString()) == 1)
                return View();
            else
            {
                ViewBag.ErrorAccess = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "NhanVien");
            }
        }
        [HttpPost]
        public ActionResult CreateNV(FormCollection collection, NhanVien s)
        {
            var fullname = collection["fullname"];
            var email = collection["email"];
            var phone_number = collection["phone_number"];
            var address = collection["address"];
            var password = collection["password"];
            var role_id = collection["role_id"];
            

            
                s.fullname = fullname.ToString();
                s.email = email.ToString();
                s.phone_number = phone_number;
                s.address = address;
                s.password = HashPassword(password);

                data.NhanViens.Add(s);
                data.SaveChanges();
                return RedirectToAction("NhanVien");

        }
       
        public ActionResult EditNV(int id)
        {
            if (int.Parse(Session["IDuser"].ToString()) == 1)
            {
                var sp = data.NhanViens.First(m => m.id == id);
                return View(sp);
            }

            else
            {
                ViewBag.ErrorAccess = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "NhanVien");
            }
            
        }
        [HttpPost]
        public ActionResult EditNV(int id, FormCollection collection)
        {
            var NhanVien = data.NhanViens.First(m => m.id == id);
            var fullname = collection["fullname"];
            var email = collection["email"];
            var phone_number = collection["phone_number"];
            var address = collection["address"];
            var password = collection["password"];
            var role_id = int.Parse(collection["role_id"]);
            NhanVien.id = id;




            if (string.IsNullOrEmpty(fullname))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                NhanVien.fullname = fullname;
                NhanVien.email = email;
                NhanVien.phone_number = phone_number;
                NhanVien.address = address;
                NhanVien.password = HashPassword(password);
                NhanVien.role_id = role_id;
                UpdateModel(NhanVien);


                data.SaveChanges();
                return RedirectToAction("NhanVien");
            }

            return this.EditNV(id);
        }



        //-----------------------------------------
        public ActionResult DeleteNV(int id)
        {      
                var D_NhanVien = data.NhanViens.First(m => m.id == id);
                return View(D_NhanVien);
      
        }
        [HttpPost]
        public ActionResult DeleteNV(int id, FormCollection collection)
        {
            var D_NhanVien = data.NhanViens.Where(m => m.id == id).First();
            data.NhanViens.Remove(D_NhanVien);
            data.SaveChanges();
            return RedirectToAction("NhanVien");
        }
        /*end NhanVien*/

        /*SanPham*/
        public ActionResult SanPham()
        {
            var all_sp = (from s in data.SanPhams select s).OrderBy(m => m.id);
            return View(all_sp);
        }

        public ActionResult CreateSP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateSP(FormCollection collection, SanPham sp)
        {
            var title = collection["title"];
            var DM_id = collection["DM_id"];
            var thumbnail = collection["thumbnail"];
            var price = collection["price"];
            var quantity = Convert.ToInt32(collection["quantity"]);
            var descripton = collection["descripton"];


                sp.title = title;
                sp.thumbnail = thumbnail;
                sp.price = int.Parse(price);
                sp.quantity = quantity;
                sp.DM_id = int.Parse(DM_id);
                sp.description = descripton;
                data.SanPhams.Add(sp);
                data.SaveChanges();
                return RedirectToAction("SanPham");
            
           
        }
        public ActionResult EditSP(int id)
        {
            var sp = data.SanPhams.First(m => m.id == id);
            return View(sp);
        }
        [HttpPost]
        public ActionResult EditSP(int id, FormCollection collection)
        {
            var sp = data.SanPhams.First(m => m.id == id);
            var title = collection["title"];
            var DM_id = collection["DM_id"];
            var thumbnail = collection["thumbnail"];
            var price = collection["price"];
            var quantity = Convert.ToInt32(collection["quantity"]);
            var descripton = collection["descripton"];
            sp.id = id;
    
          
                sp.title = title;
                sp.thumbnail = thumbnail;
                sp.price = int.Parse(price);
                sp.quantity = quantity;
                sp.DM_id = int.Parse(DM_id);
                sp.description = descripton;
                UpdateModel(sp);


                data.SaveChanges();
                return RedirectToAction("SanPham");
            

        }
        public ActionResult DeleteSP(int id)
        {
            var sp = data.SanPhams.First(m => m.id == id);
            return View(sp);
        }
        [HttpPost]
        public ActionResult DeleteSP(int id, FormCollection collection)
        {
            var sp = data.SanPhams.Where(m => m.id == id).First();
            data.SanPhams.Remove(sp);
            data.SaveChanges();
            return RedirectToAction("SanPham");
        }





    }
}
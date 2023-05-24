using DoAn.App_Start;
using DoAn.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
                    Session["IDNV"] = nv.id;
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

        public List<sp_BaoCaoTheoNam_Result> BaoCaoSanPhamNam(int year)
        {
            using (RiceStoreEntities data = new RiceStoreEntities())
            {
                var lsData = data.sp_BaoCaoTheoNam(year).ToList();
                return lsData;
            }

        }

        //public List<sp_BaoCaoSPTheoNamThang_Result> BaoCaoSanPhamThang(int year)
        //{
        //    using (RiceStoreEntities data = new RiceStoreEntities())
        //    {
        //        var lsData = data.sp_BaoCaoSPTheoNamThang(year).ToList();
        //        return lsData;
        //    }

        //}

        public List<sp_BaoCaoDoanhThuNam_Result> BaoCaoDoanhThuNam(int year)
        {
            using (RiceStoreEntities data = new RiceStoreEntities())
            {
                var lsData = data.sp_BaoCaoDoanhThuNam(year).ToList();
                return lsData;
            }

        }


        [HttpGet]
        [UserAuthorize]
        public ActionResult Index()
        {           
            return View();
        }

        [UserAuthorize]
        public ActionResult BaoCaoSPNam(int year)
        {
            var LsData = BaoCaoSanPhamNam(year);
            return Json(LsData, JsonRequestBehavior.AllowGet);
        }
        //[HttpGet]
        //public ActionResult DoanhThuSPThang()
        //{
        //    return View();
        //}
        //public ActionResult BaoCaoSPThang(int year)
        //{
        //    var LsData = BaoCaoSanPhamThang(year);
        //    return Json(LsData, JsonRequestBehavior.AllowGet);
        //}
        [HttpGet]
        public ActionResult DoanhThuNam()
        {
            return View();
        }

        public ActionResult BaoCaoDTNam(int year)
        {
            var LsData = BaoCaoDoanhThuNam(year);
            return Json(LsData, JsonRequestBehavior.AllowGet);
        }
        //-------------------------------------------------------------------------------------------

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
        [UserAuthorize]
        public ActionResult KhachHang()
        {
            return View();
        }
        [UserAuthorize]
        public ActionResult GetCustomers(string searchName)
        {
            List<KhachHang> customers = null;

            data.Configuration.ProxyCreationEnabled = false;
            if (!string.IsNullOrEmpty(searchName))
            {
                customers = data.KhachHangs.Where(x => x.fullname.ToLower().Contains(searchName.Trim().ToLower())).ToList();
            }
            else
            {
                customers = data.KhachHangs.ToList();
            }



            return Json(new
            {
                Data = customers,
                TotalItems = customers.Count
            }, JsonRequestBehavior.AllowGet);
        }

       

        [HttpPost]
        [UserAuthorize]
        public ActionResult CreateKH(KhachHang customers)
        {
           
                customers.password = HashPassword(customers.password);
                data.KhachHangs.Add(customers);

            try
            {
                data.SaveChanges();
                return Json(new { success = true });

            }
            catch
            {
                return Json(new { success = false });

            }
        }


        [HttpPost]
        [UserAuthorize]
        public JsonResult DeleteKH(int? id)
        {
            var customers = data.KhachHangs.Find(id);
            data.KhachHangs.Remove(customers);
            var rs = data.SaveChanges();
            if (rs > 0)
            {
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }



        /*!KhachHang*/


        /*DonHang*/

        //-----------------------------------------
        [UserAuthorize]
        public ActionResult DonHang()
        {
            return View();
        }

        [UserAuthorize]
        public ActionResult GetOrders(string searchName)
        {
            List<DonHang> orders = null;

            data.Configuration.ProxyCreationEnabled = false;
            if (searchName == null)
            {
                orders = data.DonHangs.Where(x => x.fullname.ToLower().Contains(searchName.Trim().ToLower())).ToList();
                
            }
            else
            {
                orders = data.DonHangs.ToList();
                
            }



            return Json(new
            {
                Data = orders,
                TotalItems = orders.Count
            }, JsonRequestBehavior.AllowGet);
        }

        [UserAuthorize]
        public ActionResult GetByIdDH(int? id)
        {
            data.Configuration.ProxyCreationEnabled = false;
            var item = data.DonHangs.Find(id);
            return Json(new { data = item }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [UserAuthorize]
        public ActionResult UpdateDH(DonHang orders)
        {
            if (orders.id > 0)
            {
                data.DonHangs.Attach(orders);
                data.Entry(orders).State = EntityState.Modified;
                data.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });

            }
        }
       


        [HttpPost]
        [UserAuthorize]
        public JsonResult DeleteDH(int? id)
        {
            var ordersDetail = data.ChiTietDonHangs.Where(x => x.order_id == id).ToList();
            var orders = data.DonHangs.Find(id);
            data.ChiTietDonHangs.RemoveRange(ordersDetail);
            data.DonHangs.Remove(orders);
            var rs = data.SaveChanges();
            if (rs > 0)
            {
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        /*end DonHang*/

        /*NhanVien*/
        [UserAuthorize]
        public ActionResult NhanVien()
        {
            return View();
        }
        [UserAuthorize]
        public ActionResult GetStaffs(string searchName)
        {
            List<NhanVien> staffs = null;

            data.Configuration.ProxyCreationEnabled = false;
            if (!string.IsNullOrEmpty(searchName))
            {
                staffs = data.NhanViens.Where(x => x.fullname.ToLower().Contains(searchName.Trim().ToLower())).ToList();
            }
            else
            {
                staffs = data.NhanViens.ToList();
            }



            return Json(new
            {
                Data = staffs,
                TotalItems = staffs.Count
            }, JsonRequestBehavior.AllowGet);
        }

        [UserAuthorize]
        public ActionResult GetByIdNV(int? id)
        {
            data.Configuration.ProxyCreationEnabled = false;
            var item = data.NhanViens.Find(id);
            return Json(new { data = item }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [UserAuthorize(ChucNang = 1)]
        public ActionResult CreateUpdateNV(NhanVien staffs)
        {
            if (staffs.id > 0)
            {
                data.NhanViens.Attach(staffs);
                data.Entry(staffs).State = EntityState.Modified;
                data.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                staffs.password = HashPassword("Aa@123456");
                data.NhanViens.Add(staffs);

            }
            try
            {
                data.SaveChanges();
                return Json(new { success = true });

            }
            catch
            {
                return Json(new { success = false });

            }
        }
        [UserAuthorize]
        public ActionResult Info(int? id)
        {

            if (id == null || Session["IDNV"] == null)
                return RedirectToAction("Index", "Home");
            else if (id != int.Parse(Session["IDNV"].ToString()))
            {
                return RedirectToAction("Info", "NhanVien", new { id = int.Parse(Session["IDKH"].ToString()) });

            }
            var nv = data.NhanViens.First(x => x.id == id);
            return View(nv);
        }

        [UserAuthorize]
        public ActionResult changePassword(int? id)
        {
            
            return View();
        }

        [HttpPost]
        [UserAuthorize]
        public ActionResult changePassword(int id, FormCollection collection)
        {
            var kh = data.NhanViens.First(m => m.id == id);
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

        [HttpPost]
        [UserAuthorize(ChucNang = 1)]
        public JsonResult DeleteNV(int? id)
        {
            var staffs = data.NhanViens.Find(id);
            data.NhanViens.Remove(staffs);
            var rs = data.SaveChanges();
            if (rs > 0)
            {
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }


        /*end NhanVien*/

        /*SanPham*/
        [UserAuthorize]
        public ActionResult SanPham()
        {           
            return View();
        }

        [UserAuthorize]
        public ActionResult GetProducts(string searchName)
        {
            List<SanPham> products = null;

            data.Configuration.ProxyCreationEnabled = false;
            if (!string.IsNullOrEmpty(searchName))
            {
                products = data.SanPhams.Where(x => x.title.ToLower().Contains(searchName.Trim().ToLower())).ToList();
            }
            else
            {
                products = data.SanPhams.ToList();
            }


          
            return Json(new
            {
                Data = products,
                TotalItems = products.Count            
            }, JsonRequestBehavior.AllowGet);
        }

        [UserAuthorize]
        public ActionResult GetByIdSP(int? id)
        {
            data.Configuration.ProxyCreationEnabled = false;
            var item = data.SanPhams.Find(id);
            return Json(new { data = item }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [UserAuthorize]
        public ActionResult CreateUpdateSP(SanPham sp)
        {
            if (sp.id > 0)
            {
                data.SanPhams.Attach(sp);               
                data.Entry(sp).State = EntityState.Modified;
                data.SaveChanges();
                return Json(new { success = true });
            }
            data.SanPhams.Add(sp);
            try
            {
                data.SaveChanges();
                return Json(new { success = true });

            }
            catch
            {
                return Json(new { success = false });

            }
        }


        [HttpPost]
        [UserAuthorize]
        public JsonResult DeleteSP(int? id)
        {
            var sp = data.SanPhams.Find(id);
            data.SanPhams.Remove(sp);
            var rs = data.SaveChanges();
            if (rs > 0)
            {
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
        




    }
}
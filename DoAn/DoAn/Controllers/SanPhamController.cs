using DoAn.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Web.UI;
using X.PagedList;
using Microsoft.Ajax.Utilities;
using System.Web.Management;

namespace DoAn.Controllers
{
    public class SanPhamController : Controller
    {
        RiceStoreEntities data = new RiceStoreEntities();
        // GET: SanPham
       
        public ActionResult Index(int? page, string nameSearch, double? from, double? to)
        {
            if (page == null) page = 1;
            var all_sp = (from s in data.SanPhams select s).OrderBy(m => m.id);
            if (!string.IsNullOrEmpty(nameSearch))
            {
                if (to != null && from != null)
                {
                    all_sp = all_sp.Where(x => x.title.Contains(nameSearch) && x.price >= from && x.price <= to).OrderBy(m => m.id);

                }
                else
                {
                    all_sp = all_sp.Where(x => x.title.Contains(nameSearch)).OrderBy(m => m.id);

                }


            }
            else
            {
                if (to != null && from != null)
                {
                    all_sp = all_sp.Where(x => x.price >= from && x.price <= to).OrderBy(m => m.id);

                }
            }
            int pageSize = 6;
            int pageNum = page ?? 1;
            return View(all_sp.ToPagedList(pageNum, pageSize));
        }

        public ActionResult Detail(int id)
        {
            var sp = data.SanPhams.Where(m => m.id == id).First();
            if(sp != null)
            {
                return View(sp);
            }
            else
            {
                return HttpNotFound();
            }
            
        }

        public ActionResult In()
        {
            return View();
        }
        
    }
}
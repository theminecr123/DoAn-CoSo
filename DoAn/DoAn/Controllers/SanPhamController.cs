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
using System.Text.RegularExpressions;
using System.Text;
using System.Net;
using DoAn.common;

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

        public ActionResult Detail(int? id, string title)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "SanPham");
            }

            var sp = data.SanPhams.FirstOrDefault(m => m.id == id);
            if (sp != null)
            {
                string expectedTitle = convertToUnSign3(sp.title);
                if (title != expectedTitle)
                {
                    // If the provided title doesn't match the expected title, redirect to the correct URL
                    return RedirectToAction("Detail", "SanPham", new { id = id, title = expectedTitle });
                }

                return View(sp);
            }
            else
            {
                return RedirectToAction("Index", "SanPham");
            }
        }

        public static string convertToUnSign3(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            // Replace whitespace characters with dashes
            temp = Regex.Replace(s, @"\s+", "-").Trim();

            // Replace multiple dashes with a single dash
            temp = Regex.Replace(s, @"-+", "-");
            return regex.Replace(temp, String.Empty);
        }
   


    }
}
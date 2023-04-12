using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DoAn.Models
{
    public class GioHang
    {
        RiceStoreEntities data = new RiceStoreEntities();
        public int id { get; set; }
        [Display(Name = "Tên gạo")]
        public string title { get; set; }

        [Display(Name = "Hình")]
        public string thumbnail { get; set; }

        [Display(Name = "Giá Bán")]
        public Double price { get; set; }

        [Display(Name = "Số Lượng")]
        public int quantity { get; set; }
        
        //Tính thành tiền
        [Display(Name = "Thành Tiền")]
        public Double thanhtien
        {
            get
            {
                return quantity * price;
            }
        }

        public GioHang(int id)
        {
            this.id = id;
            SanPham gao = data.SanPhams.Single(n => n.id == id);
            this.title =gao.title;
            this.thumbnail = gao.thumbnail;
            price = double.Parse(gao.price.ToString());
            quantity = 1;
        }
    }
}
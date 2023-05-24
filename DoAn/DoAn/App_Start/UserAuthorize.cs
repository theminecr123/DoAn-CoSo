using DoAn.Models;
using System.Web;
using System.Web.Mvc;

namespace DoAn.App_Start
{
    public class UserAuthorize : AuthorizeAttribute
    {
        public int ChucNang { get; set; }
        public override void OnAuthorization(AuthorizationContext filterContext) 
        {
            var User = HttpContext.Current.Session["IDuser"];
            if (User == null)
            {
                filterContext.Result = new RedirectToRouteResult(new
                    System.Web.Routing.RouteValueDictionary(new
                    {
                        Controller = "NhanVien",
                        action = "FlatLogin",
                        AreaReference = "Home"

                    }));
                return;
            }
            RiceStoreEntities data = new RiceStoreEntities();
            if (ChucNang == 0)
            {
                return;
            }

            if(ChucNang != int.Parse(User.ToString())) {
                filterContext.Result = new RedirectToRouteResult(new
                    System.Web.Routing.RouteValueDictionary(new
                    {
                        controller = "NhanVien",
                        action = "FlatLogin"
                    }));
            }
            if(int.Parse(User.ToString()) == 2)
            {
                filterContext.Result = new RedirectToRouteResult(new
                    System.Web.Routing.RouteValueDictionary(new
                    {
                        Controller = "Home",
                        action = "Index",
                        AreaReference = "Home"
                    }));
                return;
            }
        }
    }
}
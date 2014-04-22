using System.Web.Mvc;


namespace MvcChatR.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Home/Chat/

        public ActionResult Chat()
        {
            return View();
        }

        //
        // GET: /Home/Presentor
        public ActionResult Presentor()
        {
            return View();
        }

        //
        // GET: /Home/MultiSeriesLineChart

        public ActionResult MultiSeriesLineChart()
        {
            return View();
        }
    }
}

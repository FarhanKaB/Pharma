using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using static System.Collections.Specialized.BitVector32;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.UI.WebControls;

namespace Pharma.Controllers
{
    public class GuestController : Controller
    {
        // GET: Guest
        public ActionResult Guest_Home()
        {
            return View();
        }
        public ActionResult Guest_About()
        {
            return View();
        }
        public ActionResult Guest_Shop()
        {
            return View();
        }
        public ActionResult Guest_Contact()
        {
            return View();
        }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CCSHealthFamilyWelfareDept.Models;

namespace CCSHealthFamilyWelfareDept.Controllers
{
    public class ISPController : Controller
    {
        //
        // GET: /ISP/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }
    }
}

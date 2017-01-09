using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace PP.Signicat.WebApi.Controllers
{
    public class SignController : Controller
    {
        // GET: Sign
        public ActionResult Index()
        {
            return View();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JJM_ImageSystems.Controllers
{
    public class PowerSchoolController : Controller
    {
        public IActionResult Index(string openid_identifier)
        {
            string[] parsedUrl = openid_identifier.Split('/');
            ViewData["usertype"] = parsedUrl[parsedUrl.Length - 2];
            ViewData["username"] = parsedUrl[parsedUrl.Length - 1];

        // https://DevPowerSchool.ceoimage.com/oid/admin/ceo


            // openid.ns.ax= http://openid.net/srv/ax/1.0
            // openid.ax.type.dcid = http://powerschool.com/entity/id

            //<input type="hidden" name="openid.ns.ax" value="http://openid.net/srv/ax/1.0" />
            //<input type="hidden" name="openid.ax.type.dcid" value="http://powerschool.com/entity/id" />


            return View();
        }
    }
}
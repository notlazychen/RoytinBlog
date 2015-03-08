using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iBoxDB.LocalServer;
using Nancy;

namespace RoytinBlog
{
    public class HomeModule:NancyModule
    {
        public HomeModule(AutoBox box)
        {
            Get["/"] = _ =>
            {
                return View["home"];
            };
        }
    }
}

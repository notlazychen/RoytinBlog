using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nancy.Hosting.Self;

namespace RoytinBlog
{
    static class Program
    {

        public static NancyHost Host { get; private set; }
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string url = ConfigurationManager.AppSettings["SiteUrl"];
            Console.WriteLine(@"开始启动您的网站，URL是：{0}", url);
            Host = new NancyHost(new[]{new Uri(url), });
            Host.Start();
            Console.WriteLine(@"您的网站已经开始运行, 建议使用chrome连接浏览. ");
            while (true)
            {
                string cmd = Console.ReadLine();
                if (cmd == "q")
                {
                    break;
                }
            }
        }
    }
}

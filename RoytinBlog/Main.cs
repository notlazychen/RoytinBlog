using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nancy.Hosting.Self;

namespace RoytinBlog
{
    public partial class Main : Form
    {
        public NancyHost Host { get; private set; }
        public Main()
        {
            InitializeComponent();
            int port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            Host = new NancyHost(UrlHelper.GetUriParams(port));
            Host.Start();
        }
    }
}

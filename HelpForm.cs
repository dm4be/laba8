using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor
{
    public partial class HelpForm: Form
    {
        public HelpForm()
        {
            InitializeComponent();
            this.Text = "Справка";
            this.Width = 600;
            this.Height = 500;

            var browser = new WebBrowser
            {
                Dock = DockStyle.Fill
            };

            string helpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "help.html");

            if (File.Exists(helpPath))
            {
                browser.Navigate(helpPath);
            }
            else
            {
                browser.DocumentText = "<html><body><h2>Файл справки не найден.</h2></body></html>";
            }

            this.Controls.Add(browser);
        }

    }
}

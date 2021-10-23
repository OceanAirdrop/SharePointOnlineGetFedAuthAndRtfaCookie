using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsSharePointApp2
{
    // https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/hh147177(v=office.14)?redirectedfrom=MSDN
    // https://stackoverflow.com/questions/15049877/getting-webbrowser-cookies-to-log-in
    // https://stackoverflow.com/questions/3382498/is-it-possible-to-transfer-authentication-from-webbrowser-to-webrequest
    // https://stackoverflow.com/questions/3062925/c-sharp-get-httponly-cookie
    // https://stackoverflow.com/questions/25388696/federated-authentication-in-sharepoint-2013-getting-rtfa-and-fedauth-cookies

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initially navigate to a page that the user will have access to, to grab the FedAuth & rtFa cookies.
            this.webBrowser1.Navigate(@"https://xx.sharepoint.com/xx/xx/xx/Forms/AllItems.aspx");
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            try
            {
                if (webBrowser1.Url.AbsoluteUri == "about:blank")
                    return;

                var cookieData = GetWebBrowserCookie.GetCookieInternal(webBrowser1.Url, false);

                if (string.IsNullOrEmpty(cookieData) == false)
                {
                    textBoxCookie.Text = cookieData;

                    var dict = ParseCookieData(cookieData);
                    textBoxFedAuth.Text = dict["FedAuth"];
                    textBoxrtFa.Text = dict["rtFa"];
                }
            }
            catch (Exception)
            {
            }
        }

        private IDictionary<string, string> ParseCookieData(string cookieData)
        {
            var cookieDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (string.IsNullOrEmpty(cookieData))
                    return cookieDictionary;

                var values = cookieData.TrimEnd(';').Split(';');
                foreach (var parts in values.Select(c => c.Split(new[] { '=' }, 2)))
                {
                    var cookieName = parts[0].Trim();
                    string cookieValue;

                    if (parts.Length == 1)
                        cookieValue = string.Empty;
                    else
                        cookieValue = parts[1];

                    cookieDictionary[cookieName] = cookieValue;
                }
            }
            catch (Exception)
            {
            }

            return cookieDictionary;
        }

        private void buttonDownloadImage_Click(object sender, EventArgs e)
        {
            try
            {
                var url = $"https://xx.sharepoint.com/sites/xx/Images/{textBoxImageName.Text}";

                var handler = new HttpClientHandler();
                handler.CookieContainer = new System.Net.CookieContainer();

                var cc = new CookieCollection();
                cc.Add(new Cookie("FedAuth", textBoxFedAuth.Text));
                cc.Add(new Cookie("rtFa", textBoxrtFa.Text));

                handler.CookieContainer.Add(new Uri(url), cc);

                HttpClient httpClient = new HttpClient(handler);
                var resp = httpClient.GetAsync(url).Result;
                var byteData = resp.Content.ReadAsByteArrayAsync().Result;

                if (resp.IsSuccessStatusCode)
                {
                    pictureBox1.Image = byteArrayToImage(byteData);
                }
            }
            catch (Exception)
            {

            }
        }

        public Image byteArrayToImage(byte[] bytesArr)
        {
            using (MemoryStream memstr = new MemoryStream(bytesArr))
            {
                Image img = Image.FromStream(memstr);
                return img;
            }
        }
    }
}

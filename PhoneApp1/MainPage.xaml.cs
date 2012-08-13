using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using System.Threading;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Controls;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Phone.Shell;
using Microsoft.Advertising.Mobile;
using Microsoft.Advertising.Mobile.UI;

namespace CyberoamLogin
{
    public partial class MainPage : PhoneApplicationPage
    {
        ProgressIndicator indicator = new ProgressIndicator
        {
            IsVisible = false,
            IsIndeterminate = true,
            Text = "Logging in"
        };

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            /*
            AdControl adControl = new AdControl("test_client",   // ApplicationID
                                                "Image480_80",   // AdUnitID
                                                true);           // isAutoRefreshEnabled
            // Make the AdControl size large enough that it can contain the image
            adControl.Width = 480;
            adControl.Height = 80;

            Grid grid = (Grid)this.LayoutRoot.Children[1];
            grid.Children.Add(adControl);
            */
        
            IsolatedStorageFile myStorage = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStorage.FileExists("Data.txt"))
            {
                IsolatedStorageFileStream filestream = myStorage.OpenFile("Data.txt", FileMode.Open, FileAccess.Read);
                using (StreamReader reader = new StreamReader(filestream))
                {
                    this.textBox1.Text = reader.ReadLine();
                    this.textBox2.Text = reader.ReadLine();
                    this.passwordBox1.Password = reader.ReadLine();
                }
            }

        }

        private static ManualResetEvent allDone = new ManualResetEvent(false);    

        public void button1_Click(object sender, RoutedEventArgs e)
        {

            SystemTray.SetProgressIndicator(this, indicator);
            indicator.IsVisible = true;

            try
            {
                WebClient wb = new WebClient();
                string data = "mode=191&username=" + textBox2.Text + @"&password=" + passwordBox1.Password;
                wb.Headers["Connection"] = "keep-alive";
                wb.Headers["ContentType"] = "application/x-www-form-urlencoded";
                wb.UploadStringCompleted += new UploadStringCompletedEventHandler(wb_UploadStringCompleted);
                wb.UploadStringAsync(new Uri(@"http://" + textBox1.Text + @":8090/login.xml", UriKind.Absolute), data);    
            }
            
            catch (Exception) 
            {
                indicator.IsVisible = false;
                MessageBox.Show("Oops! There are invalid Input Parameters, please review them."); 
            }

            using (IsolatedStorageFile myStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (myStorage.FileExists("Data.txt"))
                    {
                        using (StreamWriter writer = new StreamWriter(new IsolatedStorageFileStream("Data.txt", FileMode.Open, FileAccess.Write, myStorage)))
                        {
                            string mydata = "" +textBox1.Text+ "\n" +textBox2.Text+ "\n" +passwordBox1.Password+"";
                            writer.WriteLine(mydata);
                            writer.Close();
                        }
                    }

                    using (StreamWriter writer = new StreamWriter(new IsolatedStorageFileStream("Data.txt", FileMode.Create, FileAccess.Write, myStorage)))
                    {
                        string mydata = "" + textBox1.Text + "\n" + textBox2.Text + "\n" + passwordBox1.Password + "";
                        writer.WriteLine(mydata);
                        writer.Close();
                    }

                }
                         
        }

        void wb_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                indicator.IsVisible = false;
                MessageBox.Show("An error occoured, check you Network Connection and/or Input Parameters.");

                return;
            }

            string reply = (string)e.Result;
            using (XmlReader rd = XmlReader.Create(new StringReader(reply)))
            {
                rd.ReadToFollowing("message");
                string msg = rd.ReadElementContentAsString();
                indicator.IsVisible = false;
                MessageBox.Show(msg);

            }
        }


    }


}
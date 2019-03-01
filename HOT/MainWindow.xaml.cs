using System;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Oculus;

namespace OculusHack
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string OculusInstallFolder;
        public double ss = 1.0;

        public MainWindow()
        {
            
            InitializeComponent();
            OculusInstallFolder = Properties.Settings.Default.HOTSettings;
            ss = Properties.Settings.Default.SSsetting;
            Tools.GetOculusLibraris();
            
    

            if (Tools.CheckOculusInstallFolder(OculusInstallFolder))
            {
                l_oculusfolder.Content = OculusInstallFolder;
                b_oculusfolder.IsEnabled = false;
                cb_ASW.SelectedIndex = 0; //TODO this should get actual status
                cb_debugHUD.SelectedIndex = 0; //TODO this should get actual status
                Tools.SetDebugToolSS(OculusInstallFolder,ss);

                if (!IsAdministrator()) grid_advanced.IsEnabled = false;


                //Createini(); Used for custom home start
                CheckStatus();
            }
            else if (Tools.CheckOculusInstallFolder("c:\\Program Files\\Oculus\\"))
            {
                OculusInstallFolder = "c:\\Program Files\\Oculus\\";

                Properties.Settings.Default.HOTSettings = OculusInstallFolder;
                Properties.Settings.Default.Save();

                l_oculusfolder.Content = OculusInstallFolder;
                b_oculusfolder.IsEnabled = false;

                CheckStatus();
            }
            else
            {
                l_oculusfolder.Content = "Oculus Client not found!";
                b_oculusfolder.IsEnabled = true;
                MainGrid.IsEnabled = false;
            }

        }


        ///<summary>
        ///Create MoveHome.ini in the Home2 folder to let store the optional url launch parameter
        ///</summary>
        private void Createini()
        {
            //    if (!File.Exists(OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\MoveHome.ini"))
            //    {
            //        File.CreateText(OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\MoveHome.ini");
            //    }
            //    else return;
        }

        ///<summary>
        ///Check the presence of "Home2-Win64-Shipping.exe" and library ".dll" to set proper buttons status
        ///and set the buttons accordingly, all possiblity are managed here.
        ///</summary>
        private void CheckStatus()
        {
            //Check Library version
            l_version.Content = "Runtime version: "+Tools.GetLibVersion(OculusInstallFolder);
                 
            
            //Check Home status
            if (File.Exists(OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.exe"))
                {
                ck_home_status.IsChecked = true;
                }else ck_home_status.IsChecked = false;

            //Check Dash SFX
            if (File.Exists(OculusInstallFolder + "Support\\oculus-dash\\dash\\assets\\raw\\audio_dash\\Build\\Desktop\\Master Bank.bank"))
            {
                ck_sfx_status.IsChecked = true;
            }
            else ck_sfx_status.IsChecked = false;
                     

            //Super setting value
            l_ss.Content = ss.ToString();
            b_setSS.IsEnabled = false;
                
        }

        private void b_oculusfolder_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Select OculusSetup.exe";
            ofd.DefaultExt = "*.exe";
            ofd.Filter = "OculusSetup.exe |OculusSetup.exe";
            if (ofd.ShowDialog() == false) return;

            OculusInstallFolder = System.IO.Path.GetDirectoryName(ofd.FileName);
            l_oculusfolder.Content = OculusInstallFolder;
            Properties.Settings.Default.HOTSettings = OculusInstallFolder;
            Properties.Settings.Default.Save();
            Createini();
            CheckStatus();
            MainGrid.IsEnabled = true;

        }
                
        
        private void b_lib_Click(object sender, RoutedEventArgs e)
        {
            //need to check if both Oculus client runnung and steam VR 

            if (Tools.IsOculusLibraryEnable(OculusInstallFolder))
            {
                Tools.DisableOculusLibrary(OculusInstallFolder);
                CheckStatus();
                CountDown(30);
            }
            else
            {
                Tools.EnableOculusLibrary(OculusInstallFolder);
            }
            CheckStatus();
            
        }
        
        private void b_svr_Click(object sender, RoutedEventArgs e)
        {

            //Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MoveHome.Home2-Win64-Shipping.exe");
            //FileStream fileStream = new FileStream(Tools.GetOculusInstallFolder() + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\temp.exe", FileMode.CreateNew);
            //for (int i = 0; i < stream.Length; i++)
            //    fileStream.WriteByte((byte)stream.ReadByte());
            //fileStream.Close();
            //File.Copy(Tools.GetOculusInstallFolder() + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\temp.exe", Tools.GetOculusInstallFolder() + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.exe", true);
            //File.Delete(Tools.GetOculusInstallFolder() + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\temp.exe");
            //File.WriteAllText(Tools.GetOculusInstallFolder() + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\MoveHome.ini", "steam://rungameid/250820");
            //CheckStatus();

        }

        private void CountDown(int t)
        {
            var timer = new DispatcherTimer();
            timer.Tick += delegate

            {
                l_timer.Content = t.ToString();
                t--;
                if (t == 0)
                {
                    timer.Stop();
                    Tools.EnableOculusLibrary(OculusInstallFolder);
                    l_timer.Content = "";
                    CheckStatus();
                    return;
                }
            };

            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Start();
        }
                   
        private void b_setSS_Click(object sender, RoutedEventArgs e)
        {
            Tools.SetDebugToolSS(OculusInstallFolder, ss);
            Properties.Settings.Default.SSsetting = ss;
            Properties.Settings.Default.Save();
            b_setSS.IsEnabled = false;
        }

        private void b_ss_plus_Click(object sender, RoutedEventArgs e)
        {
            if (ss < 5)
            {
                ss +=  0.05;
                CheckStatus();
                b_setSS.IsEnabled = true;
            }
        }

        private void b_ss_minus_Click(object sender, RoutedEventArgs e)
        {
            if (ss > 0.50)
            {
                ss -= 0.05;
                CheckStatus();
                b_setSS.IsEnabled = true;
            }
            
        }

        private void cb_debugHUD_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cb_debugHUD.SelectedIndex == 0)
            {
                Tools.SetDebugOSD(OculusInstallFolder, 0);
            }
            else if (cb_debugHUD.SelectedIndex == 1)
            {
                Tools.SetDebugOSD(OculusInstallFolder, 1);
            }
            else if (cb_debugHUD.SelectedIndex == 2)
            {
                Tools.SetDebugOSD(OculusInstallFolder, 2);
            }
            else if (cb_debugHUD.SelectedIndex == 3)
            {
                Tools.SetDebugOSD(OculusInstallFolder, 3);
            }
            else if (cb_debugHUD.SelectedIndex == 4)
            {
                Tools.SetDebugOSD(OculusInstallFolder, 4);
            }
            else if (cb_debugHUD.SelectedIndex == 5)
            {
                Tools.SetDebugOSD(OculusInstallFolder, 6);
            }
            else if (cb_debugHUD.SelectedIndex == 6)
            {
                Tools.SetDebugOSD(OculusInstallFolder, 10);
            }
        }

        private void Cb_ASW_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            if (cb_ASW.SelectedIndex == 0)
            {
                Tools.SetASW(OculusInstallFolder, 0);
            }
            else if (cb_ASW.SelectedIndex == 1)
            {
                Tools.SetASW(OculusInstallFolder, 1);
            }
            else if (cb_ASW.SelectedIndex == 2)
            {
                Tools.SetASW(OculusInstallFolder, 2);
            }
            else if (cb_ASW.SelectedIndex == 3)
            {
                Tools.SetASW(OculusInstallFolder, 3);
            }

        }

        private void B_restore_lib_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Select Library .zip";
            ofd.DefaultExt = "*.zip";
            ofd.Filter = "*.zip |*.zip";
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == false) return;
            string filename = ofd.FileName;
            //Tools.StopOculusService();
            while (Tools.StopOculusService() != 0)
            {
                MessageBox.Show("Oculus Service is stopping...");
            }
            Tools.RestoreLibrary(filename,OculusInstallFolder);
            Tools.StartOculusService();
            CheckStatus();
        }

        private void B_back_lib_Click(object sender, RoutedEventArgs e)
        {
            Tools.BackupLibrary(OculusInstallFolder);
        }

        private void B_guardian_Click(object sender, RoutedEventArgs e)
        {
            Tools.EnableGuardian(OculusInstallFolder);
        }

        private static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Ck_home_status_Click(object sender, RoutedEventArgs e)
        {
            if (ck_home_status.IsChecked == false)
            {
                Tools.DisableHome(OculusInstallFolder);
            }
            else Tools.EnableHome(OculusInstallFolder);
        }

        private void Ck_sfx_status_Click(object sender, RoutedEventArgs e)
        {
            if (ck_sfx_status.IsChecked == false)
            {
                MessageBox.Show("Warning, Dash will be restarted.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                Tools.DisableDashSFX(OculusInstallFolder);
            }
            else
            {
                MessageBox.Show("Warning, Dash will be restarted.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                Tools.EnableDashSFX(OculusInstallFolder);
            }
        }
    }
}

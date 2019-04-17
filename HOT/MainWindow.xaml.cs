using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Security.Principal;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Launcher;



namespace OculusHack
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string OculusInstallFolder = Properties.Settings.Default.HOTSettings;
        public double ss = Math.Round(Properties.Settings.Default.SSsetting,2);
        public List<Record> records = new List<Record>();
        public string[] args = Environment.GetCommandLineArgs();
        
        
                

        public MainWindow()
        {
            
            InitializeComponent();
                                              
            // Check if the Oculus install forlder is real
            // if not, enable the ofd to select the forlder.
            if (Tools.CheckOculusInstallFolder(OculusInstallFolder))
            {
                l_oculusfolder.Content = OculusInstallFolder;
                b_oculusfolder.IsEnabled = false;
                cb_ASW.SelectedIndex = 0; //TODO this should get actual status
                cb_debugHUD.SelectedIndex = 0; //TODO this should get actual status
                Tools.SetSS(OculusInstallFolder,ss);

                // Disable Advanced tab if not in Administrator mode.
                if (!IsAdministrator())
                {
                    grid_advanced.IsEnabled = false;
                    tb_admin.Text = "To enable this tab you need to run as Administrator";
                }


                // Check if "args" are passed and launch apps with relative settings.
                if (args.Length > 1)
                {
                    //TODO evaulate if really needed args check.
                    //foreach (Record rec in records)
                    //{
                    //    if (args[1].Substring(args[1].LastIndexOf("\\") + 1) == rec.exe)
                    //    {
                    //        l_launcher.Content = "gotcha!";
                    //        Tools.SetSS(OculusInstallFolder, rec.ss);
                    //        ss = rec.ss;
                    //        Tools.SetASW(OculusInstallFolder, rec.asw);
                    //        Tools.SetOSD(OculusInstallFolder, rec.osd);
                    //        CfgTools.RunApp(args[1]);
                    //        //TODO: reset Oculus to default
                    //        //TODO: optional lose HOT
                    //    }
                    //}
                }

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

            // Read all records in cfg, or create it if not exist
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\HOT\\HOT.cfg"))
            {
                records = CfgTools.ReadCfg(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\HOT\\HOT.cfg");
            }
            

            //TODO: implement run exe check for launch parameters
            //ExeCheck execheck = new ExeCheck();
            //ExeCheck();

        }

       

        public void ExeCheck()
        {   
            try
            {
                string ComputerName = "localhost";
                string WmiQuery = "Select * From __InstanceCreationEvent Within 1 " + "Where TargetInstance ISA 'Win32_Process' "; ;
                ManagementEventWatcher Watcher;
                ManagementScope Scope;

                Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
                Scope.Connect();

                Watcher = new ManagementEventWatcher(Scope, new EventQuery(WmiQuery));
                Watcher.EventArrived += new EventArrivedEventHandler(this.evento);
                Watcher.Start();
                //Console.Read();
                //Watcher.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0} Trace {1}", e.Message, e.StackTrace);
            }

        }
        

        //Check status of avialability of Home and Dash SFX
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

        private void evento(object sender, EventArrivedEventArgs e)
        {
            //in this point the new events arrives
            //you can access to any property of the Win32_Process class
            //Console.WriteLine("TargetInstance.Handle :    " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Handle"]);
            string exe = Convert.ToString(((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]);
            l_launcher.Content = exe;
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
            CheckStatus();
            MainGrid.IsEnabled = true;

        }
                  
        private void b_setSS_Click(object sender, RoutedEventArgs e)
        {
            Tools.SetSS(OculusInstallFolder, ss);
            //Store actual SS as default value
            Properties.Settings.Default.SSsetting = ss;
            Properties.Settings.Default.Save();
            b_setSS.IsEnabled = false;
        }

        private void b_ss_plus_Click(object sender, RoutedEventArgs e)
        {
            if (ss < 5)
            {
                ss += 0.05;
                Math.Round(ss, 2);
                CheckStatus();
                b_setSS.IsEnabled = true;
            }
        }

        private void b_ss_minus_Click(object sender, RoutedEventArgs e)
        {
            if (ss > 0.50)
            {
                ss -= 0.05;
                Math.Round(ss, 2);
                CheckStatus();
                b_setSS.IsEnabled = true;
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

        private void cb_debugHUD_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cb_debugHUD.SelectedIndex == 0)
            {
                Tools.SetOSD(OculusInstallFolder, 0);
            }
            else if (cb_debugHUD.SelectedIndex == 1)
            {
                Tools.SetOSD(OculusInstallFolder, 1);
            }
            else if (cb_debugHUD.SelectedIndex == 2)
            {
                Tools.SetOSD(OculusInstallFolder, 2);
            }
            else if (cb_debugHUD.SelectedIndex == 3)
            {
                Tools.SetOSD(OculusInstallFolder, 3);
            }
            else if (cb_debugHUD.SelectedIndex == 4)
            {
                Tools.SetOSD(OculusInstallFolder, 4);
            }
            else if (cb_debugHUD.SelectedIndex == 5)
            {
                Tools.SetOSD(OculusInstallFolder, 6);
            }
            else if (cb_debugHUD.SelectedIndex == 6)
            {
                Tools.SetOSD(OculusInstallFolder, 10);
            }
        }
                             


        #region Advanced setting tab
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
            Microsoft.Win32.SaveFileDialog ofd = new Microsoft.Win32.SaveFileDialog();
            ofd.Title = "Select backup file name";
            ofd.FileName = "OculusLib_"+Tools.GetLibVersion(OculusInstallFolder) + ".zip";
            ofd.Filter = ".zip |.zip";
            if (ofd.ShowDialog() == false) return;

            Tools.BackupLibrary(OculusInstallFolder,ofd.FileName);
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
        #endregion

    }
}

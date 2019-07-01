using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Launcher;
using Json;


namespace OculusHack
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string OculusInstallFolder = Properties.Settings.Default.HOTSettings;
        public double ss = Math.Round(Properties.Settings.Default.SSsetting,2);
        public string openvrcfg = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\openvr\\openvrpaths.vrpath";
        private List<JsonRecord> jrecs = new List<JsonRecord>();

        private string cfg_file;
        private string newapp = "";
        private string lastapp = "";

        public ObservableCollection<Record> records = new ObservableCollection<Record>();

        public MainWindow()
        {
            
            InitializeComponent();

            #region Open Composite: check for update and download
            popup.IsOpen = true;

            void dl_dll()
            {
                //pop_oc.Text = "Check for update...";
                if (OC.CheckForUpdate())
                {
                    //pop_oc.Text = "Download Open Composite...";
                    OC.downloadDll();
                }
                              
                Dispatcher.Invoke(() =>
                {
                    popup.IsOpen = false;
                });
            }

            Thread thread = new Thread(dl_dll);
            thread.Start();
            #endregion

            //if Oculus installfolder is null, try the default
            if (OculusInstallFolder == "")
            {
                OculusInstallFolder = "c:\\Program Files\\Oculus";
                Properties.Settings.Default.HOTSettings = OculusInstallFolder;
                Properties.Settings.Default.Save();

            }

            // Check if the Oculus install folder is ok
            // if not, enable the ofd to select the folder, or abort app.
            while (!Tools.CheckOculusInstallFolder(OculusInstallFolder))
            {
                MessageBoxResult mb = MessageBox.Show("Please install Oculus client, and click OK to select OculusSetup.exe", "Oculus desktop client not found", MessageBoxButton.OKCancel);
                if (mb == MessageBoxResult.OK)
                {
                    OculusInstallFolder = get_oculusfolder();
                }
                else
                {
                    Application.Current.MainWindow.Close();// TODO implement better close, instead crash
                }

            }

            #region Set default parameter

            //jrecs = JsonReader.ReadJsonFile(openvrcfg); // TODO WIP **********

            cb_ASW.SelectedIndex = 0; 
            cb_debugHUD.SelectedIndex = 0;
            Tools.SetSS(OculusInstallFolder,ss);
            l_ss.Content = ss.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

            ReadAppsCfg();

            CheckEnviroment();
            #endregion

            #region Admin mode - exe check
            //Activate exe check if in Admin mode, or disable Advanced tab if not in Administrator mode.
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                grid_advanced.IsEnabled = false;
                tb_admin.Text = "To enable this tab you need to run as Administrator";
                b_add_exe.IsEnabled = false;
                b_del_exe.IsEnabled = false;
                lv_records.Visibility = Visibility.Hidden;
                tb_launcher.Visibility = Visibility.Visible;
                
            }
            else
            {
                //Start exe check with Management Event. Admini required
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"); 

                ManagementEventWatcher watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
                watcher.Start();
            }

            // new Exe found
            void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
            {
                newapp = e.NewEvent.Properties["ProcessName"].Value.ToString();

                if (newapp!=lastapp)
                {
                    foreach (Record rec in records)
                    {
                        if (e.NewEvent.Properties["ProcessName"].Value.ToString() == rec.exe)
                        {
                            //Use Dipatcher to allow cross treading to set runtime setup.
                            Dispatcher.Invoke(() =>
                            {
                                //l_launcher.Content = "eccolo";
                                Tools.SetSS(OculusInstallFolder, rec.ss);
                                cb_ASW.SelectedIndex = rec.asw;
                                cb_debugHUD.SelectedIndex = rec.osd;
                                ss = rec.ss;
                                l_ss.Content = ss.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                                //if (rec.oc == 1 && !OC.IsOCactive())
                                //{
                                //    OC.EnableOC();
                                //    cb_OC.IsChecked = true;
                                //}
                                //else if (rec.oc == 0 && OC.IsOCactive())
                                //{
                                //    OC.DisableOC();
                                //    cb_OC.IsChecked = false;
                                //}

                            });

                        }
                    }
                }
                                
                lastapp = e.NewEvent.Properties["ProcessName"].Value.ToString();

            }
            #endregion

               

        }
        
        private void CheckEnviroment()
        {
            //Check Library version
            l_version.Content = "Runtime version: " + Tools.GetLibVersion(OculusInstallFolder);
            
            //Check Home status
            if (Tools.OculusHome(OculusInstallFolder, 1, false))
            {
                ck_home_status.IsChecked = true;
            }
            else ck_home_status.IsChecked = false;

            //Check Dash SFX
            if (Tools.DashSFX(OculusInstallFolder, 1,false))
            {
                ck_sfx_status.IsChecked = true;
            }
            else ck_sfx_status.IsChecked = false;

            //check Dash black BG
            if (Tools.DashBackground(OculusInstallFolder, 0, false))
            {
                ck_blk_dash.IsChecked = false;
            }
            else ck_blk_dash.IsChecked = true;

            //Chekc OC active
            if (OC.IsOCactive())
            {
                cb_OC.IsChecked = true;
            }
            else cb_OC.IsChecked = false;
        }

        private string get_oculusfolder()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Select OculusSetup.exe";
            ofd.DefaultExt = "*.exe";
            ofd.Filter = "OculusSetup.exe |OculusSetup.exe";
            if (ofd.ShowDialog() == false) return null;

            OculusInstallFolder = Path.GetDirectoryName(ofd.FileName);

            return OculusInstallFolder;
            
        }

        private void ReadAppsCfg()
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\HOT.cfg"))
            {
                cfg_file = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\HOT.cfg";
                records = CfgTools.ReadCfg(cfg_file);
                lv_records.ItemsSource = records;
            }
            else
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack");
                File.Create(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\HOT.cfg");
                cfg_file = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\HOT.cfg";
            }
        }

        #region Main tab
        private void b_setSS_Click(object sender, RoutedEventArgs e)
        {
            Tools.SetSS(OculusInstallFolder, ss);
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
                l_ss.Content = ss.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                b_setSS.IsEnabled = true;
            }
        }

        private void b_ss_minus_Click(object sender, RoutedEventArgs e)
        {
            if (ss > 0.50)
            {
                ss -= 0.05;
                Math.Round(ss, 2);
                l_ss.Content = ss.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
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
            else if (cb_ASW.SelectedIndex == 4)
            {
                Tools.SetASW(OculusInstallFolder, 4);
            }


        }

        private void cb_OSD_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cb_debugHUD.SelectedIndex == 0) // OFF
            {
                Tools.SetOSD(OculusInstallFolder, 0);
            }
            else if (cb_debugHUD.SelectedIndex == 1) // Performance summary
            {
                Tools.SetOSD(OculusInstallFolder, 1);
            }
            else if (cb_debugHUD.SelectedIndex == 2) // Latency Timing
            {
                Tools.SetOSD(OculusInstallFolder, 2);
            }
            else if (cb_debugHUD.SelectedIndex == 3) // Applicamtion Render Timing
            {
                Tools.SetOSD(OculusInstallFolder, 3);
            }
            else if (cb_debugHUD.SelectedIndex == 4) // Compositor Render Timing
            {
                Tools.SetOSD(OculusInstallFolder, 4);
            }
            else if (cb_debugHUD.SelectedIndex == 5) // ASW status
            {
                Tools.SetOSD(OculusInstallFolder, 6);
            }
            else if (cb_debugHUD.SelectedIndex == 6) // Layer info, pixel density
            {
                Tools.SetOSD(OculusInstallFolder, 10);
            }
        }

        private void Cb_OC_Click(object sender, RoutedEventArgs e)
        {
            if (cb_OC.IsChecked != true)
            {
                OC.DisableOC();
            }
            else
            {
                OC.EnableOC();
            }
        }

        private void B_add_exe_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Select App Exe";
            ofd.DefaultExt = "*.exe";
            ofd.Filter = "*.exe |*.exe";
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == false) return;

            Record rec = new Record(Path.GetFileName(ofd.FileName), ss, cb_ASW.SelectedIndex , cb_debugHUD.SelectedIndex, Convert.ToInt16(cb_OC.IsChecked));
            records.Add(rec);
            CfgTools.AddRecordToCfg(rec, cfg_file);
            

        }

        private void B_del_exe_Click(object sender, RoutedEventArgs e)
        {
            int idx = lv_records.SelectedIndex;
            records.RemoveAt(idx);
            CfgTools.WriteCfg(records, cfg_file);
        }
        #endregion
        
        #region Advanced setting tab
        
        //private static bool IsAdministrator()
        //{
        //    return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        //}

        private void B_restore_lib_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Select Library .zip";
            ofd.DefaultExt = "*.zip";
            ofd.Filter = "*.zip |*.zip";
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == false) return;
            string filename = ofd.FileName;
            while (Tools.StopOculusService() != 0)
            {
                MessageBox.Show("Oculus Service is stopping...");
            }
            Tools.RestoreLibrary(filename,OculusInstallFolder);
            Tools.StartOculusService();
            CheckEnviroment();
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
     
        private void Ck_home_status_Click(object sender, RoutedEventArgs e)
        {
            Tools.KillOculusHome();

            if (ck_home_status.IsChecked == false)
            {
                Tools.OculusHome(OculusInstallFolder, 0, true);
            }
            else Tools.OculusHome(OculusInstallFolder, 1, true);
        }

        private void Ck_sfx_status_Click(object sender, RoutedEventArgs e)
        {
            if (ck_sfx_status.IsChecked == false)
            {
                Tools.DashSFX(OculusInstallFolder, 0, true);
            }
            else    Tools.DashSFX(OculusInstallFolder, 1, true);

            Tools.KillDash();
        }

        private void Ck_blk_dash_Click(object sender, RoutedEventArgs e)
        {
            if (ck_blk_dash.IsChecked == false)
            {
                Tools.DashBackground(OculusInstallFolder, 0, true);
            }
            else Tools.DashBackground(OculusInstallFolder, 1, true);

            Tools.KillDash();

        }


        #endregion


    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Launcher;


namespace OculusHack
{
    public partial class MainWindow : Window
    {
        public string OculusInstallFolder = Properties.Settings.Default.HOTSettings;
        public double ss = Math.Round(Properties.Settings.Default.SSsetting,2);
        public string openvrcfg = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\openvr\\openvrpaths.vrpath";

        private string cfg_file;

        public ObservableCollection<Record> records = new ObservableCollection<Record>();

        public MainWindow()
        {
            
            InitializeComponent();

            #region Check if Oculus is installed and service is running
            // check Oculus if installed correctly

            // Renable Native library in case off
            Tools.SetNativeLibrary(OculusInstallFolder, true);

            if (Tools.GetOculusInstallFolder() == null)
            {
                MessageBoxResult mb = MessageBox.Show("Oculus installation not found. Please install using OculusSetup.exe", "Oculus register entry not found", MessageBoxButton.OK);
                if (mb == MessageBoxResult.OK)
                {
                    Application.Current.MainWindow.Close();
                }

            }
            // check if service is running
            else if (!Tools.IsOculusServiceRunning())
            {
                MessageBoxResult mb = MessageBox.Show("Oculus service is not running","Oculus service not found", MessageBoxButton.OK);
                //TODO implement Admin call to activate service.
                if (mb == MessageBoxResult.OK)
                {
                    Application.Current.MainWindow.Close(); 
                }
            }
            // all looks ok, go on.
            else
            { 
                OculusInstallFolder = Tools.GetOculusInstallFolder();
                Properties.Settings.Default.HOTSettings = OculusInstallFolder;
                Properties.Settings.Default.Save();
            }
            #endregion

            #region Set default parameter
            if (!Tools.SetSS(OculusInstallFolder, ss))
            {
                MessageBox.Show("OculusDebugToolCLI.exe not found or wrong version!\nPleasce check your Oculus installation.");
                grid_debugtools.IsEnabled = false;
            }
            else
            {
                cb_ASW.SelectedIndex = 0;
                cb_debugHUD.SelectedIndex = 0;
            }
            
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
               
                    foreach (Record rec in records)
                    {
                        if (e.NewEvent.Properties["ProcessName"].Value.ToString() == Path.GetFileName(rec.exe))
                        {
                            //Use Dipatcher to allow cross treading to set runtime setup.
                            Dispatcher.Invoke(() =>
                            {
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
            #endregion

            #region Open Composite: check for update and download
            DownLoadOC();
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

        private async void DownLoadOC()
        {
            popup.IsOpen = true;
            pop_oc.Text = "Check fo Open Composite update";
            bool update = await OC.CheckForUpdate();
            if (update)
            {
                pop_oc.Text = "NEW Open Composite version found !";
                await Task.Delay(2000);
                pop_oc.Text = "Downloading Open Composite...";
                bool download = await OC.downloadDll();
                if (!download)
                {
                    pop_oc.Text = "Open Composite download: connection failed!";

                }
                else pop_oc.Text = "Open Composite is updated.";
                await Task.Delay(2000);
                popup.IsOpen = false;
            }
            else
            {
                popup.IsOpen = false;
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

            Record rec = new Record(ofd.FileName, ss, cb_ASW.SelectedIndex , cb_debugHUD.SelectedIndex, Convert.ToInt16(cb_OC.IsChecked));
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
        
        private async void B_restore_lib_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Select Library .zip";
            ofd.DefaultExt = "*.zip";
            ofd.Filter = "*.zip |*.zip";
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == false) return;
            string filename = ofd.FileName;

            b_lib.IsEnabled = false;

            b_lib.Content = "Oculus Service is stopping...";
            await Tools.StopOculusService();

            b_lib.Content = "Wait...";
            await Tools.RestoreLibrary(filename, OculusInstallFolder);

            b_lib.Content = "Oculus Service is starting...";
            await Tools.StartOculusService();

            b_lib.IsEnabled = true;
            b_lib.Content = "Restore Library";

            CheckEnviroment();
        }

        private async void B_back_lib_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog ofd = new Microsoft.Win32.SaveFileDialog();
            ofd.Title = "Select backup file name";
            ofd.DefaultExt = "*.zip";
            ofd.FileName = "OculusLib_" + Tools.GetLibVersion(OculusInstallFolder) + ".zip";
            ofd.Filter = "*.zip |*.zip";
            if (ofd.ShowDialog() == false) return;
            
            b_back_lib.IsEnabled = false;
            b_back_lib.Content = "Wait...";
                       
            await Tools.BackupLibrary(OculusInstallFolder, ofd.FileName);
            
            b_back_lib.IsEnabled = true;
            b_back_lib.Content = "Backup Library";

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
            Tools.KillDash();

            if (ck_sfx_status.IsChecked == false)
            {
                Tools.DashSFX(OculusInstallFolder, 0, true);
            }
            else    Tools.DashSFX(OculusInstallFolder, 1, true);
            
        }

        private void Ck_blk_dash_Click(object sender, RoutedEventArgs e)
        {
            Tools.KillDash();

            if (ck_blk_dash.IsChecked == false)
            {
                Tools.DashBackground(OculusInstallFolder, 0, true);
            }
            else Tools.DashBackground(OculusInstallFolder, 1, true);

        }

        private void B_disable_oculus_Click(object sender, RoutedEventArgs e)
        {
            // start Steam VR
            Tools.SetNativeLibrary(OculusInstallFolder, false);
            b_disable_oculus.Content = "Oculus Library is DISABLE";
            MainGrid.IsEnabled = false;
            int timeup = 20;
            void timer_countdown()
            {
                for (int i = 0; i < timeup; i++)
                {
                    Dispatcher.Invoke(() =>
                    {
                        b_disable_oculus.Content = "Oculus Library is DISABLE... " + (timeup - i);
                    });
                    Thread.Sleep(1000);

                }

                Dispatcher.Invoke(() =>
                        {
                            Tools.SetNativeLibrary(OculusInstallFolder, true);
                            b_disable_oculus.Content = "Oculus Library is ENABLE";
                            MainGrid.IsEnabled = true;
                        });
                
            }

            Thread thread = new Thread(timer_countdown);
            thread.Start();

        }

        #endregion

        
    }
}

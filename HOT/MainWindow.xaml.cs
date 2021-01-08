using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Launcher;
using Valve.VR;
using System.Reflection;
using System.Security.Permissions;
using System.Diagnostics;

namespace OculusHack
{
    public partial class MainWindow : Window
    {
        
        public string OculusInstallFolder = Properties.Settings.Default.HOTSettings;
        public double ss = Math.Round(Properties.Settings.Default.SSsetting,2);
        public int asw = Properties.Settings.Default.ASWsetting;

        // Steamvr pace maker
        public string openvrcfg = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\openvr\\openvrpaths.vrpath";
        public bool ASWPacemaker = false;
        private string cfg_file;

        // Encoder list
        public ObservableCollection<encode_res> list_encode_res = new ObservableCollection<encode_res>();


        // Watcher record list
        public ObservableCollection<Record> records = new ObservableCollection<Record>();
        

        public MainWindow()
        {
            
            InitializeComponent();

            #region Check Oculus install and Service
            // Check install
            if (Tools.GetOculusInstallFolder() == null)
            {
                MessageBoxResult mb = MessageBox.Show("Oculus installation not found. Please install using OculusSetup.exe", "Oculus register entry not found", MessageBoxButton.OK);
                if (mb == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                }

            }
            else
            {
                // Renable Native library in case off, due to possible crash on disable
                Tools.SetNativeLibrary(OculusInstallFolder, true);
                OculusInstallFolder = Tools.GetOculusInstallFolder();
                Properties.Settings.Default.HOTSettings = OculusInstallFolder;
                Properties.Settings.Default.Save();
            }
            
            // Check Service
            if (!Tools.IsOculusServiceRunning())
            {
                MessageBoxResult mb = MessageBox.Show("Oculus service is not running","Oculus service not found", MessageBoxButton.OK);
                //TODO implement Admin call to activate service.
                if (mb == MessageBoxResult.OK)
                {
                    grid_main.IsEnabled = false;
                    tab_misc.IsSelected = true;
                }
            }
            l_version.Content = "Runtime version: " + Tools.GetLibVersion(OculusInstallFolder);
            #endregion

            #region Check if Oculus Debug tool are available and set default parameter
            if (Tools.IsOculusServiceRunning())
            {
                if (!Tools.SetSS(OculusInstallFolder, ss))
                {
                    MessageBox.Show("OculusDebugToolCLI.exe not found or wrong version!\nPlease check your Oculus installation.\nDebug function are disable.");
                    grid_main.IsEnabled = false;
                }
                else
                {
                    cb_ASW.SelectedIndex = asw;
                    cb_debugHUD.SelectedIndex = 0;
                }
            }
            l_ss.Content = ss.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            #endregion

            #region Read Link values
            encode_res er1 = new encode_res();
            er1.value = -1;
            er1.name = "Default";
            list_encode_res.Add(er1);

            encode_res er2 = new encode_res();
            er2.value = 2352;
            er2.name = er2.value + " Balanced";
            list_encode_res.Add(er2);

            encode_res er3 = new encode_res();
            er3.value = 2608;
            er3.name = er3.value.ToString();
            list_encode_res.Add(er3);

            encode_res er4 = new encode_res();
            er4.value = 2912;
            er4.name = er4.value + " Quality";
            list_encode_res.Add(er4);

            encode_res er5 = new encode_res();
            er5.value = 3288;
            er5.name = er5.value.ToString();
            list_encode_res.Add(er5);

            encode_res er6 = new encode_res();
            er6.value = 3664; // Volga suggest https://twitter.com/volgaksoy/status/1316243051791015936
            er6.name = er6.value.ToString();
            list_encode_res.Add(er6);

            encode_res er7 = new encode_res();
            er7.value = 4000;
            er7.name = er7.value.ToString();
            list_encode_res.Add(er7);

            cb_link_res.ItemsSource = list_encode_res;

            sl_bitrate.Value = Tools.GetLinkBitrate();
            l_link_res.Content = Tools.GetLinkEncodingResolution();
            l_link_curve.Content = Tools.GetLinkDistortionCurve();
            #endregion

            #region ** OBSOLETE ** Check Admin mode -> Now Admin is default
            //Activate exe check if in Admin mode, or disable Advanced tab if not in Administrator mode.
            //if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            //{
            //    //grid_advanced.IsEnabled = false;
            //    b_add_exe.IsEnabled = false;
            //    b_del_exe.IsEnabled = false;
            //    lv_records.Visibility = Visibility.Hidden;
            //    tb_launcher.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    //Start exe check with Management Event. Admini required
            //    WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"); 
            //    ManagementEventWatcher watcher = new ManagementEventWatcher(query);
            //    watcher.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            //    watcher.Start();
            //}
            #endregion

            #region Check if Open Composite is available for use
            if (!OC.IsAvailable())
            {
                //Check is OC is actived
                if (OC.IsActive())
                {
                    cb_OC.IsChecked = true;
                }
                else cb_OC.IsChecked = false;

                cb_OC.Foreground = Brushes.Red;
                cb_OC.Content = "Open Composite is not available";
                cb_OC.IsEnabled = false;
            }
            #endregion

            #region Watcher initializzation
            ReadAppsCfg();
            //ManagementEventWatcher watcher = new ManagementEventWatcher(query);
            //watcher.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            #endregion

            
            //Check Dash SFX
            if (Tools.DashSFX(OculusInstallFolder, 1,false))
            {
                ck_sfx_status.IsChecked = true;
            }
            else ck_sfx_status.IsChecked = false;

            //check Dash black BG
            /*
            if (Tools.DashBackground(OculusInstallFolder, 0, false))
            {
                ck_blk_dash.IsChecked = false;
            }
            else ck_blk_dash.IsChecked = true;
            */

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
                Properties.Settings.Default.ASWsetting = 0;
            }
            else if (cb_ASW.SelectedIndex == 1)
            {
                Tools.SetASW(OculusInstallFolder, 1);
                Properties.Settings.Default.ASWsetting = 1;
            }
            else if (cb_ASW.SelectedIndex == 2)
            {
                Tools.SetASW(OculusInstallFolder, 2);
                Properties.Settings.Default.ASWsetting = 2;
            }
            else if (cb_ASW.SelectedIndex == 3)
            {
                Tools.SetASW(OculusInstallFolder, 3);
                Properties.Settings.Default.ASWsetting = 3;
            }
            else if (cb_ASW.SelectedIndex == 4)
            {
                Tools.SetASW(OculusInstallFolder, 4);
                Properties.Settings.Default.ASWsetting = 4;
            }
            else if (cb_ASW.SelectedIndex == 5)
            {
                Tools.SetASW(OculusInstallFolder, 5);
                Properties.Settings.Default.ASWsetting = 5;
            }

            Properties.Settings.Default.Save();
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
            else if (cb_debugHUD.SelectedIndex == 7) // Link
            {
                Tools.SetOSD(OculusInstallFolder, 7);
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

        private void b_watcher_Click(object sender, RoutedEventArgs e)
        {
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
            ManagementEventWatcher watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            watcher.Start();
            b_watcher.IsEnabled = false;
            b_watcher.Content = "Watcher actived...";
        }

        #endregion

        #region Service tab
        private async void B_stop_service_Click(object sender, RoutedEventArgs e)
        {
            b_stop_service.Content = "Service is Stopping...";
            grid_main.IsEnabled = false;
            await Tools.StopOculusService();
            b_stop_service.Content = "Stop Oculus Service";
        }

        private async void B_start_service_Click(object sender, RoutedEventArgs e)
        {
            b_start_service.Content = "Service is Starting...";
            await Tools.StartOculusService();
            b_start_service.Content = "Start Oculus Service";
            grid_main.IsEnabled = true;
        }

        private async void B_restart_service_Click(object sender, RoutedEventArgs e)
        {
            b_restart_service.Content = "Service is Stopping...";
            grid_main.IsEnabled = false;
            await Tools.StopOculusService();

            b_restart_service.Content = "Service is Starting...";
            await Tools.StartOculusService();

            b_restart_service.Content = "Restart Oculus Service";
            grid_main.IsEnabled = true;

        }
        #endregion

        #region Link tab
        private async void B_link_apply_Click(object sender, RoutedEventArgs e)
        {
            //encoding resolution
            if (cb_link_res.SelectedIndex != -1)
            {
                Tools.SetLinkEncodingResolution(list_encode_res[cb_link_res.SelectedIndex].value);
            }
            
            
            //distrorion curve
            if (cb_link_curve.SelectedIndex == 0) // DEFAULT
            {
                Tools.SetLinkDistortionCurve(-1);
            }
            else if (cb_link_curve.SelectedIndex == 1) // LOW
            {
                Tools.SetLinkDistortionCurve(0);
            }
            else if (cb_link_curve.SelectedIndex == 2) // HIGH
            {
                Tools.SetLinkDistortionCurve(1);
            }

            MessageBox.Show("Oculus Service need to restart");
            b_link_apply.IsEnabled = false;
            b_link_apply.Content = "Service is stopping...";
            await Tools.StopOculusService();
            b_link_apply.Content = "Service is starting...";
            await Tools.StartOculusService();
            b_link_apply.IsEnabled = true;
            b_link_apply.Content = "Apply";
            l_link_res.Content = Tools.GetLinkEncodingResolution();
            l_link_curve.Content = Tools.GetLinkDistortionCurve();

        }

        private void Sl_bitrate_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Tools.SetLinkBitrate((int)sl_bitrate.Value);
        }
        #endregion

        #region Open Composite tab
        private async void DownLoadOC()
        {
            //popup.IsOpen = true;
            b_dl_OC.IsEnabled = false;

            b_dl_OC.Content = "Check for Open Composite update";
            string latesthash = await OC.GetLatestHash();
            await Task.Delay(2000);

            if (latesthash is null)
            {
                b_dl_OC.Content = "Open Composite version Check: connection failed!";
                await Task.Delay(2000);
            }
            else if (!OC.CheckForVersion(latesthash))
            {
                b_dl_OC.Content = "New version available!";
                await Task.Delay(2000);
                b_dl_OC.Content = "Downloading Open Composite...";
                bool download = await OC.downloadDll();
                if (!download)
                {
                    b_dl_OC.Content = "Open Composite Download: connection failed!";
                    await Task.Delay(3000);
                    b_dl_OC.Content = "Download Open Composite";
                }
                else
                {
                    b_dl_OC.Content = "Open Composite is updated.";
                    await Task.Delay(3000);
                    b_dl_OC.Content = "Download Open Composite";
                }

            }

            //popup.IsOpen = false;

            if (!OC.IsAvailable())
            {
                cb_OC.Foreground = Brushes.Red;
                cb_OC.Content = "Open Composite not available";
                cb_OC.IsEnabled = false;
            }
            else
            {
                cb_OC.Foreground = Brushes.Black;
                cb_OC.Content = "Use Open Composite";
                cb_OC.IsEnabled = true;
            }

            b_dl_OC.IsEnabled = true;

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

        private void B_dl_OC_Click(object sender, RoutedEventArgs e)
        {
            DownLoadOC();
        }
        #endregion

        #region Misc tab

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
            b_back_lib.IsEnabled = false;

            b_lib.Content = "Oculus Service is stopping...";
            await Tools.StopOculusService();

            b_lib.Content = "Wait...";
            await Tools.RestoreLibrary(filename, OculusInstallFolder);

            b_lib.Content = "Oculus Service is starting...";
            await Tools.StartOculusService();

            b_lib.IsEnabled = true;
            b_back_lib.IsEnabled = true;
            b_lib.Content = "Restore Library";

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
            b_lib.IsEnabled = false;
            b_back_lib.Content = "Wait...";

            await Tools.BackupLibrary(OculusInstallFolder, ofd.FileName);

            b_back_lib.IsEnabled = true;
            b_lib.IsEnabled = true;
            b_back_lib.Content = "Backup Library";

        }

        private void Ck_sfx_status_Click(object sender, RoutedEventArgs e)
        {
            Tools.KillDash();

            if (ck_sfx_status.IsChecked == false)
            {
                Tools.DashSFX(OculusInstallFolder, 0, true);
            }
            else Tools.DashSFX(OculusInstallFolder, 1, true);

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

        private async void B_disable_oculus_Click(object sender, RoutedEventArgs e)
        {
            // start Steam VR
            Tools.SetNativeLibrary(OculusInstallFolder, false);
            grid_main.IsEnabled = false;
            int timeup = 20;

            for (int i = timeup; i >= 0; i--)
            {
                await Task.Delay(1000);
                b_disable_oculus.Content = i;
            }
            Tools.SetNativeLibrary(OculusInstallFolder, true);
            b_disable_oculus.Content = "Disable Oculus Library";

        }
        #endregion

        #region SteamVR tab
        private void B_aswp_on_Click(object sender, RoutedEventArgs e)
        {
            if (ASWPacemaker)
            {
                ASWPacemaker = false;
            }
            else
            {
                OpenVR.Shutdown();
                ASWPacemaker = true;
            }

            counter();
            
        }

        private async void counter()
        {
            if (ASWPacemaker)
            {
                var error = EVRInitError.None;
                OpenVR.Init(ref error, EVRApplicationType.VRApplication_Background);
                //OculusHack.ASWPacemaker.InitOpenVR();

                //Get frame time rendering of the compositor.
                Compositor_FrameTiming cft = new Compositor_FrameTiming();
                cft.m_nSize = (uint)Marshal.SizeOf(typeof(Compositor_FrameTiming));

                while (ASWPacemaker)
                {
                    await Task.Delay(200); // Delay of the polling check of frame timing
                    OpenVR.Compositor.GetFrameTiming(ref cft, 0);
                    tb_steamvr.Text = cft.m_flTotalRenderGpuMs.ToString() + "\n";

                    if (cft.m_flTotalRenderGpuMs > 11)
                    {
                        Tools.SetASW(OculusInstallFolder, 1); //TODO make a quicker method
                        tb_steamvr.Foreground = Brushes.DarkOrange;
                        await Task.Delay(500); // Delay before returning to normal rendering
                    }
                    else
                    {
                        Tools.SetASW(OculusInstallFolder, 0);
                        tb_steamvr.Foreground = Brushes.Black;
                    }
                }
            }

        }




        #endregion


        public void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
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

                    });

                }

            }
        }

        public class encode_res
        {
            public string name { get; set; }
            public int value { get; set; }
        }

        
    }
}

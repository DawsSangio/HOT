
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;
using System.Text;
using System;
using System.IO.Compression;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Permissions;

namespace OculusHack
{
    public class Manifest 
         

    {
        public string cname { get; set; }
        public string name { get; set; }
        public string exe { get; set; }
        public string steam_exe { get; set; }
        public string steam_id { get; set; }
        public string assets { get; set; }
        public int vr_mode { get; set; }
        

        public string CreateManifest()
        {
            string launchparameter = "steam://launch/" + this.steam_id + "/othervr";
           
            if (vr_mode == 1) launchparameter = "steam://launch/" + this.steam_id + "/vr";
            if (vr_mode == 2) launchparameter = "";

            string manifest =   "{\n\"canonicalName\":\"" +this.cname +"\",\n" +
                                "\"displayName\":\"" + this.name +"\",\n" +
                                "\"files\":{\n\""    + this.exe.Replace("\\", "\\\\") +"\":\"\"\n},\n" +
                                "\"firewallExceptionsRequired\":false,\n" +
                                "\"isCore\":false,\n"+
                                "\"launchFile\":\""         +this.steam_exe.Replace("\\", "\\\\").Replace("/","\\\\")   +"\",\n" +
                                "\"launchParameters\":\""   +launchparameter                                            +"\",\n" +
                                "\"manifestVersion\":0,\n" +
                                "\"packageType\":\"APP\",\n" +
                                "\"thirdParty\":true,\n" +
                                "\"version\":\"1\",\n" +
                                "\"versionCode\":1\n}";
            return manifest;
        }

        public string SteamVRManifest()
        {
            //steam://rungameid/250820
            string manifest = "{\n\"canonicalName\":\"" + this.cname + "\",\n" +
                                "\"displayName\":\"" + this.name + "\",\n" +
                                "\"files\":{\n\"" + this.steam_exe.Replace("\\", "\\\\").Replace("/", "\\\\") + "\":\"\"\n},\n" +
                                "\"firewallExceptionsRequired\":false,\n" +
                                "\"isCore\":false,\n" +
                                "\"launchFile\":\"" + this.steam_exe.Replace("\\", "\\\\").Replace("/", "\\\\") + "\",\n" +
                                "\"launchParameters\":\"steam://rungameid/250820\",\n" +
                                "\"manifestVersion\":0,\n" +
                                "\"packageType\":\"APP\",\n" +
                                "\"thirdParty\":true,\n" +
                                "\"version\":\"1\",\n" +
                                "\"versionCode\":1\n}";
            return manifest;
        }

        public string CreateAssets()
        {
           string assets =  "{\"dominantColor\": \"#0D0C19\",\n"+
                            "\"files\": {\n"+
                            "\"original.jpg\": \"0f94d358cc180d3c268f0acdc4a6be1196879f67be47a0a87609a3bf6e339bc2\",\n"+
                            "\"cover_landscape_image.jpg\": \"805d7bac0942adbb8a84cadda86af670da1dd3e0fced122c4ac6cbeba1d7d8eb\",\n" +
                            "\"cover_square_image.jpg\": \"426ac36b1071664d9825604cd2ed8ba6358022867e092a332668cb5cc9a82ef9\",\n" +
                            "\"icon_image.jpg\": \"9a88775296c0563ed81f0cf99bac742265136ba3f85dba17ba3e0673c16edf4c\",\n" +
                            "\"small_landscape_image.jpg\": \"8b6b456aeb3674e6b37daa0894144ea9d4fe79c9104a70c5fa1aa08b2c9fe796\",\n" +
                            "\"logo_transparent_image.png\": \"550a46ffcc4b905f4c3c03999ba0ea1850b0e5257bb7306974db2460e846eb3f\"\n"+
                            //"\"cover_landscape_image_large.png\": \"5543818ba507276532e4e4a06a1d01b7eee3abccbd5f8bc63540635b2637e6ef\",\n"+
                            "},\n" +
                            "\"packageType\": \"ASSET_BUNDLE\",\n"+
                            "\"isCore\": false,\n"+
                            "\"appId\": null,\n"+
                            "\"canonicalName\": \"" +this.cname +"_assets\",\n"+
                            "\"launchFile\": null,\n"+
                            "\"launchParameters\": null,\n"+
                            "\"launchFile2D\": null,\n"+
                            "\"launchParameters2D\": null,\n"+
                            "\"version\": \"1\",\n"+
                            "\"versionCode\": 1,\n"+
                            "\"redistributables\": null,\n"+
                            "\"firewallExceptionsRequired\": false,\n"+
                            "\"thirdParty\": true,\n"+
                            "\"manifestVersion\": 0\n}";
            return assets;
        }

        public string CreateCanonicalName(string exe)
        {
            string can = System.IO.Path.GetFileNameWithoutExtension(exe);
            can = can.Trim(' ');
            this.cname = can;
            return can;
        }

        public static void CreateImages(Manifest manifest, string OculusDataFolder)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile("https://steamcdn-a.opskins.media/steam/apps/" + manifest.steam_id + "/header.jpg", OculusDataFolder + "\\Software\\StoreAssets\\" + manifest.cname + "_assets\\original.jpg");
            }
            // cover_landscape_image.jpg    360*202
            // cover_square_image.jpg       360*360 used for dash menu and client
            // icon_image.jpg               192*192
            // logo_transparent_image.png   360*168 to match Steam banner aspect ratio
            // small_landscape_image.jpg    270*90  
            Image original = Image.FromFile(OculusDataFolder + "\\Software\\StoreAssets\\" + manifest.cname + "_assets\\original.jpg");
           

            Bitmap icon_image = new Bitmap(192, 192);
            Graphics icon_grp = Graphics.FromImage(icon_image);
            Rectangle icon_rec = new Rectangle(0, 51, 192, 90);
            icon_grp.DrawImage(original, icon_rec);
            
            icon_image.Save(OculusDataFolder + "\\Software\\StoreAssets\\" + manifest.cname + "_assets\\icon_image.jpg", ImageFormat.Jpeg);

            Bitmap cover_land = new Bitmap(original, 360, 202);
            cover_land.Save(OculusDataFolder + "\\Software\\StoreAssets\\" + manifest.cname + "_assets\\cover_landscape_image.jpg", ImageFormat.Jpeg);

            Bitmap cover_square = new Bitmap(360, 360);
            Graphics grp = Graphics.FromImage(cover_square);
            Rectangle rec = new Rectangle(0, 96, 360, 168);
            grp.DrawImage(original, rec);
            cover_square.Save(OculusDataFolder + "\\Software\\StoreAssets\\" + manifest.cname + "_assets\\cover_square_image.jpg", ImageFormat.Jpeg);

            Bitmap logo = new Bitmap(original, 360, 168); ;
            logo.Save(OculusDataFolder + "\\Software\\StoreAssets\\" + manifest.cname + "_assets\\logo_transparent_image.png", ImageFormat.Png);

            Bitmap small_land = new Bitmap(original, 270, 90);
            small_land.Save(OculusDataFolder + "\\Software\\StoreAssets\\" + manifest.cname + "_assets\\small_landscape_image.jpg", ImageFormat.Jpeg);

            original.Dispose();
        }
                
    }

    public static class Tools
    {
        #region Oculus folder 
        public static string GetOculusInstallFolder()
        {
            try
            {
                string key = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Oculus VR, LLC\\Oculus", "Base", null);
                return key;
            }
            catch (Exception)
            {

                return null;
            }
        }

        //TODO: fix me, get path form registry
        public static List<string> GetOculusLibraris()
        {
            List<string> libraries = new List<string>();
            Microsoft.Win32.RegistryKey subKeys = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Oculus VR, LLC\\Oculus\\Libraries");
            string[] lib = subKeys.GetSubKeyNames();
            foreach (string entry in lib)
            {
                string key = (string)Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\Oculus VR, LLC\\Oculus\\Libraries\\" + entry, "Path",null);
                libraries.Add(key);

            }
            return libraries;

        }
        #endregion

        #region Oculus Service
        public static Task<bool> StartOculusService()
        {
            return Task.Run(() =>
            {
                //ServiceController OVRService = new ServiceController("OVRService");
                //if (OVRService.Status == ServiceControllerStatus.Stopped)
                //{
                //    OVRService.Start();
                //    OVRService.WaitForStatus(ServiceControllerStatus.Running);
                //}
                //return true;

                Process process = new Process();
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.FileName = "net";
                process.StartInfo.Arguments = "start \"OVRService\"";
                process.StartInfo.Verb = "runas"; //run as admin
                process.Start();
                process.WaitForExit();
                return true;
            });
        }

        public static Task<bool> StopOculusService()
        {
            return Task.Run(() =>
            {
                //ServiceController OVRService = new ServiceController("OVRService");
                //if (OVRService.Status == ServiceControllerStatus.Running)
                //{
                //    OVRService.Stop();
                //    OVRService.WaitForStatus(ServiceControllerStatus.Stopped);
                //}
                //return true;
                Process process = new Process();
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.FileName = "net";
                process.StartInfo.Arguments = "stop \"OVRService\"";
                process.StartInfo.Verb = "runas"; //run as admin
                process.Start();
                process.WaitForExit();
                return true;
            });
        }

        public static bool IsOculusServiceRunning()
        {
            ServiceController OVRService = new ServiceController("OVRService");
            if (OVRService.Status == ServiceControllerStatus.Running)
            {
                return true;
            }
            else return false;
        }
        #endregion

        #region Oculus Home enviroment
                
        /// <summary>
        /// Check Oculus Home enviroment status, with optional update
        /// mode: 1 = home enable, 0 = home disable
        /// </summary>
        public static bool OculusHome(string OculusInstallFolder, int mode, bool update)
        {

            string home_file = OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.exe";
            string home_bkup_file = OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.ex_";

            if (File.Exists(home_file) && mode == 1)
            {
                return true;
            }
            else if (File.Exists(home_file) && mode == 0)
            {
                if (update)
                {
                    //disable
                    File.Copy(home_file, home_bkup_file, true);
                    File.Delete(home_file);
                }
                return false;
            }
            else if (!File.Exists(home_file) && File.Exists(home_bkup_file) && mode == 1)
            {
                if (update)
                {
                    //enable
                    File.Move(home_bkup_file, home_file);
                }
                return false;
            }
            else if (!File.Exists(home_file) && mode == 0)
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Check Dash Sound FX status, with optional update
        /// mode: 1 = sound on, 0 = sound off
        /// </summary>
        public static bool DashSFX(string OculusInstallFolder, int mode, bool update)
        {
            string bank_file = OculusInstallFolder + "\\Support\\oculus-dash\\dash\\assets\\raw\\audio_dash\\Build\\Desktop\\Master Bank.bank";
            string bank_bkup_file = OculusInstallFolder + "\\Support\\oculus-dash\\dash\\assets\\raw\\audio_dash\\Build\\Desktop\\Master Bank.ban_";

            if (File.Exists(bank_file) && mode == 1)
            {
                return true;
            }
            else if (File.Exists(bank_file) && mode == 0)
            {
                if (update)
                {
                    //disable
                    File.Copy(bank_file, bank_bkup_file, true);
                    File.Delete(bank_file);
                }
                return false;
            }
            else if (File.Exists(bank_bkup_file) && mode == 1)
            {
                if (update)
                {
                    //enable
                    File.Move(bank_bkup_file, bank_file);
                }
                return false;
            }
            else if (File.Exists(bank_bkup_file) && mode == 0)
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Check Dash background status, with optional update
        /// 0 = white, 1 = black
        /// </summary>
        public static bool DashBackground(string OculusInstallFolder, int mode, bool update)
        {
            string file = OculusInstallFolder + "\\Support\\oculus-dash\\dash\\assets\\raw\\materials\\environment\\the_void\\the_void_new.material";
            string txt = File.ReadAllText(file);
            if (txt.Contains("6") && mode == 0)
            {
                return true;
            }
            else if (txt.Contains("6") && mode == 1)
            {
                if (update)
                {
                    File.WriteAllText(file, txt.Replace("006", "007"));
                }
                return false;
            }
            else if (txt.Contains("7") && mode == 1)
            {
                return true;
            }
            else if (txt.Contains("7") && mode == 0)
            {
                if (update)
                {
                    File.WriteAllText(file, txt.Replace("007", "006"));
                }
                return false;
            }
            else return false;
           
           
        }

        public static void KillOculusHome()
        {
            foreach (Process pname in Process.GetProcessesByName("Home2-Win64-Shipping"))
            {
                pname.Kill();
                pname.WaitForExit();
            }

        }

        public static void KillDash()
        {
            foreach (Process pname in Process.GetProcessesByName("OculusDash"))
            {
                pname.Kill();
                pname.WaitForExit();
            }

        }

        public static void KillDesktopClient()
        {
            foreach (Process pname in Process.GetProcessesByName("OculusClient"))
            {
                pname.Kill();
                pname.WaitForExit();
            }
        }

        #endregion

        #region Debugtools

        public static bool SetSS(string OculusInstallFolder, double ss)
        {
            if (ss == 1 || ss == 1.00)
            {
                ss = 0;
            }
            File.CreateText("cmd").Dispose();
            string file = "cmd";
            File.WriteAllText(file, "service set-pixels-per-display-pixel-override " + ss.ToString().Replace(",", ".") + "\nexit", Encoding.Default);
            try
            {
                Process debugtool = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.WorkingDirectory = OculusInstallFolder + "\\Support\\oculus-diagnostics";
                startInfo.FileName = OculusInstallFolder + "\\Support\\oculus-diagnostics\\OculusDebugToolCLI.exe";
                string args = " -f " + "\"" + Directory.GetCurrentDirectory() + "\\cmd\"";
                startInfo.Arguments = args;
                debugtool.StartInfo = startInfo;
                debugtool.Start();
                debugtool.WaitForExit();
                File.Delete(file);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
            
        }

        /// <summary>
        /// Set the Debug OSD.
        /// "mode: 0=Off, 1=Perfomance summary, 2=Latency timing, 3=Application render timing, 4=Compositor render timing, 6=AWS status, 10=Layer info 
        /// </summary>
        public static bool SetOSD(string OculusInstallFolder, int mode)
        {
            File.CreateText("cmd").Dispose();
            string file = "cmd";

            if (mode == 10)//Layer mode
            {
                File.WriteAllText(file, "perfhud set-mode 0\n" +
                                        "layerhud set-mode 1\nexit", Encoding.Default);
            }
            else
            {
                File.WriteAllText(file, "layerhud reset\nperfhud set-mode " + mode + "\nexit", Encoding.Default);
            }

            try
            {
                Process debugtool = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.WorkingDirectory = OculusInstallFolder + "\\Support\\oculus-diagnostics";
                startInfo.FileName = OculusInstallFolder + "\\Support\\oculus-diagnostics\\OculusDebugToolCLI.exe";
                startInfo.Arguments = " -f " + "\"" + Directory.GetCurrentDirectory() + "\\cmd\"";
                debugtool.StartInfo = startInfo;
                debugtool.Start();
                debugtool.WaitForExit();
                File.Delete(file);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            

        }

        /// <summary>
        /// Set ASW mode.
        /// 0 = Auto, 1 = 45fps(AWS), 2 = 45fps(no AWS), 3 = Off, 4 = 30fps(ASW) 
        /// </summary>
        public static bool SetASW(string OculusInstallFolder, int mode)
        {
            string asw;
            if (mode == 4)
            {
                asw = "asw.Clock30";
            }
            else if (mode == 3)
            {
                asw = "asw.Off";
            }
            else if (mode == 2)
            {
                asw = "asw.Sim45"; //NO ASW
            }
            else if (mode == 1)
            {
                asw = "asw.Clock45";
            }
            else asw = "asw.Auto";

            File.CreateText("cmd").Dispose();
            string file = "cmd";
            File.WriteAllText(file, "server:" +asw +"\nexit", Encoding.Default);

            try
            {
                Process debugtool = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.WorkingDirectory = OculusInstallFolder + "\\Support\\oculus-diagnostics";
                startInfo.FileName = OculusInstallFolder + "\\Support\\oculus-diagnostics\\OculusDebugToolCLI.exe";
                startInfo.Arguments = " -f " + "\"" + Directory.GetCurrentDirectory() + "\\cmd\"";
                debugtool.StartInfo = startInfo;
                debugtool.Start();
                debugtool.WaitForExit();
                File.Delete(file);
                return true;
            }
            catch (Exception)
            {
                return false;
                
            }
            

        }
        #endregion

        #region Manage Runtime Library

        ///<summary>
        ///Retrun Oculus native library status based on active request: true or false
        ///</summary>
        public static bool SetNativeLibrary(string OculusInstallFolder, bool active)
        {
            try
            {
                if (active)
                {
                    File.Move(OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT32_1.dl_",
                        OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT32_1.dll");
                    File.Move(OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT64_1.dl_",
                                OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT64_1.dll");
                    return true;
                }
                else 
                {
                    File.Move(OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT32_1.dll",
                                OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT32_1.dl_");
                    File.Move(OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT64_1.dll",
                                OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT64_1.dl_");
                    return true;
                }

            }
            catch (FileNotFoundException)
            {
                
                return false;
            }
            

        }

        
        /// <summary>
        /// Return Oculus Library version, major release.
        /// </summary>
        public static string GetLibVersion(string OculusInstallFolder)
        {
            
            FileVersionInfo fa = FileVersionInfo.GetVersionInfo(OculusInstallFolder+ "\\support\\oculus-runtime\\OVRServer_x64.exe");
            
            string version = fa.FileMajorPart +"."+fa.FileMinorPart + "." + fa.FileBuildPart + "." + fa.FilePrivatePart;
            return version;

        }

        /// <summary>
        /// Backup, to a zip file, the main core Oculus library
        /// </summary>
        public static Task BackupLibrary(string OculusInstallFolder, string zipfile)
        {
            /*string[] libraryfiles = {
                "oculus-diagnostics\\OculusDebugTool.exe",
                "oculus-diagnostics\\OculusDebugToolCLI.exe",
                "oculus-diagnostics\\OculusLogGatherer.exe",
                "oculus-diagnostics\\OculusMirror.exe",
                               
                "oculus-drivers\\oculus-driver.exe",
                "oculus-drivers\\version.txt",

                "oculus-runtime\\Firmware\\firmware.zip",
                "oculus-runtime\\Firmware\\firmware_km.zip",

                "oculus-runtime\\LibOVRP2P32_1.dll",
                "oculus-runtime\\LibOVRP2P32_1.dll",
                "oculus-runtime\\LibOVRP2P64_1.dll",
                "oculus-runtime\\LibOVRPlatform32_1.dll",
                "oculus-runtime\\LibOVRPlatform64_1.dll",
                "oculus-runtime\\LibOVRRT32_1.dll",
                "oculus-runtime\\LibOVRRT64_1.dll",
                "oculus-runtime\\OculusAppFramework.dll",
                "oculus-runtime\\OVRRedir.exe",
                "oculus-runtime\\OVRServer_x64.exe",
                "oculus-runtime\\OVRServiceLauncher.exe"
            };*/
            List<string> libraryfiles = new List<string>();
            void addFileToLibrary(string[] files)
            {
                foreach (string file in files)
                {
                    libraryfiles.Add(file);
                }
            }

            addFileToLibrary(Directory.GetFiles(OculusInstallFolder + "Support\\oculus-diagnostics", "*.*", SearchOption.AllDirectories));
            addFileToLibrary(Directory.GetFiles(OculusInstallFolder + "Support\\oculus-drivers", "*.*", SearchOption.AllDirectories));
            addFileToLibrary(Directory.GetFiles(OculusInstallFolder + "Support\\oculus-runtime", "*.*", SearchOption.AllDirectories));

            string filename = zipfile;
            File.Create(filename).Dispose();
            return Task.Run(() =>
            {
                using (FileStream zipToOpen = new FileStream(filename, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        foreach (string file in libraryfiles)
                        {
                            //ZipArchiveEntry entry = archive.CreateEntryFromFile(OculusInstallFolder + "Support\\" + file, file);
                            ZipArchiveEntry entry = archive.CreateEntryFromFile(file, file.Replace(OculusInstallFolder + "Support\\",""));

                        }
                        archive.Dispose();
                    }
                }
            });
            
        }
        
        /// <summary>
        /// Restore the main core Oculus library from a zipfile
        /// </summary>
        public static Task<bool> RestoreLibrary(string filename, string OculusInstallFolder)
        {
            //Delete orignal file before replacing from archive.zip
            ZipArchive archive = ZipFile.OpenRead(filename);
            return Task.Run(() =>
            {
                foreach (var file in archive.Entries)
                {
                    if (!file.ToString().EndsWith("/"))
                    {
                        File.Delete(OculusInstallFolder + "\\Support\\" + file);
                    }
                }
                archive.Dispose();
                ZipFile.ExtractToDirectory(filename, OculusInstallFolder + "\\Support\\");
                return true;
            });
        }

        #endregion

        #region Guardian
        /// <summary>
        /// Probably outdated since 1.31 runtime
        /// </summary>
        /// <param name="OculusInstallFolder"></param>
        public static void EnableGuardian(string OculusInstallFolder)
        {
            Process proc = new Process();
            ProcessStartInfo pinfo = new ProcessStartInfo(OculusInstallFolder + "Support\\oculus-overlays\\oculus-overlays.exe");
            proc.StartInfo = pinfo;
            proc.Start();
        }
        #endregion

        public static bool IsOculusClientRunning()
        {

            var pname = Process.GetProcessesByName("OculusClient");
            if (pname.Length == 0) return false;
            else return true;

        }

        public static bool IsSteamVRRunning()
        {
            var pname = Process.GetProcessesByName("vrmonitor");
            pname = Process.GetProcessesByName("vrserver");
            if (pname.Length == 0) return false;
            else return true;
        }
        


                  
               
    }


}
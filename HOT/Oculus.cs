
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System;
using System.IO.Compression;
using System.Collections.Generic;

namespace Oculus
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
           string assets =  "{\"dominantColor\": \"#0D0C19\",\n"+ //TODO: colore di fondo ?
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

        //public static OculusManifest ReadOculusApps()
        //{
        //    OculusManifest manifest = new OculusManifest();

        //    return manifest;
        //}


    }

    public static class Tools
    {
        
        ///<summary>
        ///Check the existance of "OculusSetup.exe", "OculusDash.exe" and libOVR, in the given folder.
        ///</summary>   
        public static bool CheckOculusInstallFolder(string OculusInstallFolder)
        {
            if (File.Exists(OculusInstallFolder + "\\OculusSetup.exe")
                && File.Exists(OculusInstallFolder + "\\Support\\oculus-runtime\\LibOVRRT64_1.dll"))
            { 
            return true;
            }
            else return false;
        }

        public static List<string> GetOculusLibraris()
        {
            List<string> libraries = new List<string>();
            Microsoft.Win32.RegistryKey subKeys = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Oculus VR, LLC\\Oculus\\Libraries");
            string[] lib = subKeys.GetSubKeyNames();
            foreach (string entry in lib)
            {
                string key = (string)Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\Oculus VR, LLC\\Oculus\\Libraries\\" + entry, "Path",null);
                //TODO pulire percorso
                libraries.Add(key);

            }
            return libraries;

        }

        #region Oculus Service
        public static void StartOculusService()
        {

            ServiceController OVRService = new ServiceController("OVRService");
            if (OVRService.Status== ServiceControllerStatus.Stopped)
            {
                OVRService.Start();
                OVRService.WaitForStatus(ServiceControllerStatus.Running);
            }
        }

        public static int StopOculusService()
        {
            ServiceController OVRService = new ServiceController("OVRService");
            if (OVRService.Status == ServiceControllerStatus.Running)
            {
                OVRService.Stop();
                OVRService.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            return 0;
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
        ///<summary>
        ///Check the existance of "Home2-Win64-Shipping.exe" and the size is more than 60Mbyte.
        ///Retrun: 2 active, 1 hidden, 0 not found
        ///</summary>
        //public static int GetHomeStatus(string OculusInstallFolder)
        //{
        //    string[] files = Directory.GetFiles(OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\", "*.*", SearchOption.AllDirectories);
                        
        //    foreach (string file in files)
        //    {
        //        FileInfo fileinfo = new FileInfo(file);
        //        if (fileinfo.Length > 60000000 && file == OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.exe")
        //        {
        //            return 2; //is active
        //        }
        //        else if (fileinfo.Length > 60000000 && file == OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.ex_")
        //        {
        //            return 1; //is hidden
        //        }
        //        else if (fileinfo.Length > 60000000)
        //        {
        //            File.Copy(file, OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.ex_", true);
        //            return 1; //is hidden
        //        }

        //    }
        //    return 0; //not found
        //}

        ///<summary>
        ///Make backup of Home2-Win64-Shipping.exe renaming *.ex_
        ///</summary>
        //public static void BackupHome2exe(string OculusInstallFolder)
        //{

        //    File.Copy(  OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.exe",
        //                OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.ex_",
        //                true);
          
        //}
       
        public static void DisableHome(string OculusInstallFolder)
        {
            
            foreach (Process pname in Process.GetProcessesByName("Home2-Win64-Shipping"))
            {
                pname.Kill();
                pname.WaitForExit();
            }

            File.Move(OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.exe",
                   OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.ex_");

        }

        public static void EnableHome(string OculusInstallFolder)
        {
            File.Move(OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.ex_",
                   OculusInstallFolder + "\\Support\\oculus-worlds\\Home2\\Binaries\\Win64\\Home2-Win64-Shipping.exe");
            
            Process home = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = OculusInstallFolder + "Support\\oculus-worlds\\Home2\\Binaries\\Win64\\";
            startInfo.FileName = "Home2-Win64-Shipping.exe";
            home.StartInfo = startInfo;
            home.Start();
        }
               
        #endregion

        #region Oculus core library
        ///<summary>
        ///Check the existance of "LibOVRRT32_1.dll" and "LibOVRRT32_1.dll"
        ///</summary>
        public static bool IsOculusLibraryEnable(string OculusInstallFolder)
                {
                    // TODO need improvement to check file not found
                    if (File.Exists(OculusInstallFolder + "\\Support\\oculus-runtime\\LibOVRRT32_1.dll") &&
                         File.Exists(OculusInstallFolder + "\\Support\\oculus-runtime\\LibOVRRT64_1.dll"))
                    {
                        return true;
                    }
                    else return false;
                }

        ///<summary>
        ///Disable Oculus native library renaming "LibOVRRT" to hide them
        ///</summary>
        public static void DisableOculusLibrary(string OculusInstallFolder)
        {
                File.Move(  OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT32_1.dll",
                            OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT32_1.dl_");
                File.Move(  OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT64_1.dll",
                            OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT64_1.dl_");
        }

        ///<summary>
        ///Enable Oculus native library renaming "LibOVRRT" .dl_ to .dll to show them
        ///</summary>
        public static void EnableOculusLibrary(string OculusInstallFolder)
        {
                File.Move(  OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT32_1.dl_",
                        OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT32_1.dll");
            File.Move(  OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT64_1.dl_",
                        OculusInstallFolder + "\\support\\oculus-runtime\\LibOVRRT64_1.dll");
        }
        #endregion

        #region Dash SFX
        ///<summary>
        ///Disable Dash background "humming" sfx. 
        ///</summary>
        public static void DisableDashSFX(string OculusInstallFolder)
        {
            KillDash();
            File.Move(OculusInstallFolder + "\\Support\\oculus-dash\\dash\\assets\\raw\\audio_dash\\Build\\Desktop\\Master Bank.bank",
                        OculusInstallFolder + "\\Support\\oculus-dash\\dash\\assets\\raw\\audio_dash\\Build\\Desktop\\Master Bank.ban_");
        }

        ///<summary>
        ///Enable Dash background "humming" sfx. 
        ///</summary>
        public static void EnableDashSFX(string OculusInstallFolder)
        {
            File.Move(OculusInstallFolder + "\\Support\\oculus-dash\\dash\\assets\\raw\\audio_dash\\Build\\Desktop\\Master Bank.ban_",
                      OculusInstallFolder + "\\Support\\oculus-dash\\dash\\assets\\raw\\audio_dash\\Build\\Desktop\\Master Bank.bank");
            KillDash();
        }
        #endregion

        #region Debugtools
        //TODO fix access denied, allow to run without admin rights, "cmd" must be created near program exe
        public static void SetDebugToolSS(string OculusInstallFolder, double ss)
        {
            if (ss == 1)
            {
                ss = 0;
            }
            File.CreateText("cmd").Dispose();
            string file = "cmd";
            File.WriteAllText(file, "service set-pixels-per-display-pixel-override " + ss.ToString().Replace(",", ".") + "\nexit", Encoding.Default);

            Process debugtool = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = OculusInstallFolder + "Support\\oculus-diagnostics\\";
            startInfo.FileName = "OculusDebugToolCLI.exe";
            string args = " -f " + "\"" + Directory.GetCurrentDirectory() + "\\cmd\"";
            startInfo.Arguments = args;
            debugtool.StartInfo = startInfo;
            debugtool.Start();
            debugtool.WaitForExit();
            File.Delete(file);

        }

        public static void SetDebugOSD(string OculusInstallFolder, int mode)
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

            Process debugtool = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = OculusInstallFolder + "Support\\oculus-diagnostics\\";
            startInfo.FileName = "OculusDebugToolCLI.exe";
            startInfo.Arguments = " -f " + "\"" + Directory.GetCurrentDirectory() + "\\cmd\"";
            debugtool.StartInfo = startInfo;
            debugtool.Start();
            debugtool.WaitForExit();
            File.Delete(file);

        }

        //ASW mode: 0=auto, 1=45 on, 2=45 off, 3=off
        public static void SetASW(string OculusInstallFolder, int mode)
        {
            string asw;
            if (mode == 3)
            {
                asw = "asw.Off";
            }
            else if (mode == 2)
            {
                asw = "asw.Sim45";
            }
            else if (mode == 1)
            {
                asw = "asw.Clock45";
            }
            else asw = "asw.Auto";

            File.CreateText("cmd").Dispose();
            string file = "cmd";

          
            File.WriteAllText(file, "server:" +asw +"\nexit", Encoding.Default);

            Process debugtool = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = OculusInstallFolder + "Support\\oculus-diagnostics\\";
            startInfo.FileName = "OculusDebugToolCLI.exe";
            startInfo.Arguments = " -f " + "\"" + Directory.GetCurrentDirectory() + "\\cmd\"";
            debugtool.StartInfo = startInfo;
            debugtool.Start();
            debugtool.WaitForExit();
            File.Delete(file);

        }
        #endregion

        #region Manage Runtime Library

        public static string GetLibVersion(string OculusInstallFolder)
        {
            
            FileVersionInfo fa = FileVersionInfo.GetVersionInfo(OculusInstallFolder+ "support\\oculus-runtime\\OVRServer_x64.exe");
            string version = fa.ProductMajorPart +"."+fa.ProductMinorPart;
            return version;

        }

        public static void BackupLibrary(string OculusInstallFolder)
        {
            string[] libraryfiles = {

                "oculus-diagnostics\\OculusDebugTool.exe",
                "oculus-diagnostics\\OculusDebugToolCLI.exe",
                "oculus-diagnostics\\OculusLogGatherer.exe",
                "oculus-diagnostics\\OculusMirror.exe",

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
            };
            string filename = GetLibVersion(OculusInstallFolder) + ".zip";
            File.Create(filename).Dispose(); 
                using (FileStream zipToOpen = new FileStream(filename, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        foreach (string file in libraryfiles)
                        {
                            ZipArchiveEntry entry = archive.CreateEntryFromFile(OculusInstallFolder + "Support\\"+file, file);
                        }
                        archive.Dispose();
                    
                    }
                   
                }
        }

        public static void RestoreLibrary(string filename, string OculusInstallFolder)
        {
            //Delete orignal file before replacing from archive.zip
            ZipArchive archive = ZipFile.OpenRead(filename);
            foreach (var file in archive.Entries)
            {
                if (!file.ToString().EndsWith("/"))
                {
                    File.Delete(OculusInstallFolder + "Support\\" + file);
                }
            }
            archive.Dispose();
            
            ZipFile.ExtractToDirectory(filename, OculusInstallFolder+"\\Support\\");
        }



        #endregion

        #region Guardian
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
                    
        
    }

   

}
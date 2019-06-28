using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace OculusHack
{
    /// <summary>
    /// Static class with methods to manage Open Composite
    /// </summary>
    public static class OC
    {
        //TODO: fix the management of the .vrpath modification on the go.
        public static void EnableOC()
        {
            List<string> cfgparts = ReadSteamvrCfg();
            string oc = "\"" + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("\\", "\\\\") + "\\\\OculusHack\"";
            //Remove also original OpenComposite entry if present, need to not destroy StreamVr Dll structure.
            for (int i = 2; i < cfgparts.Count; i++)
            {
                if (!cfgparts[i].Contains("\\SteamVR"))
                {
                    cfgparts.RemoveAt(i);
                    i--;
                }
            }
            cfgparts.Insert(2, oc);
            WriteSteamvrCfg(cfgparts);
        }


        public static void DisableOC()
        {
            List<string> cfgparts = ReadSteamvrCfg();

            //Remove every line which do not contain SteamVR folder reference
            for (int i = 2; i < cfgparts.Count; i++)
            {
                if (!cfgparts[i].Contains("\\SteamVR"))
                {
                    cfgparts.RemoveAt(i);
                    i--;
                }
            }


            WriteSteamvrCfg(cfgparts);
        }

        public static bool IsOCactive()
        {
            List<string> cfgparts = ReadSteamvrCfg();
            if (cfgparts[2].Contains("OculusHack"))
            {
                return true;
            }
            else return false;

        }


        /// <summary>
        /// Read SteamVR cfg trimming space, tabs and \n \r
        /// 0 - initial part
        /// 1 - end part
        /// 2... other dll folder entry.
        /// </summary>
        public static List<string> ReadSteamvrCfg()
        {
            List<string> cfg_parts = new List<string>();

            string file = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\openvr\\openvrpaths.vrpath";
            string txt = File.ReadAllText(file);
            int idx_runtime = txt.IndexOf("runtime") + 7;
            int idx_runtime_start = txt.IndexOf("[", idx_runtime) + 1;
            int idx_runtime_end = txt.IndexOf("]", idx_runtime_start);

            string txt_start = txt.Substring(0, idx_runtime_start);
            cfg_parts.Add(txt_start);

            string txt_end = txt.Substring(idx_runtime_end);
            cfg_parts.Add(txt_end);

            string runtimes = txt.Substring(idx_runtime_start, idx_runtime_end - idx_runtime_start).Replace("\n", "").Replace("\r", "").Replace("\t", "");
            string[] runtime = runtimes.Split(',');

            foreach (string run in runtime)
            {
                cfg_parts.Add(run.Trim());
            }

            return cfg_parts;
        }

        public static void WriteSteamvrCfg(List<string> cfgparts)
        {
            string file = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\openvr\\openvrpaths.vrpath";

            //write initial part
            File.WriteAllText(file, cfgparts[0] + "\r\n");

            //write other folder
            for (int i = 2; i < cfgparts.Count; i++)
            {
                string comma = ",";
                if (i == cfgparts.Count - 1)
                {
                    comma = "";
                }
                File.AppendAllText(file, "    " + cfgparts[i] + comma + "\r\n");
            }

            //write end part
            File.AppendAllText(file, "  " + cfgparts[1]);

        }

        /// <summary>
        /// Download OpenComposite dll to [user]\appdata\local\OculusHack\bin
        /// </summary>
        public static void downloadDll()
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin");
            string destfile_86 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin\\vrclient.dll";
            string destfile_64 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin\\vrclient_x64.dll";



            if (!File.Exists(destfile_86) || !File.Exists(destfile_64))
            {

                //TODO: check for internet connection
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile("https://znix.xyz/OpenComposite/download.php?arch=x86", destfile_86);
                    webClient.DownloadFile("https://znix.xyz/OpenComposite/download.php?arch=x64", destfile_64);
                }

            }


        }

        public static bool CheckForUpdate()
        {
            return false;
        }

        public static string GetLatestHash()
        {
            string id = "";

            using (WebClient webClient = new WebClient())
            {
                //await wc.DownloadStringTaskAsync("https://gitlab.com/api/v4/projects/znixian%2fOpenOVR/repository/commits?per_page=1");
                //Stream id = new Stream(webClient.OpenReadAsync("https://gitlab.com/api/v4/projects/znixian%2fOpenOVR/repository/commits?per_page=1"));

                //Commit commit = JsonConvert.DeserializeObject<List<Commit>>(json).First();

                return id;
            }
        }

    }

}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace OculusHack
{
    /// <summary>
    /// Static class with methods to manage Open Composite
    /// </summary>
    public static class OC
    {
        // This part is very hacky..
        // TODO: use Json pharser.
        public static bool IsActive()
        {
            try
            {
                List<string> cfgparts = ReadSteamvrCfg();
                for (int i = 2; i < cfgparts.Count; i++)
                {
                    if (cfgparts[i].Contains("\\OculusHack"))
                    {
                        return true;
                    }
                    else if (cfgparts[i].Contains("\\SteamVR"))
                    {
                        return false;
                    }
                }
                return false;
            }
            catch (Exception)
            {

                return false;

            }

        }

        public static bool IsAvailable()
        {

            string OCapi64 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin\\vrclient_x64.dll";
            string OCapi = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin\\vrclient.dll";
            if (File.Exists(OCapi) && File.Exists(OCapi64))
            {
                return true;
            }
            else return false;

        }

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

        public static void EnableLocalOC(string exepath)
        {
            string path = Path.GetDirectoryName(exepath);
            string[] apis = Directory.GetFiles(path, "openvr_api.dll",SearchOption.AllDirectories);
            string OCapi = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin\\vrclient_x64.dll";

            foreach (string api in apis)
            {
                File.Copy(api, Path.GetDirectoryName(api) + "\\openvr_api.original",true);
                File.Copy(OCapi, api, true);
            }
        }

        public static void DisableLocalOC(string exepath)
        {
            string path = Path.GetDirectoryName(exepath);
            string[] apis = Directory.GetFiles(path, "openvr_api.original", SearchOption.AllDirectories);

            foreach (string api in apis)
            {
                File.Copy(api, Path.GetDirectoryName(api) + "\\openvr_api.dll", true);
            }
        }

        /// <summary>
        /// Download OpenComposite dll and version.txt to [user]\appdata\local\OculusHack\bin
        /// </summary>
        public static async Task<bool> downloadDll()
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin");
            string tempfile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin\\temp.dll";
            string destfile_86 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin\\vrclient.dll";
            string destfile_64 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin\\vrclient_x64.dll";
            string version = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin\\version.txt";

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    await webClient.DownloadFileTaskAsync("https://znix.xyz/OpenComposite/download.php?arch=x86", tempfile);
                    //await webClient.DownloadFileTaskAsync("https://znix.xyz/OpenComposite/download.php?arch=x64", destfile_64);
                }
                File.Copy(tempfile, destfile_86,true);

                using (WebClient webClient = new WebClient())
                {
                    //await webClient.DownloadFileTaskAsync("https://znix.xyz/OpenComposite/download.php?arch=x86", destfile_86);
                    await webClient.DownloadFileTaskAsync("https://znix.xyz/OpenComposite/download.php?arch=x64", tempfile);
                }
                File.Copy(tempfile, destfile_64, true);

                File.Delete(tempfile);

                File.Create(version).Dispose();
                File.WriteAllText(version, await GetLatestHash());

                return true;

            }
            catch (WebException)
            {
                File.Delete(tempfile);
                return false;
            }

        }

        public static bool CheckForVersion(string newVersionHash)
        {
            try
            {
                string _old = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OculusHack\\bin\\version.txt");
                if (_old == newVersionHash)
                {
                    return true;
                }
                else return false;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public static async Task<string> GetLatestHash()
        {
            string hash;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    hash = await webClient.DownloadStringTaskAsync("https://gitlab.com/api/v4/projects/znixian%2fOpenOVR/repository/commits?per_page=1");
                }
                return hash.Substring(hash.IndexOf("\"id\":\"") + 6, hash.IndexOf("\",\"short_id\"") - 8);
            }
            catch (WebException)
            {
                return null;
                
            }
            
        }

    }

}

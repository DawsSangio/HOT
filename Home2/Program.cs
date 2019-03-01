using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OculusHack
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            SteamVR:    steam://rungameid/250820
            Steam Home: steam://rungameid/
            */
            try
            {
                string cmd = File.ReadAllText("movehome.ini");
                System.Diagnostics.Process.Start(cmd);

            }
            catch
            {
                
            }

        }
    }
}

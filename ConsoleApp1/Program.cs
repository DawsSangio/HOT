using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
using System.Timers;
using OpenTK;
using OculusWrap;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string arg in args)
                {
                Console.WriteLine(arg.ToString());

            }



            //Process find testin
            ////void WaitForProcess()
            ////{
            //ManagementEventWatcher startWatch = new ManagementEventWatcher(
            //    new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            //startWatch.EventArrived
            //                    += new EventArrivedEventHandler(startWatch_EventArrived);
            //startWatch.Start();

            //ManagementEventWatcher stopWatch = new ManagementEventWatcher(
            //    new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            //stopWatch.EventArrived
            //                    += new EventArrivedEventHandler(stopWatch_EventArrived);
            //stopWatch.Start();
            ////}

            //void stopWatch_EventArrived(object sender, EventArrivedEventArgs e)
            //{
            //    stopWatch.Stop();
            //    Console.WriteLine("Process stopped: {0}"
            //                      , e.NewEvent.Properties["ProcessName"].Value);
            //}

            //void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
            //{
            //    startWatch.Stop();
            //    Console.WriteLine("Process started: {0}"
            //                      , e.NewEvent.Properties["ProcessName"].Value);
            //}



            //TIEMR Testing
            //int t = 5;
            //Timer timer = new Timer(1000);
            //timer.Start();
            //timer.Elapsed += finito;

            //void finito(Object source, ElapsedEventArgs e)
            //{
            //  Console.WriteLine(t +" secondi");
            //   t--;
            //    if (t < 0)
            //    {
            //        timer.Stop();
            //        Console.WriteLine("Finito!");
            //    }
            //}
            //for (int t = 20; t > 0; t--)
            //{
            //    Console.WriteLine(t + " secondi");
            //    Thread.Sleep(1000);
            //}
            //Console.WriteLine("Finito!");


            // Oculus wrap testing
            // Console.WriteLine(OVR.VersionString);

            ////Open.TK
            //OculusRift rift = new OculusRift();
            //Console.WriteLine("HMD is connected: " + rift.IsConnected);
            //Console.WriteLine("IPD is: " + rift.InterpupillaryDistance);
            //Console.WriteLine("H resulution is: " + rift.HResolution);
            //Console.WriteLine("V resolution is: " + rift.VResolution);

            ////OculusWrap
            //Wrap wrap = new Wrap();
            //wrap.Initialize();
            //Console.WriteLine("HMD is inizialized: " + wrap.Initialize());
            //Console.WriteLine("IPD is: " + rift.InterpupillaryDistance);
            //Console.WriteLine("H resulution is: " + rift.HResolution);
            //Console.WriteLine("V resolution is: " + rift.VResolution);wrap.Initialized;






            Console.ReadLine();
        }

    }
}

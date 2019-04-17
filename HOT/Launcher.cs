using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;


namespace Launcher
{
    public class CfgTools
    {

        public static void WriteCfg(List<Record> records, string cfg_file)
        {
            string space = " ";
            using (StreamWriter outputFile = new StreamWriter("HOT.cfg"))
            {
                foreach (Record record in records)
                {
                    //outputFile.WriteLine(record.name +space +record.exe +space +record.opt1 +space +record.opt2 +space +record.opt3);
                    outputFile.WriteLine("\n");
                }
                outputFile.Dispose();
            }

        }

        public static List<Record> ReadCfg(string cfg_file)
        {

                List<Record> records = new List<Record>();
                IEnumerable<string> lines = File.ReadLines(cfg_file);
                foreach (string line in lines)
                {
                    Record record = new Record();
                    record.exe = line.Substring(0, line.IndexOf(" "));
                    record.ss = Convert.ToDouble(line.Substring(line.IndexOf(" "), 5));
                    record.asw = Convert.ToInt16(line.Substring(line.IndexOf(" ") + 6, 1));
                    record.osd = Convert.ToInt16(line.Substring(line.IndexOf(" ") + 8, 1));
                    records.Add(record);
                }

                return records;
            
        }

        public static void AddRecordToCfg(Record record, string cfg_file)
        {
            List<Record> records = new List<Record>(ReadCfg(cfg_file));
            string space = " ";
            foreach (Record rc in records)
            {
                if (record.exe == rc.exe)
                {
                    //modify paramenter
                }
                else File.AppendAllText(cfg_file, record.exe + space + record.ss + space + record.asw + space + record.osd + "\n");
            }
            
            
        }
        
        public static void RunApp(string exe)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = Path.GetDirectoryName(exe);
            startInfo.FileName = exe;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        public static void CreateRegEntry(string exe)
        {
            //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\
            Microsoft.Win32.Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\" + Path.GetFileName(exe), "Debugger", "ciao");
        }

        public static void DeleteRegEntry(string exe)
        {
            //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\" + Path.GetFileName(exe),false);
        }


    }

    public class Record
    {
        public string name { get; set; }
        public string exe { get; set; }
        public double ss { get; set; }
        public int asw { get; set; }
        public int osd { get; set; }


    }

    //public class ExeCheck
    //{
    //    public void evento(object sender, EventArrivedEventArgs e)
    //    {
    //        //in this point the new events arrives
    //        //you can access to any property of the Win32_Process class
    //        //Console.WriteLine("TargetInstance.Handle :    " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Handle"]);
    //        Console.WriteLine("TargetInstance.Name :      " + ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"]);

    //    }

    //    public ExeCheck()
    //    {
    //        try
    //        {
    //            string ComputerName = "localhost";
    //            string WmiQuery = "Select * From __InstanceCreationEvent Within 1 " + "Where TargetInstance ISA 'Win32_Process' "; ;
    //            ManagementEventWatcher Watcher;
    //            ManagementScope Scope;

    //            Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
    //            Scope.Connect();

    //            Watcher = new ManagementEventWatcher(Scope, new EventQuery(WmiQuery));
    //            Watcher.EventArrived += new EventArrivedEventHandler(this.evento);
    //            Watcher.Start();
    //            //Console.Read();
    //            //Watcher.Stop();
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine("Exception {0} Trace {1}", e.Message, e.StackTrace);
    //        }

    //    }
    //}





}


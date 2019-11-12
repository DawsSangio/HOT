using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;


namespace Launcher
{
    public class CfgTools
    {
        // Structure of HOT.cfg entries
        //
        // [app.exe]_[ss]_[asw]_[osd]_[OC]
        // es: 
        // Oculus.exe = 1.00 0 0
        // aces.exe = 1.30 1 0

        /// <summary>
        /// Read HOT.cfg and and return a collentecion of the enrty
        /// </summary>
        public static ObservableCollection<Record> ReadCfg(string cfg_file)
        {

            ObservableCollection<Record> records = new ObservableCollection<Record>();
            IEnumerable<string> lines = File.ReadLines(cfg_file);
            foreach (string line in lines)
            {
                Record record = new Record();
                string[] values = line.Split(' ');
                int first_quote = line.IndexOf("\"");
                int sec_quote = line.IndexOf("\"", first_quote+1);
                record.exe = line.Substring(first_quote, sec_quote).Trim('"');
                record.ss = double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                record.asw = Convert.ToInt16(values[2]);
                record.osd = Convert.ToInt16(values[3]);
                record.oc = Convert.ToInt16(values[4]);
                records.Add(record);
            }

            return records;

        }

        public static void AddRecordToCfg(Record record, string cfg_file)
        {
            string space = " ";
            string quote = "\"";
            File.AppendAllText(cfg_file,quote + record.exe + quote + space + record.ss.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + space + record.asw + space + record.osd + space + record.oc + "\n");

        }

        public static void WriteCfg(ObservableCollection<Record> records, string cfg_file)
        {
            File.Delete(cfg_file);
            File.Create(cfg_file).Dispose();
            foreach (Record rec in records)
            {
                AddRecordToCfg(rec, cfg_file);
            }

        }


    }

    public class Record
    {
        public string exe { get; set; }
        public double ss { get; set; }
        public int asw { get; set; }
        public int osd { get; set; }
        public int oc { get; set; }

        public Record() { }

        public Record(string exe, double ss, int asw, int osd, int oc)
        {
            this.exe = exe;
            this.ss = ss;
            this.asw = asw;
            this.osd = osd;
            this.oc = oc;
        }



    }
    

}


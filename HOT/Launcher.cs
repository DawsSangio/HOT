using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Launcher
{
    public class CfgTools
    {
        // Structure of HOT.cfg entries
        //
        // [app.exe]_[ss]_[asw]_[osd]_[Bitrate]_[Hfov]_[Vfov]

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
                int first_quote = line.IndexOf("\"");
                int sec_quote = line.IndexOf("\"", first_quote+1);
                record.exe = line.Substring(first_quote, sec_quote).Trim('"');
                string[] values = line.Substring(sec_quote+1).Split(' ');
                record.ss = Convert.ToDouble(values[1]);
                record.asw = Convert.ToInt16(values[2]);
                record.osd = Convert.ToInt16(values[3]);
                record.bitrate = Convert.ToInt16(values[4]);
                record.hfov = Convert.ToDouble(values[5]);
                record.vfov = Convert.ToDouble(values[6]);
                records.Add(record);
            }

            return records;

        }

        private static void AddRecordToCfg(Record record, string cfg_file)
        {
            string space = " ";
            string quote = "\"";
            File.AppendAllText(cfg_file, quote + record.exe + quote
                                        + space + record.ss
                                        + space + record.asw
                                        + space + record.osd
                                        + space + record.bitrate
                                        + space + record.hfov
                                        + space + record.vfov + "\n");

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

    public class Record : INotifyPropertyChanged
    {
        private string _exe;
        public string exe
        {
            get { return this._exe; }
            set
            {
                if (value != this._exe)
                {
                    this._exe = value;
                    NotifyPropertyChanged();
                }
            }
        }


        private double _ss;
        public double ss
        {
            get { return this._ss; }
            set
            {
                if (value != this._ss)
                {
                    this._ss = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int _bitrate;
        public int bitrate
        {
            get { return this._bitrate; }
            set
            {
                if (value != this._bitrate)
                {
                    this._bitrate = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _hfov;
        public double hfov
        {
            get { return this._hfov; }
            set
            {
                if (value != this._hfov)
                {
                    this._hfov = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _vfov;
        public double vfov
        {
            get { return this._vfov; }
            set
            {
                if (value != this._vfov)
                {
                    this._vfov = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int _asw;
        public int asw
        {
            get { return this._asw; }
            set
            {
                if (value != this._asw)
                {
                    this._asw = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int osd { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Record() { }

        public Record(string exe, double ss, int asw, int osd, int bitrate, double hfov, double vfov)
        {
            this.exe = exe;
            this.ss = ss;
            this.asw = asw;
            this.osd = osd;
            this.bitrate = bitrate;
            this.hfov = hfov;
            this.vfov = vfov;

        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }


}


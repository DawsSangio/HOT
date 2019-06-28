using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json
{
    public class JsonRecord
    {
        public string record { get; set; }
        public List<string> values { get; set; }
    }

    public static class JsonReader
    {

        public static List<JsonRecord> ReadJsonFile(string file)
        {
            List<JsonRecord> jrecs = new List<JsonRecord>();
            IEnumerable<string> lines = File.ReadLines(file);
            String txt = File.ReadAllText(file);

            foreach (string line in lines)
            {
                //read the records
                if (line.Contains("[") )
                {
                    JsonRecord jsr = new JsonRecord();
                    jsr.record = line.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(":","").Replace("[","").Trim();
                    jrecs.Add(jsr);
                }
                else if (line.Contains("\":"))
                {
                    JsonRecord jsr = new JsonRecord();
                    string line_1 = line.Substring(0, line.IndexOf(":"));
                    jsr.record = line_1.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(":", "").Replace("[", "").Trim();
                    jrecs.Add(jsr);
                }

            }


            // Read the values
            // Crete an array of the records (splitter) to use for spliting
            string[] splitter = new string[jrecs.Count];
            for (int i = 0; i < jrecs.Count; i++)
            {
                splitter[i] = jrecs[i].record;
            }
            // Split the file using records(splitter)
            string[] splited = txt.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

                        
            foreach (JsonRecord jsr in jrecs)
            {
                List<string> values = new List<string>();

                string value_dot = splited[jrecs.IndexOf(jsr)+1];
                string value = value_dot.Substring(value_dot.IndexOf(":")+1);
                // read multiple values per record
                if (value.Contains("["))
                {
                    string[] sub_value = value.Split(',');//TODO remove empty
                    

                    for (int i = 0; i < sub_value.Length; i++)
                    {
                        values.Add(sub_value[i].Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("[", "").Replace("]","").Trim());
                    }

                   
                }
                else
                {
                    values.Add(value.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(":", "").Replace("[", "").Trim());
                }

                
                jsr.values = values;

            }

            return jrecs;
        }

       
    }
}

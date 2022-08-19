using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfTetris
{
    public class Configuration
    {
        public Configuration()
        {

        }
        public bool LoadConfig(ref List<Key> keys, ref double volume)
        {
            bool isnotclear = false;
            List<string> confstring = new List<string>();
            using (StreamReader reader = new StreamReader("config.cfg"))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    confstring.Add(line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 1));
                }
                for (int i = 1; i < confstring.Count; i++)
                {
                    if (confstring[i] != " ")
                    {
                        isnotclear = true;
                    }
                }
                if (isnotclear)
                {
                    for (int i = 1; i < confstring.Count; i++)
                    {
                        try
                        {
                            volume = Convert.ToDouble(confstring[0]);
                            KeyConverter conv = new KeyConverter();
                            keys.Add((Key)conv.ConvertFrom(confstring[i]));
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
        }

        public void SaveConfig(List<Key> keys, double volume)
        {
            using (StreamWriter writer = new StreamWriter("config.cfg", false))
            {
                writer.WriteLine("Volume=" + volume.ToString());
                writer.WriteLine("Left=" + keys[0].ToString());
                writer.WriteLine("Right=" + keys[1].ToString());
                writer.WriteLine("Down=" + keys[2].ToString());
                writer.WriteLine("Drop=" + keys[3].ToString());
                writer.WriteLine("CCW=" + keys[4].ToString());
                writer.WriteLine("Hold=" + keys[5].ToString());
                writer.WriteLine("CW=" + keys[6].ToString());
            }
        }
    }
}

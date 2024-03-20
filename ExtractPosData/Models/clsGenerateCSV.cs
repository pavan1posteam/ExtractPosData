using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CybertillPos.Model
{
    public class clsGenerateCSV
    {
        public static string GenerateCSVFile<T>(IList<T> list, string Name, int StoreId, string BaseUrl)
        {

            if (list == null || list.Count == 0) return "";
            if (!Directory.Exists(BaseUrl + "\\" + StoreId + "\\Upload\\"))
            {
                Directory.CreateDirectory(BaseUrl + "\\" + StoreId + "\\Upload\\");
            }
            string filename = Name + StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
            string fcname = BaseUrl + "\\" + StoreId + "\\Upload\\" + filename;
            // Console.WriteLine("Generating " + filename + " ........");
            //File.WriteAllText(BaseUrl + "\\" + StoreId + "\\Upload\\" + filename, csvData.ToString());
            // return filename;

            //get type from 0th member
            Type t = list[0].GetType();
            string newLine = Environment.NewLine;

            using (var sw = new StreamWriter(fcname))
            {
                //make a new instance of the class name we figured out to get its props
                object o = Activator.CreateInstance(t);
                //gets all properties
                PropertyInfo[] props = o.GetType().GetProperties();

                //foreach of the properties in class above, write out properties
                //this is the header row
                foreach (PropertyInfo pi in props)
                {
                    sw.Write(pi.Name + ",");
                }
                sw.Write(newLine);

                //this acts as datarow
                foreach (T item in list)
                {
                    //this acts as datacolumn
                    foreach (PropertyInfo pi in props)
                    {
                        //this is the row+col intersection (the value)
                        string whatToWrite =
                            Convert.ToString(item.GetType()
                                                 .GetProperty(pi.Name)
                                                 .GetValue(item, null))
                                .Replace(',', ' ') + ',';

                        sw.Write(whatToWrite);

                    }
                    sw.Write(newLine);
                }
                return filename;
            }
        }
    }
}

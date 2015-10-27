
using System.Collections.Generic;
using System.IO;

using System.Security.Cryptography.X509Certificates;
using System.Text;


namespace NIST.DMC
{
    public class Report
    {

        public static void GenerateTextReport(List<KeyValuePair<X509Certificate2, bool>> VerifiedCMS, string FilePath, char Delimiter)
        {
            
            using (StreamWriter sw = new StreamWriter(FilePath+".report.txt", true, Encoding.ASCII))
            {
                sw.WriteLine("Valid {0} Certificate", Delimiter);
                foreach(KeyValuePair<X509Certificate2,bool> k in VerifiedCMS)
                {
                    sw.Write(k.Value);
                    if (k.Value)
                        sw.Write("  {0} ", Delimiter);
                    else
                        sw.Write(" {0} ", Delimiter);
                    sw.WriteLine(k.Key.Subject);
                    
                }
            }
        }


    }
}

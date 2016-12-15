using System;
using NIST.DMC;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace NIST.DMCLI
{
    class Program
    {
        static void Main(string[] args)
        {

            //TODO: remove Console.read()
            X509Certificate2 x2 = null;
            FormatManager fm = null;
            
            string invokedVerb = null;
            object invokedVerbInstance = null;


            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options,
              (verb, subOptions) =>
              {

                  invokedVerb = verb;
                  invokedVerbInstance = subOptions;
              }))
            {

                WriteRed("Incorrect arguments.");
                Console.Read();
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            #region simple signature
            if (invokedVerb == "sign")
            {
                
                var signoptioninstance = (SignOption)invokedVerbInstance;

                if(!File.Exists(signoptioninstance.File))
                    Environment.Exit(1);

                if (signoptioninstance.Path != null && signoptioninstance.Password != null)
                    CertificatesManager.AddProtectedCertificate(signoptioninstance.Path, signoptioninstance.Password);


                x2 = CertificatesManager.FindByFriendlyName(signoptioninstance.Certificate);
                if (x2 != null)
                {
                    fm = FormatManagerFactory.GetInstance(signoptioninstance.File);
                    fm.EncodeAndSign(x2, signoptioninstance.File);
                    WriteGreen("Signed " + signoptioninstance.File);
                    Environment.Exit(0);
                }
                else
                {
                    WriteRed("Could not find certificate.");
                    Environment.Exit(1);
                }

                Console.Read();
            }
            #endregion

            #region simple signature verification
            if (invokedVerb == "verify")
            {
                var verifyoptioninstance = (VerifyOption)invokedVerbInstance;
                if (!FileExists(verifyoptioninstance.File))
                    Environment.Exit(1);

                fm = FormatManagerFactory.GetInstance(verifyoptioninstance.File);
                List<KeyValuePair<X509Certificate2, bool>> UsedCertificates = new List<KeyValuePair<X509Certificate2, bool>>();
                if(fm.VerifyFile(verifyoptioninstance.File, ref UsedCertificates))
                {
                    WriteGreen("All signatures in " + verifyoptioninstance.File + " were verified.");
                    Environment.Exit(0);
                }
                else
                {
                    WriteRed("Could not verify all signatures in " + verifyoptioninstance.File);
                    Environment.Exit(1);
                }


                Console.WriteLine("Verified {0}",verifyoptioninstance.File);
                Console.Read();
            }
            #endregion

            #region signature with metadata
            if(invokedVerb == "signm")
            {
                var signminstance = (SignWithMetadata)invokedVerbInstance;

                if (!FileExists(signminstance.File))
                    Environment.Exit(1);

                //make sure all the metadata fields are properly formatted
                signminstance.Metadata.ToList().ForEach(field =>
                {
                    if (!IsValidMetadata(field))
                    {
                        WriteRed("Invalid field " + field);
                        Environment.Exit(1);
                    }
                    else
                        WriteGreen("Valid field " + field);
                });

                STEPTRACE4PLOT fileManager = new STEPTRACE4PLOT(signminstance.File);
                STEPInterface.Elements elts = fileManager.GetBlocks();
                var b = elts.CreateBlock();
                signminstance.Metadata.ToList().ForEach(field =>
                {
                    string[] data = field.Split(':');
                    elts.AddMetadata(b, data[0], data[1]);
                });

                x2 = CertificatesManager.FindByFriendlyName(signminstance.Certificate);
                if (x2 != null)
                {
                    fileManager.EncodeAndSign(x2, signminstance.File,b);
                    WriteGreen("Signed " + signminstance.File);
                    Environment.Exit(0);
                }
                else
                {
                    WriteRed("Could not find certificate.");
                    Environment.Exit(1);
                }
                Console.WriteLine(elts.FullText());
                Console.Read();
            }
            #endregion

            #region  signature with metadata verification
            if (invokedVerb == "verifym")
            {
                var verifym = (VerifyWithMetadata)invokedVerbInstance;
                if (!FileExists(verifym.File))
                    Environment.Exit(1);

                fm = new STEPTRACE4PLOT(verifym.File);
                List<KeyValuePair<X509Certificate2, bool>> UsedCertificates = new List<KeyValuePair<X509Certificate2, bool>>();
                if (fm.VerifyFile(verifym.File, ref UsedCertificates))
                {
                    WriteGreen("All signatures in " + verifym.File + " were verified.");
                    //Environment.Exit(0);
                }
                else
                {
                    WriteRed("Could not verify all signatures in " + verifym.File);
                    //Environment.Exit(1);
                }


                Console.WriteLine("Verified {0}", verifym.File);
                Console.Read();
            }
            #endregion
        }

        private static void SwitchRed()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
        }

        private static void SwitchGreen()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }

        private static void WriteGreen(string text)
        {
            SwitchGreen();
            Console.WriteLine(text);
        }

        private static void WriteRed(string text)
        {
            SwitchRed();
            Console.WriteLine(text);
        }

        private static bool FileExists(string FilePath)
        {
            bool exists = true;
            if (!File.Exists(FilePath))
            {
                WriteRed("Could not find " + FilePath);
                exists = false;
            }
            return exists;
        }

        private static bool IsValidMetadata(string field)
        {

            bool valid = false;
            if (field.IndexOf(':') > 1 && field.IndexOf(':') < field.Length - 1)
                valid = true;
            return valid;
        }
    }
}

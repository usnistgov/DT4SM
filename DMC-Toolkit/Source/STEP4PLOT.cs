using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Security.Cryptography.Pkcs;
using System.IO.Pipes;
using System.Collections.Generic;

namespace NIST.DMC
{
    public class STEP4PLOT: Text4PLOT
    {

        public override void SignFile(string filePath, object signature)
        {
            using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.ASCII))
            {
                sw.WriteLine("\n");
                sw.WriteLine("SIGNATURE;");
                sw.WriteLine("-----BEGIN PKCS7-----");
                sw.WriteLine(signature.ToString());
                sw.WriteLine("-----END PKCS7-----");
                sw.WriteLine("ENDSEC;");
            }
        }

        private static String RetrieveContent(StreamReader fileStream)
        {
            string Line;
            string Content = "";
            bool Save = false;
            while ((Line = fileStream.ReadLine()) != null)
            {
                //ISO-10303-21;
                if (Save && !Line.Contains("END-ISO-10303-21;"))
                    Content = Content + Line;
                else
                {
                    if (Line.Contains("ISO-10303-21;"))
                    {
                        Save = true;
                        Content = Content + Line;
                    }
                    else if (Line.Contains("END-ISO-10303-21;"))
                    {
                        Content = Content + Line;
                        Save = false;
                        //sr.Close();
                        return Content;
                    }
                }
            }

            return Content;

        }


        protected override byte[] Hash(string filePath)
        {
            //Method is overriden because we only need to compute the hash of the content BEFORE the signature
            //We do not want to cross-sign hence why we need to extract the "real" data from the file
            Thread ThreadStream;
            Thread ThreadRead;
            byte[] DataHash = new byte[0];

            StreamReader FileStream = new StreamReader(filePath);
            #region thread

            ThreadRead = new Thread(reader =>
            {
                //Create a named pipe using the current process ID of this application
                using (NamedPipeServerStream PipeStream = new NamedPipeServerStream("StepSign"))
                {
                    //wait for the streamer to send the data
                    PipeStream.WaitForConnection();
                    using (StreamReader sr = new StreamReader(PipeStream))
                    {
                        DataHash = HashFunction.ComputeHash(sr.BaseStream);
                    }
                }
            });

            ThreadStream = new Thread(streamer =>
            {
                using (NamedPipeClientStream PipeStream = new NamedPipeClientStream("StepSign"))
                {
                    //flag to stop reading
                    string EOD = "END-ISO-10303-21;";
                    byte[] bEOD = System.Text.Encoding.ASCII.GetBytes(EOD);
                    int SearchIndex = 0;

                    //we connect to the StepSign pipe as a client to send data  
                    PipeStream.Connect();

                    //use a binarywriter on the pipe to stream data
                    using (BinaryWriter sw = new BinaryWriter(PipeStream))
                    {
                        int read = 0;
                        while ((read = FileStream.BaseStream.ReadByte()) != -1)
                        {
                            sw.Write((byte)read);
                            //did we stream all the data yet?
                            if ((byte)read == bEOD[SearchIndex])
                            {
                                SearchIndex += 1;
                                //if we just finished reading the flag, then we just read all the data we need to hash
                                if (SearchIndex >= bEOD.Length)
                                    break;
                            }
                            else
                                SearchIndex = 0;

                        }
                    }

                }
            });

            ThreadRead.Start();
            ThreadStream.Start();
            ThreadStream.Join();
            #endregion
            FileStream.Close();
            return DataHash;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;


namespace NIST.DMC
{
    public class GCODE4PLOT: Text4PLOT
    {

        
        public override void SignFile(string filePath, object digitalSignature)
        {
            using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.ASCII))
            {
                sw.WriteLine("");
                sw.WriteLine("(-----BEGIN PKCS7-----)");
                sw.WriteLine("("+digitalSignature.ToString()+")");
                sw.WriteLine("(-----END PKCS7-----)");

            }
        }

        private  String ExtractFirstSignature(string filePath)
        {
            StreamReader sr = new StreamReader(filePath);

            string Line;
            string CMS = "";
            bool Save = false;
            while ((Line = sr.ReadLine()) != null)
            {
                if (Save && !Line.Contains("-----END PKCS7-----"))
                    CMS = CMS + Line;
                else
                {
                    if (Line.Contains("-----BEGIN PKCS7-----"))
                    {
                        Save = true;
                    }
                    else if (Line.Contains("-----END PKCS7-----"))
                    {
                        Save = false;
                        return CMS.Trim(')','(');
                    }
                }

            }
            return CMS.Trim(')', '(');
        }

        public  override List<String> ExtractAllSignatures(string filePath)
        {
            List<String> Signatures = new List<string>();
            string Line = "";
            string Signature = "";
            bool Save = false;
            using (StreamReader stream = new StreamReader(filePath))
            {
                while ((Line = stream.ReadLine()) != null)
                {
                    if (Save && !Line.Contains("-----END PKCS7-----"))
                        Signature = Signature + Line;
                    else
                    {
                        if (Line.Contains("-----BEGIN PKCS7-----"))
                        {
                            Save = true;
                        }
                        else if (Line.Contains("-----END PKCS7-----"))
                        {
                            Save = false;
                            Signatures.Add(Signature.Trim(')', '('));
                            Signature = "";
                        }
                    }
                }
            }
            return Signatures;
        }

        private  String RetrieveContent(StreamReader fileStream)
        {
            string Line;
            string Content = "";

            while ((Line = fileStream.ReadLine()) != null)
            {

                Content = Content + Line;
                if (Line.Contains("M30"))
                    break;
            }

            return Content;

        }

        //TODO: Refactor
        protected override byte[] Hash(string filePath)
        {
            Thread tStreamer;
            Thread tDataReader;
            byte[] dataDigest = new byte[0];

            StreamReader sno = new StreamReader(filePath);
            #region thread

            tDataReader = new Thread(reader =>
            {
                //Create a named pipe using the current process ID of this application
                using (NamedPipeServerStream pipeStream = new NamedPipeServerStream("GSign"))
                {
                    //wait for the streamer to send the data
                    pipeStream.WaitForConnection();
                    using (StreamReader sr = new StreamReader(pipeStream))
                    {
                        dataDigest = HashFunction.ComputeHash(sr.BaseStream);
                    }
                }
            });

            tStreamer = new Thread(streamer =>
            {
                using (NamedPipeClientStream pipeStream = new NamedPipeClientStream("GSign"))
                {

                    string EOD = "M30";
                    byte[] bEOD = System.Text.Encoding.ASCII.GetBytes(EOD);
                    int searchIdx = 0;

                    //we connect to the StepSign pipe as a client to send data  
                    pipeStream.Connect();

                    //use a binarywriter on the pipe to stream data
                    using (BinaryWriter sw = new BinaryWriter(pipeStream))
                    {
                        int read = 0;
                        while ((read = sno.BaseStream.ReadByte()) != -1)
                        {
                            sw.Write((byte)read);

                            if ((byte)read == bEOD[searchIdx])
                            {
                                searchIdx += 1;
                                if (searchIdx >= bEOD.Length)
                                    break;
                            }
                            else
                                searchIdx = 0;

                        }
                    }

                }
            });

            tDataReader.Start();
            tStreamer.Start();
            tStreamer.Join();
            #endregion
            sno.Close();
            return dataDigest;
        }
    }
}

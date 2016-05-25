using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;


namespace NIST.DMC
{
    /// <summary>
    /// FormatManager for non-XML file
    /// </summary>
    public  class Text4PLOT: FormatManager
    {
        #region Text4PLOT properties and methods
        protected static SHA256Managed HashFunction = new SHA256Managed();

        protected virtual byte[] Hash(String filePath)
        {
            Stream s = File.OpenRead(filePath);
            byte[] digest = Hash(s);
            s.Close();
            return digest;
        }

        private byte[] Hash(Stream dataStream)
        {

            return HashFunction.ComputeHash(dataStream);
        }


        private static byte[] Hash(byte[] data)
        {

            return HashFunction.ComputeHash(data);
        }

        /// <summary>
        /// Signs a data hash and returns the signature
        /// </summary>
        /// <param name="x">Certificate used to sign the data</param>
        /// <param name="hashedData">Data digest to be signed</param>
        /// <returns>Returns a string containing a PKCS#7 signature</returns>
        protected  String EncodeCMS(X509Certificate2 x, byte[] hashedData)
        {
            //we are creating a CMS/PKCS#7 message
            Oid digestOid = new Oid("1.2.840.113549.1.7.2");
            ContentInfo contentInfo = new ContentInfo(digestOid, hashedData);

            //true: signature is detached and will be added to the file
            SignedCms signedCms = new SignedCms(contentInfo, true);

            CmsSigner cmsSigner = new CmsSigner(x);
            // false will prompt the user to enter the pin if a PIV is used
            signedCms.ComputeSignature(cmsSigner, false);
            byte[] encode = signedCms.Encode();
            return Convert.ToBase64String(encode);

        }

        #endregion

        #region Methods from FormatManager

        public override object EncodeCMS(X509Certificate2 certificate, String filePath)
        {
            //TODO: Test the hash first
            return EncodeCMS(certificate, Hash(filePath));
        }

        public override void EncodeAndSign(X509Certificate2 certificate, string filePath)
        {
            string DigitalSignature = EncodeCMS(certificate, Hash(filePath));
            SignFile(filePath, DigitalSignature);
        }

        public  override void SignFile(String filePath, object digitalSignature)
        {
            //TODO: Add test to DigitalSignature type

            using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.ASCII))
            {

                sw.WriteLine("-----BEGIN PKCS7-----");
                sw.WriteLine(digitalSignature.ToString());
                sw.WriteLine("-----END PKCS7-----");

            }
        }

        public override bool VerifyFile(string filePath, ref List< KeyValuePair<X509Certificate2, bool>> verifiedCMS)
        {
            
            byte[] DataDigest = new byte[0];
            byte[] EncodedCMS = new byte[0];

            //digest of the data without the signature(s)
            DataDigest = Hash(filePath);
            //signatures found in the file
            List<String> Signatures = ExtractAllSignatures(filePath);
            if (Signatures.Count < 1) throw new NoSignatureFoundException(filePath);
            //Content information created from the data digest
            ContentInfo StepContent = new ContentInfo(DataDigest);

            SignedCms SignedCMS = new SignedCms(StepContent, true);
            List<KeyValuePair<X509Certificate2, bool>> UsedCertificates = new List<KeyValuePair<X509Certificate2, bool>>();

            bool Validation = true;

            foreach (String Signature in Signatures)
            {
                SignedCMS.Decode(Convert.FromBase64String(Signature));
                SignerInfoEnumerator Enumerator = SignedCMS.SignerInfos.GetEnumerator();
                if (!Enumerator.MoveNext()) throw new InvalidSignerInformationException(Signature);

                try
                {
                    //after decoding the signed cms, we check the signature
                    SignedCMS.CheckSignature(true);
                    UsedCertificates.Add(new KeyValuePair<X509Certificate2, bool>(Enumerator.Current.Certificate, true));

                }
                catch (System.Security.Cryptography.CryptographicException e)
                {
                    //signature can't be verified
                    UsedCertificates.Add(new KeyValuePair<X509Certificate2, bool>(Enumerator.Current.Certificate, false));
                    Validation = false;
                }

            }

            verifiedCMS = UsedCertificates;
            return Validation;
        }


        private String ExtractFirstSignature(string filePath)
        {

            string Line;
            string CMS = "";
            bool Save = false;
            using (StreamReader sr = new StreamReader(filePath))
            {
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
                            return CMS;
                        }
                    }
                }
            }
            return CMS;
        }

        public virtual List<String> ExtractAllSignatures(string filePath)
        {
            List<String> Signatures = new List<string>();
            string Line = "";
            string Signature = "";
            bool Save = false;
            using (StreamReader sr = new StreamReader(filePath))
            {
                while ((Line = sr.ReadLine()) != null)
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
                            Signatures.Add(Signature);
                            Signature = "";
                        }
                    }
                }
            }
            return Signatures;
        }

        #endregion

    }
}

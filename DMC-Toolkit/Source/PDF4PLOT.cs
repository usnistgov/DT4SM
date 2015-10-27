using System.Security.Cryptography.X509Certificates;
using iTextSharp.text.pdf;
using BcX509 = Org.BouncyCastle.X509;
using DotNetUtils = Org.BouncyCastle.Security.DotNetUtilities;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.pdf.security;
using System.Security.Cryptography.Pkcs;
using System;

namespace NIST.DMC
{
    public class PDF4PLOT: Binary4PLOT
    {
        public override object EncodeCMS(X509Certificate2 certificate, string filePath)
        {
            return base.EncodeCMS(certificate, filePath);
        }

        public override void EncodeAndSign(X509Certificate2 certificate, string filePath)
        {
            PdfReader Reader = new PdfReader(filePath);
            PdfStamper Stamper = PdfStamper.CreateSignature(Reader, new FileStream(filePath + ".signed", FileMode.Create),'0');
            PdfSignatureAppearance SAP = Stamper.SignatureAppearance;
            BcX509.X509Certificate BouncyCertificate = DotNetUtils.FromX509Certificate(certificate);
            var chain = new List<BcX509.X509Certificate> { BouncyCertificate };

            IExternalSignature ES = new X509Certificate2Signature(certificate, DigestAlgorithms.SHA256);
            MakeSignature.SignDetached(SAP, ES, chain, null, null, null, 0, CryptoStandard.CMS);
            Stamper.Close();
            Reader.Close();
            File.Delete(filePath);
            File.Move(filePath + ".signed", filePath);

        }

        public override bool VerifyFile(string filePath, ref List<KeyValuePair<X509Certificate2, bool>> verifiedCMS)
        {
            PdfReader Reader = new PdfReader(filePath);
            AcroFields Fields = Reader.AcroFields;
            List<String> Names = Fields.GetSignatureNames();
            List<KeyValuePair<X509Certificate2, bool>> UsedCertificates = new List<KeyValuePair<X509Certificate2, bool>>();
            bool Validation = false;
            foreach(String Signature in Names)
            {
                PdfPKCS7 CMS = Fields.VerifySignature(Signature);
                bool currentValidation = CMS.Verify();
                UsedCertificates.Add(new KeyValuePair<X509Certificate2, bool>(new X509Certificate2(DotNetUtils.ToX509Certificate(CMS.SigningCertificate)), currentValidation));
                //If one signature fails, so does the global validation of the file   
                if(!currentValidation)
                    Validation = false;

            }
            verifiedCMS = UsedCertificates;
            return Validation;
        }

        public void AddSignature(X509Certificate2 certificate, string filePath)
        {
            EncodeAndSign(certificate, filePath);
        }

        public int SignaturesCount(string filePath)
        {
            PdfReader Reader = new PdfReader(filePath);
            AcroFields Fields = Reader.AcroFields;
            return Fields.GetSignatureNames().ToArray().Length;
        }
    }
}

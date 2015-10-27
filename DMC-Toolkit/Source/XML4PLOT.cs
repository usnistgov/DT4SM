using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;

using System.Xml;

namespace NIST.DMC
{
    /// <summary>
    /// FormatManager for XML files
    /// </summary>
    public class XML4PLOT: FormatManager
    {

        public override object EncodeCMS(X509Certificate2 certificate, string xmlFilePath)
        {
            XmlDocument Document = new XmlDocument();
            Document.PreserveWhitespace = true;
            XmlTextReader XmlFile = new XmlTextReader(xmlFilePath);
            Document.Load(XmlFile);
            XmlFile.Close();
            XmlNodeList SignaturesList = Document.GetElementsByTagName("Signature");
            // Remove existing signatures, this is not a countersigning. 
            for (int i = 0; i < SignaturesList.Count; i++)
            {
                SignaturesList[i].ParentNode.RemoveChild(SignaturesList[i]);
                i--;
            }

            SignedXml SignedXml = new SignedXml(Document);
            SignedXml.SigningKey = certificate.PrivateKey;
            Reference Reference = new Reference();
            Reference.Uri = "";
            XmlDsigEnvelopedSignatureTransform EnvelopedSignatureTransform = new XmlDsigEnvelopedSignatureTransform();
            Reference.AddTransform(EnvelopedSignatureTransform);
            SignedXml.AddReference(Reference);
            KeyInfo Key = new KeyInfo();
            Key.AddClause(new KeyInfoX509Data(certificate));
            SignedXml.KeyInfo = Key;
            SignedXml.ComputeSignature();
            // Get the XML representation of the signature and save 
            // it to an XmlElement object.
            XmlElement XmlDigitalSignature = SignedXml.GetXml();
            
            return XmlDigitalSignature;
        }

        public  override void SignFile(String xmlFilePath, object xmlDigitalSignature)
        {
            XmlElement XmlDigitalSignature = (XmlElement)xmlDigitalSignature;
            XmlDocument Document = new XmlDocument();
            Document.PreserveWhitespace = true;
            XmlTextReader XmlFile = new XmlTextReader(xmlFilePath);
            Document.Load(XmlFile);
            XmlFile.Close();
            // Append the element to the XML document.
            Document.DocumentElement.AppendChild(Document.ImportNode(XmlDigitalSignature, true));

            if (Document.FirstChild is XmlDeclaration)
            {
                Document.RemoveChild(Document.FirstChild);
            }

            // Save the signed XML document to a file specified 
            // using the passed string. 
            using (XmlTextWriter textwriter = new XmlTextWriter(xmlFilePath, new UTF8Encoding(false)))
            {
                textwriter.WriteStartDocument();
                Document.WriteTo(textwriter);
                textwriter.Close();
            }
        }

        public  override void EncodeAndSign(X509Certificate2 certificate, string xmlFilePath)
        {
            SignFile(xmlFilePath, EncodeCMS(certificate, xmlFilePath));
        }

        public  override Boolean VerifyFile(String xmlFilePath, ref List<KeyValuePair<X509Certificate2, bool>> verifiedCMS)
        {

            XmlDocument Document = new XmlDocument();
            Document.PreserveWhitespace = true;
            Document.Load(xmlFilePath);

            // Each and every signature is based on the unsigned content of the file, 
            // we need to remove the signature(s) from the file before processing.
            
            XmlNodeList TemporarySignatureList = Document.GetElementsByTagName("Signature");
            if (TemporarySignatureList.Count < 1) throw new NoSignatureFoundException(xmlFilePath);
            List<XmlNode> SignaturesList = new List<XmlNode>();

            for (int i = 0; i < TemporarySignatureList.Count; i++)
            {
                SignaturesList.Add(TemporarySignatureList[i].CloneNode(true));
                TemporarySignatureList[i].ParentNode.RemoveChild(TemporarySignatureList[i]);
                i--;
            }

            SignedXml SignedXML = new SignedXml(Document);

            bool Validation = true;

            // Time to check each signature.

            foreach (XmlElement Signature in SignaturesList)
            {
                SignedXML.LoadXml(Signature);
                if (!SignedXML.CheckSignature())
                    Validation = false;
                // Get the certificate from the signature key information
                var certificate = SignedXML.KeyInfo.OfType<KeyInfoX509Data>().First();
                X509Certificate2 SignatureCertificate = null;
                if (certificate != null)
                    SignatureCertificate = certificate.Certificates[0] as X509Certificate2;
                
                verifiedCMS.Add(new KeyValuePair<X509Certificate2, bool>(SignatureCertificate, Validation));
            }

            return Validation;
        }

    }
}

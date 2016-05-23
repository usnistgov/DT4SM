using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.IO;
using STEPInterface;
using System.Security.Cryptography.Pkcs;

namespace NIST.DMC
{
    public class STEPTRACE4PLOT : STEP4PLOT
    {
        private STEPInterface.Elements signatureBlock;

        public STEPTRACE4PLOT(string block)
        {
            signatureBlock = new Elements(ExtractBlocks(block), false);
            
        }

        public override void EncodeAndSign(X509Certificate2 certificate, string filePath)
        {
            throw new NotImplementedException();
        }

        public void EncodeAndSign(X509Certificate2 certificate, string filePath, int block)
        {
            SignFile(filePath,  EncodeCMS(certificate, filePath, block), block);

        }

        public override object EncodeCMS(X509Certificate2 certificate, string filePath)
        {
            //TO DO: need double hash. 1 level merkle tree
            throw new NotImplementedException();
        }

        public object EncodeCMS(X509Certificate2 certificate, string filePath, int block)
        {
            
            byte[] hashContent = this.Hash(filePath);
            string traceBlock = signatureBlock.GetCompleteTrace(block);
            if (traceBlock != null)
            {
                byte[] hashTrace = HashFunction.ComputeHash(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(traceBlock)));
                byte[] merkleHash = new byte[hashContent.Length + hashTrace.Length];
                Array.Copy(hashContent, merkleHash, hashContent.Length);
                Array.Copy(hashTrace, 0, merkleHash, hashContent.Length, hashTrace.Length);
                return EncodeCMS(certificate, merkleHash);
            }
            else
                return null;


        }

        public  void SignFile(string filePath,  object digitalSignature, int block)
        {
            
            var x = digitalSignature.ToString();
            signatureBlock.SetPKCSSignatureByBlockId(block, digitalSignature.ToString());
            //need to write signature inside file
            using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.ASCII))
                sw.WriteLine(signatureBlock.GetSignatureTextByBlockId(block));
        }

        public override void SignFile(string filePath, object digitalSignature)
        {
            //TO DO: get the CMS and call Element.Sign()
            throw new NotImplementedException();
        }

        public override bool VerifyFile(string filePath, ref List<KeyValuePair<X509Certificate2, bool>> verifiedCMS)
        {
            byte[] DataDigest = new byte[0];
            byte[] BlockDigest = new byte[0];
            signatureBlock = new Elements(ExtractBlocks(filePath), false);

            //digest of the data without the signature(s)
            DataDigest = Hash(filePath);
            //signatures found in the file
            Dictionary<string, string> Signatures = ExtractAllSignatures(filePath);
            if (Signatures.Count < 1) throw new NoSignatureFoundException(filePath);

            List<KeyValuePair<X509Certificate2, bool>> UsedCertificates = new List<KeyValuePair<X509Certificate2, bool>>();
           
            bool Validation = true;

            foreach (String Signature in Signatures.Keys)
            {
                BlockDigest = HashFunction.ComputeHash(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Signatures[Signature])));
                byte[] merkleHash = new byte[DataDigest.Length + BlockDigest.Length];
                Array.Copy(DataDigest, merkleHash, DataDigest.Length);
                Array.Copy(BlockDigest, 0, merkleHash, DataDigest.Length, BlockDigest.Length);

                //Content information created from the data digest
                ContentInfo StepContent = new ContentInfo(merkleHash);

                SignedCms SignedCMS = new SignedCms(StepContent, true);
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

        public STEPInterface.Elements GetBlocks()
        {
            return this.signatureBlock;
        }

        private string ExtractBlocks(string filePath)
        {
            string blocks = String.Empty;
            bool save = false;
            string line;
            using (StreamReader sw = new StreamReader(filePath))
            {
                while ((line = sw.ReadLine()) != null)
                {
                    if (save)
                        blocks = blocks + (line + "\n");
                    else
                        if (line.Contains("END-ISO-10303-21;"))
                        save = true;
                }
            }

            return blocks;
        }

        public virtual Dictionary<string, string> ExtractAllSignatures(string filePath)
        {
            Dictionary<string, string> Signatures = new Dictionary<string, string>();
            foreach(string id in signatureBlock.GetSignatureBlocksId())
            {
                string key = signatureBlock.GetPKCSSignatureByBlockId(Int32.Parse(id));
                string value = signatureBlock.GetCompleteTrace(Int32.Parse(id));
                Signatures.Add(key, value);
            }
            return Signatures;
        }
    }
}

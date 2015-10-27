# Digital Manufacturing Certificate (DMC) Toolkit
The information below provides a basic level of documentation for the DMC Toolkit. For support or more information, please contact Thomas Hedberg (thomas.hedberg@nist.gov).

## What is the NIST DMC Toolkit
The NIST Digital Manufacturing Certificate (DMC) Toolkit is a toolkit designed specifically to provide certification – digital signature using software and hardware certificates, and verification – of manufacturing related data. In its first release, the DMC toolkit supports signature and validation of data in the following formats: ISO 10303-21 (STEP) [1], ISO 6983 (G-code) [2], ISO 32000 (PDF) [3] and 14739 (PRC, aka 3D PDF) [4], and Quality Information Framework (QIF) [5]. The project offers both the Toolkit as a C# API and a fully-functional demonstration application.

## Introduction to digital signature
Switching from drawings to models has been proven to bring multiple benefits to actors involved in the product lifecycle. One major drawback we have identified is the lack of data certification, traditionally done by apposing a handwritten signature on a document. In a model-based environment the equivalent of a handwritten signature is called a digital signature and share its same attributes – it is unique, unforgeable, and cannot be repudiated. 

A digital signature on a document assures that the issuer of the signature has approved the content of the document. A digital signature is the result of the encryption of the document’s fingerprint with the signer’s private key. A signer is a physical person or trusted cyber system that has been issued a digital certificate, by a certification authority, which binds the person’s identity to a private key and its public key. To verify the authenticity of the digital signature, one must decrypt the digital signature using the signer’s public key and compare that to the document’s fingerprint. When both fingerprints are identical, the digital signature is authenticated and the signer cannot repudiate it. 

The Internet Engineering Task Force (IETF) has issued several standards describing how to generate and represent interoperable signatures. Those include the widely used Public Key Cryptography Standard (PKCS) #7 – RFC 2315 [6] - and its superset the Cryptographic Message Standard (CMS) – RFC 3369 [7]. The NIST DMC Toolkit follows these recommendations.

## Formats supported
This section describes how a signature is encoded into a file for each of the different formats supported by the NIST DMC toolkit.

ISO 10303 defines different serialization mechanisms to encode product data. ISO 10303-21 describes its most popular serialization and produces ASCII files. The DMC Toolkit follows the work from the ISO Technical Committee 184 / Subcommittee 4 on the Draft International Standard (DIS) 10303-21 3rd edition. This draft recommends to embed the signature, following the PKCS#7 format, at the end the data file, in a signature data block. A signature data block opens with a `SIGNATURE;` tag and closes with an `ENDSEC;` tag. A valid signature block would look like the following:

```
SIGNATURE;
-----BEGIN PKCS7-----
signature in pkcs7 format
-----END PKCS7-----
ENDSEC;
```

As of October 2015 ISO 6983 does not officially provides guidance on how to embed signatures in its data file. The DMC Toolkit implementation follows similar guidelines to ISO/DIS 10303-21 3rd edition. In the current version, digital signatures can be found at the end of the file as comments between the `(-----BEGIN PKCS7-----)` and `(-----END PKCS7-----)` tags. A valid signature block would look like the following:

```
(-----BEGIN PKCS7-----)
signature in pkcs7 format
(-----END PKCS7-----)
```

The DMC Toolkit signs PDF files using the iText PDF library (http://sourceforge.net/projects/itext/). This library follows the official ISO standard, providing interoperable signatures that can be read by any software component compatible with ISO 32000.

The Quality Information Framework uses XML to represent its data. The W3C owns a standard that describes digital-signature representation for XML data. The W3C XMLDsig specification can be found at http://www.w3.org/TR/xmldsig-core/. The DMC Toolkit follows the XMLDsig specification to encode QIF signatures. QIF signatures are the last XML nodes of the root node of the QIF document. A valid signature would like the following:

```XML
<?xml version="1.0" encoding="utf-8"?>
<QIFDocument ...>
...
<Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
	<SignedInfo>
	<CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315" />
	<SignatureMethod Algorithm="http://www.w3.org/2000/09/xmldsig#rsa-sha1" />
	<Reference URI="">
		<Transforms>
		<Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature" />
		</Transforms><DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1" />
		<DigestValue>...</DigestValue>
	</Reference>
	</SignedInfo>
		<SignatureValue>
		...
		</SignatureValue>
		<KeyInfo>
		<X509Data>
			<X509Certificate>
			...
			</X509Certificate>
		</X509Data>
		</KeyInfo>
	</Signature>
</QIFDocument>
```

## Developer’s information

The DMC Toolkit is a C# API designed to support digital signature and verification of ASCII, XML or binary data. The API was developed with extensibility in mind and can easily be extended to enable new support. 

### How to sign a file
Before signing a file you need to select a certificate containing the signer’s information. The API provides a `CertificatesManager` that manipulates the certificate local store. You can either access the full list of certificates (`GetCertificates()`) or retrieve a particular one (`FindByFriendlyName()`). The next step consists of using the right instance of a `FormatManager` for the type of file you want to sign. If you are not sure what `FormatManager` needs to be used, you can use the `FormatManagerFactory` to get the right instance (`GetInstance()`) for a file. Once you have the `FormatManager` instance, the signer’s certificate, and the file path, you can either generate a CMS (`EncodeCMS()`), sign the file with a CMS of your choice (`SignFile()`) or generate the CMS and sign the file with it (`EncodeAndSign()`).

```C#
String FilePath = @“C:\Users\assemblyX12-XY.stp”;
FormatManager f = FormatManagerFactory.GetInstance(FilePath);
X509Certificate2 x2 = CertificatesManager.FindByFriendlyName("Tom");
f.EncodeAndSign(x2, FilePath);
Console.ReadLine();
```

### How to validate a file
Before validating a file you need to get the right instance of a `FormatManager`. Similar to the signing operation, a `FormatManager` instance can be created either through the `FormatManagerFactory` or by instantiating a concrete subclass of `FormatManager`. Once you have a `FormatManager` instance and the path of the file containing the signature(s) to validate, you need to call the `VerifyFile()` method. This method returns a boolean indicating whether all signatures are valid or not. It also takes a reference parameter that is populated with the status (valid/invalid) of all signatures in the file.

```C#
String FilePath = @“C:\Users\assemblyX12-XY.stp”;
FormatManager f = FormatManagerFactory.GetInstance(filePath);
List<KeyValuePair<X509Certificate2, bool>> UsedCertificates = new List<KeyValuePair<X509Certificate2, bool>>();
Bool Validation = f.VerifyFile(FilePath, ref UsedCertificates);
Console.WriteLine(“Validation is {0}”, Validation);
foreach(KeyValuePair<X509Certificate2, bool> k in UsedCertificates)
{
	Console.WriteLine(k.Key.Subject);
	Console.Write(k.Value);
}
Console.ReadLine();

```
### How to support new data format
To extend the API and support new formats you need to write a new type of `FormatManager` and implement its four (4) main methods. We strongly recommend that you do not modify the existing `FormatManager` concrete classes. Once you write a new `FormatManager` you need to re-configure the `FormatManagerFactory` to associate your new class to a file extension. To do so, you will edit the `GetInstance()` method to support the new file extension.

Before extending the API we recommend anyone to carefully read the following 2 articles on the MSDN: Using System.Security.Cryptography.Pkcs [8] and About System.Security.Cryptography.Pkcs [9].

### Requirements and Mono compatibility 
The Mono project is an open source and cross-platform .NET framework that allows you to run your code of different platforms. At this time the Mono support of the System.Security namespace is incomplete, thus the DMC Toolkit can only be used in a Windows environment. To know more about the Mono implementation status you can refer to the Mono Class Status Pages [10].

## References
[1] ISO 10303-21:2002 Industrial automation systems and integration -- Product data representation and exchange -- Part 21: Implementation methods: Clear text encoding of the exchange structure

[2] ISO 6983-1:2009 Automation systems and integration -- Numerical control of machines -- Program format and definitions of address words -- Part 1: Data format for positioning, line motion and contouring control systems

[3] ISO 32000-1:2008 Document management -- Portable document format -- Part 1: PDF 1.7

[4] ISO 14739-1:2014 Document management -- 3D use of Product Representation Compact (PRC) format -- Part 1: PRC 10001

[5] Quality Insurance Framework (QIF) http://qifstandards.org/

[6] Request for comments 2315, PKCS #7: Cryptographic Message Syntax Version 1.5, http://www.ietf.org/rfc/rfc2315.txt

[7] Request for comments 3369, Cryptographic Message Syntax (CMS), https://www.ietf.org/rfc/rfc3369.txt 

[8] Microsoft Developer Network, Using System.Security.Cryptography.Pkcs https://msdn.microsoft.com/en-us/library/ms180955(v=vs.85).aspx

[9] Microsoft Developer Network, About System.Security.Cryptography.Pkcs https://msdn.microsoft.com/en-us/library/ms180945(v=vs.85).aspx

[10] Mono Class Status Pages http://go-mono.com/status/

#region

using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using NFeEletronica.NotaFiscal;

#endregion

namespace NFeEletronica.Assinatura
{
    public class AssinaturaDeXml
    {
        /// <summary>
        ///     Assina um arquivo Xml
        /// </summary>
        /// <param name="nota"></param>
        /// <param name="x509Cert"></param>
        /// <param name="tagAssinatura"></param>
        /// <param name="uri"></param>
        public string AssinarNota(Nota nota, X509Certificate2 x509Cert, string tagAssinatura, string uri = "")
        {
            //Abrir o arquivo XML a ser assinado e ler o seu conteúdo
            return Assinar(nota, x509Cert, tagAssinatura, uri);
        }

        private string Assinar(Nota nota, X509Certificate2 x509Cert, string tagAssinatura, string uri = "")
        {
            string xmlString;
            using (var srReader = File.OpenText(nota.CaminhoFisico))
            {
                xmlString = srReader.ReadToEnd();
            }

            // Create a new XML document.
            var doc = new XmlDocument {PreserveWhitespace = false};

            // Format the document to ignore white spaces.
            doc.LoadXml(xmlString);

            var reference = new Reference();
            if (!string.IsNullOrEmpty(nota.NotaId))
                reference.Uri = "#" + tagAssinatura + nota.NotaId;
            else if (!string.IsNullOrEmpty(uri))
                reference.Uri = uri;

            // Create a SignedXml object.
            var signedXml = new SignedXml(doc) {SigningKey = x509Cert.PrivateKey};

            // Add the key to the SignedXml document

            // Add an enveloped transformation to the reference.
            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            var c14 = new XmlDsigC14NTransform();
            reference.AddTransform(c14);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Create a new KeyInfo object
            var keyInfo = new KeyInfo();

            // Load the certificate into a KeyInfoX509Data object
            // and add it to the KeyInfo object.
            keyInfo.AddClause(new KeyInfoX509Data(x509Cert));

            // Add the KeyInfo object to the SignedXml object.
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            var xmlDigitalSignature = signedXml.GetXml();

            // Gravar o elemento no documento XML
            var assinaturaNodes = doc.GetElementsByTagName(tagAssinatura);
            foreach (XmlNode nodes in assinaturaNodes)
            {
                nodes.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
                break;
            }

            // Atualizar a string do XML já assinada
            var stringXmlAssinado = doc.OuterXml;

            //Atualiza a nota assinada
            nota.ConteudoXml = stringXmlAssinado;

            // Gravar o XML Assinado no HD
            var signedFile = nota.CaminhoFisico;
            var sw2 = File.CreateText(signedFile);
            sw2.Write(stringXmlAssinado);
            sw2.Close();

            return signedFile;
        }
    }
}
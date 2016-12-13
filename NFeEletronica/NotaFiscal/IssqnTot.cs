#region

using System.Xml.Serialization;

#endregion

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("ISSQNtot", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class IssqnTot
    {
        [XmlElement("vServ")]
        public string vServ { get; set; }

        [XmlElement("vBC")]
        public string vBC { get; set; }

        [XmlElement("dCompet")]
        public string dCompet { get; set; }

        [XmlElement("cRegTrib")]
        public string cRegTrib { get; set; }
    }
}
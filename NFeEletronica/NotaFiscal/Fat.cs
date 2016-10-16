#region

using System.Xml.Serialization;

#endregion

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("fat", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Fat
    {
        [XmlElement("nFat")]
        public string nFat { get; set; }

        [XmlElement("vOrig")]
        public string vOrig { get; set; }

        [XmlElement("vLiq")]
        public string vLiq { get; set; }
    }
}
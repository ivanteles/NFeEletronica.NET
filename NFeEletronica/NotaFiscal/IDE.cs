using System;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("ide", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Ide
    {
        public Ide()
        {
            //valores padrão
            cUF = "35";
            mod = "55";
            serie = "1";
            tpImp = "1";
            tpEmis = "1";
            cDV = "0";
            tpAmb = "2";
            finNFe = "1";
            procEmi = "3";
            verProc = "2.2.19";
        }

        [XmlElement("cUF")]
        public String cUF { get; set; }
        [XmlElement("cNF")]
        public String cNF { get; set; }
        [XmlElement("natOp")]
        public String natOp { get; set; }
        [XmlElement("indPag")]
        public String indPag { get; set; }
        [XmlElement("mod")]
        public String mod { get; set; }
        [XmlElement("serie")]
        public String serie { get; set; }
        [XmlElement("nNF")]
        public String nNF { get; set; }
        [XmlElement("dEmi")]
        public String dEmi { get; set; }
        [XmlElement("dhEmi")]
        public String dhEmi { get; set; }
        [XmlElement("dhSaiEnt")]
        public String dhSaiEnt { get; set; }
        [XmlElement("tpNF")]
        public String tpNF { get; set; }
        [XmlElement("idDest")]
        public String idDest { get; set; }
        [XmlElement("cMunFG")]
        public String cMunFG { get; set; }
        [XmlElement("tpImp")]
        public String tpImp { get; set; }
        [XmlElement("tpEmis")]
        public String tpEmis { get; set; }
        [XmlElement("cDV")]
        public String cDV { get; set; }
        [XmlElement("tpAmb")]
        public String tpAmb { get; set; }
        [XmlElement("finNFe")]
        public String finNFe { get; set; }
        [XmlElement("indFinal")]
        public String indFinal { get; set; }
        [XmlElement("indPres")]
        public String indPres { get; set; }
        [XmlElement("procEmi")]
        public String procEmi { get; set; }
        [XmlElement("verProc")]
        public String verProc { get; set; }
    }
}
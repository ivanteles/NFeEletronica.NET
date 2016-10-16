using System;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("det", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Det
    {

        [XmlElement("nItem")]
        public String nItem { get; set; }

        [XmlElement("prod")]
        public Prod Prod { get; set; }

        [XmlElement("imposto")]
        public Imposto Imposto { get; set; }


        //ICMS
        public GetIcms icms { get; set; }

        //CST
        public GetIpi ipi { get; set; }

        //PIS
        public GetPis pis { get; set; }

        //COFINS
        public GetCofins cofins { get; set; }

    }
}
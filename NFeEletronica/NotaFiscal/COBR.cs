#region

using System.Collections.Generic;
using System.Xml.Serialization;

#endregion

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("cobr", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Cobr
    {
        public Cobr()
        {
            dup = new List<DUP>();
        }

        [XmlElement("fat")]
        public Fat Fat { get; set; }

        [XmlElement("dup")]
        public List<DUP> dup { get; set; }
    }
}
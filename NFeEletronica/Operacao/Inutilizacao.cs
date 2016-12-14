using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using NFeEletronica.Assinatura;
using NFeEletronica.Contexto;
using NFeEletronica.NotaFiscal;
using NFeEletronica.Retorno;
using NFeEletronica.Utils;

namespace NFeEletronica.Operacao
{
    public class Inutilizacao : BaseOperacao
    {
        private readonly bool _producao;
        public readonly string ArquivoSchema;
        private readonly string _uf;

        public Inutilizacao(INFeContexto nfeContexto) : base(nfeContexto)
        {
            ArquivoSchema = "inutNFe_v" + nfeContexto.Versao.VersaoString + ".xsd";
            _producao = nfeContexto.Producao;
            _uf = nfeContexto.Uf;
        }

        public RetornoSimples NfeInutilizacaoNF2(NFeEletronica.Consulta.Inutilizacao inutilizacao)
        {
           

            

            var id = "ID" + inutilizacao.Uf + inutilizacao.Ano + inutilizacao.Cnpj +
                     Int32.Parse(inutilizacao.Mod).ToString("D2") + Int32.Parse(inutilizacao.Serie).ToString("D3") +
                     Int32.Parse(inutilizacao.NumeroNfeInicial).ToString("D9") +
                     Int32.Parse(inutilizacao.NumeroNfeFinal).ToString("D9");
            //Monta corpo do xml de envio
            var xmlString = new StringBuilder();
            xmlString.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xmlString.Append("<inutNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"2.00\">");
            xmlString.Append("<infInut Id=\"" + id + "\">");
            xmlString.Append("    <tpAmb>" + (NFeContexto.Producao ? "1" : "2") + "</tpAmb>");
            xmlString.Append("    <xServ>INUTILIZAR</xServ>");
            xmlString.Append("    <cUF>" + inutilizacao.Uf + "</cUF>");
            xmlString.Append("    <ano>" + inutilizacao.Ano + "</ano>");
            xmlString.Append("    <CNPJ>" + inutilizacao.Cnpj + "</CNPJ>");
            xmlString.Append("    <mod>" + inutilizacao.Mod + "</mod>");
            xmlString.Append("    <serie>" + inutilizacao.Serie + "</serie>");
            xmlString.Append("    <nNFIni>" + inutilizacao.NumeroNfeInicial + "</nNFIni>");
            xmlString.Append("    <nNFFin>" + inutilizacao.NumeroNfeFinal + "</nNFFin>");
            xmlString.Append("    <xJust>" + inutilizacao.Justificativa + "</xJust>");
            xmlString.Append("</infInut>");
            xmlString.Append("</inutNFe>");


            var assinado = Assinar(xmlString, id);

            if (_producao)
            {
                var webservice = new nfeInutilizacaoNF2.NfeInutilizacao2();
                var cabecalho = new nfeInutilizacaoNF2.nfeCabecMsg
                {
                    cUF = _uf,
                    versaoDados = NFeContexto.Versao.VersaoString
                };


                webservice.nfeCabecMsgValue = cabecalho;
                var resultado = webservice.nfeInutilizacaoNF2(assinado);


                var status = resultado["cStat"].InnerText;
                var motivo = resultado["xMotivo"].InnerText;
                return new RetornoSimples(status, motivo);
            }
            else
            {
                var webservice = new NfeInutilizacao21.NfeInutilizacao2();
                var cabecalho = new NfeInutilizacao21.nfeCabecMsg
                {
                    cUF = _uf,
                    versaoDados = NFeContexto.Versao.VersaoString
                };


                webservice.nfeCabecMsgValue = cabecalho;
                var resultado = webservice.nfeInutilizacaoNF2(assinado);


                var status = resultado["cStat"].InnerText;
                var motivo = resultado["xMotivo"].InnerText;
                return new RetornoSimples(status, motivo);
            }
        }

        private XmlNode Assinar(StringBuilder xmlStringBuilder, String id)
        {
            var bllXml = new Xml();
            var arquivoTemporario = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\temp.xml";
            var sw2 = File.CreateText(arquivoTemporario);
            sw2.Write(xmlStringBuilder.ToString());
            sw2.Close();

            var nota = new Nota(NFeContexto) {CaminhoFisico = arquivoTemporario};

            //Assina a nota
            var bllAssinatura = new AssinaturaDeXml();
            try
            {
                bllAssinatura.AssinarNota(nota, NFeContexto.Certificado, "inutNFe", "#" + id);
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao assinar Nota: " + e.Message);
            }

            //Verifica se a nota está de acordo com o schema, se não estiver vai disparar um erro
            try
            {
                bllXml.ValidaSchema(arquivoTemporario,
                    Util.ContentFolderSchemaValidacao + "\\" + NFeContexto.Versao.PastaXml + "\\" + ArquivoSchema);
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao validar Nota: " + e.Message);
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(arquivoTemporario);

            return xmlDoc;
        }
    }
}
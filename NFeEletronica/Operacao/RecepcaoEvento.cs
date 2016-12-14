﻿#region

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using NFeEletronica.Assinatura;
using NFeEletronica.Consulta;
using NFeEletronica.Contexto;
using NFeEletronica.nfeRecepcaoEvento;
using NFeEletronica.NotaFiscal;
using NFeEletronica.Retorno;
using NFeEletronica.Utils;

#endregion

namespace NFeEletronica.Operacao
{
    public class RecepcaoEvento : BaseOperacao
    {
        private readonly bool _producao;
        private readonly string _uf;

        public RecepcaoEvento(INFeContexto nfe)
            : base(nfe)
        {
            _producao = nfe.Producao;
            _uf = nfe.Uf;
        }

        private RetornoSimples EnviarEvento(StringBuilder eventoXml, string id, string arquivoEvento, string schema)
        {
            var documentXml = Assinar(eventoXml, id);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(arquivoEvento);
            var conteudoXml = xmlDoc.OuterXml;
            var nota = new Nota(NFeContexto) {CaminhoFisico = arquivoEvento};
            var bllXml = new Xml();
            var xmlString = new StringBuilder();
            xmlString.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xmlString.Append("<envEvento versao=\"1.00\" xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
            xmlString.Append("	<idLote>0131318</idLote>");
            xmlString.Append(conteudoXml);
            xmlString.Append("</envEvento>");

            var sw2 = File.CreateText(arquivoEvento);
            sw2.Write(xmlString.ToString());
            sw2.Close();

            //Verifica se a nota está de acordo com o schema, se não estiver vai disparar um erro
            try
            {
                bllXml.ValidaSchema(arquivoEvento,
                    Util.ContentFolderSchemaValidacao + "\\" + NFeContexto.Versao.PastaXml + "\\" + schema);
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao validar Nota: " + e.Message);
            }

            if (_producao)
            {
                var recepcao = new nfeRecepcaoEvento.RecepcaoEvento();
                var cabecalho = new nfeCabecMsg
                {
                    cUF = _uf,
                    versaoDados = "1.00"
                };

                recepcao.nfeCabecMsgValue = cabecalho;
                recepcao.ClientCertificates.Add(NFeContexto.Certificado);

                var resposta = recepcao.nfeRecepcaoEvento(Xml.StringToXml(xmlString.ToString()));

                var status = resposta["retEvento"]["infEvento"]["xMotivo"].InnerText;
                var motivo = resposta["retEvento"]["infEvento"]["cStat"].InnerText;
                return new RetornoSimples(status, motivo);
            }
            else
            {
                var recepcao = new NfeRecepcaoEvento1.RecepcaoEvento();
                var cabecalho = new NfeRecepcaoEvento1.nfeCabecMsg
                {
                    cUF = _uf,
                    versaoDados = "1.00"
                };

                recepcao.nfeCabecMsgValue = cabecalho;
                recepcao.ClientCertificates.Add(NFeContexto.Certificado);

                var resposta = recepcao.nfeRecepcaoEvento(Xml.StringToXml(xmlString.ToString()));

                var status = resposta["retEvento"]["infEvento"]["xMotivo"].InnerText;
                var motivo = resposta["retEvento"]["infEvento"]["cStat"].InnerText;
                return new RetornoSimples(status, motivo);
            }
        }

        public IRetorno CartaCorrecao(CartaCorrecao cartaCorrecao)
        {
            var tpEvento = "110110";
            var id = "ID" + tpEvento + cartaCorrecao.NotaChaveAcesso + "01";

            var xmlString = new StringBuilder();
            xmlString.Append("<evento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + "1.00" + "\">");
            xmlString.Append("  <infEvento Id=\"" + id + "\">");
            xmlString.Append("      <cOrgao>" + cartaCorrecao.CodigoUf + "</cOrgao>");
            xmlString.Append("      <tpAmb>" + (NFeContexto.Producao ? "1" : "2") + "</tpAmb>");
            xmlString.Append("      <CNPJ>" + cartaCorrecao.Cnpj + "</CNPJ>");
            xmlString.Append("      <chNFe>" + cartaCorrecao.NotaChaveAcesso + "</chNFe>");
            xmlString.Append("      <dhEvento>" + DateTime.Now.ToString("s") + "-03:00" + "</dhEvento>");
            xmlString.Append("      <tpEvento>" + tpEvento + "</tpEvento>");
            xmlString.Append("      <nSeqEvento>" + "1" + "</nSeqEvento>");
            xmlString.Append("      <verEvento>" + "1.00" + "</verEvento>");
            xmlString.Append("      <detEvento versao=\"" + "1.00" + "\">");
            xmlString.Append("         <descEvento>" + "Carta de Correcao" + "</descEvento>");
            xmlString.Append("         <xCorrecao>" + cartaCorrecao.Correcao + "</xCorrecao>");
            xmlString.Append(
                "         <xCondUso>A Carta de Correcao e disciplinada pelo paragrafo 1o-A do art. 7o do Convenio S/N, de 15 de dezembro de 1970 e pode ser utilizada para regularizacao de erro ocorrido na emissao de documento fiscal, desde que o erro nao esteja relacionado com: I - as variaveis que determinam o valor do imposto tais como: base de calculo, aliquota, diferenca de preco, quantidade, valor da operacao ou da prestacao; II - a correcao de dados cadastrais que implique mudanca do remetente ou do destinatario; III - a data de emissao ou de saida.</xCondUso>");
            xmlString.Append("      </detEvento>");
            xmlString.Append("  </infEvento>");
            xmlString.Append("</evento>");

            var arquivoTemporario = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\temp.xml";
            return EnviarEvento(xmlString, id, arquivoTemporario, "envCCe_v1.00.xsd");
        }

        public IRetorno Cancelar(Cancelamento eventoCancelamento, string caminhoXml)
        {
            var tpEvento = "110111";
            var id = "ID" + tpEvento + eventoCancelamento.NotaChaveAcesso + "01";

            var xmlString = new StringBuilder();
            xmlString.Append("	<evento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.00\">");
            xmlString.Append("		<infEvento Id=\"" + id + "\">");
            xmlString.Append("			<cOrgao>35</cOrgao>");
            xmlString.Append("			<tpAmb>" + (NFeContexto.Producao ? "1" : "2") + "</tpAmb>");
            xmlString.Append("			<CNPJ>" + eventoCancelamento.Cnpj + "</CNPJ>");
            xmlString.Append("			<chNFe>" + eventoCancelamento.NotaChaveAcesso + "</chNFe>");
            xmlString.Append("			<dhEvento>" + DateTime.Now.ToString("s") + "-03:00" + "</dhEvento>");
            xmlString.Append("			<tpEvento>" + tpEvento + "</tpEvento>");
            xmlString.Append("			<nSeqEvento>1</nSeqEvento>");
            xmlString.Append("			<verEvento>1.00</verEvento>");
            xmlString.Append("			<detEvento versao=\"1.00\">");
            xmlString.Append("				<descEvento>Cancelamento</descEvento>");
            xmlString.Append("				<nProt>" + eventoCancelamento.Protocolo + "</nProt>");
            xmlString.Append("				<xJust>" + eventoCancelamento.Justificativa + "</xJust>");
            xmlString.Append("			</detEvento>");
            xmlString.Append("		</infEvento>");
            xmlString.Append("	</evento>");

            var arquivoTemporario = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\temp.xml";
            return EnviarEvento(xmlString, id, arquivoTemporario, "envEventoCancNFe_v1.00.xsd");
        }

        private string Assinar(StringBuilder xmlStringBuilder, string id)
        {
            var arquivoTemporario = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\temp.xml";
            var sw2 = File.CreateText(arquivoTemporario);
            sw2.Write(xmlStringBuilder.ToString());
            sw2.Close();
            var nota = new Nota(NFeContexto) {CaminhoFisico = arquivoTemporario};
            //Assina a nota
            var bllAssinatura = new AssinaturaDeXml();
            try
            {
                bllAssinatura.AssinarNota(nota, NFeContexto.Certificado, "evento", "#" + id);
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao assinar Nota: " + e.Message);
            }

            return xmlStringBuilder.ToString();
        }
    }
}
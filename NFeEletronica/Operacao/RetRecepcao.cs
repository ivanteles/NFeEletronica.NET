#region

using System;
using System.Text;
using System.Threading;
using System.Xml;
using NFeEletronica.Assinatura;
using NFeEletronica.Contexto;
using NFeEletronica.nfeRecepcaoEvento;

#endregion

namespace NFeEletronica.Operacao
{
    /// <summary>
    ///     Consulta Processamento de Lote de NF-e
    /// </summary>
    public class RetRecepcao : BaseOperacao
    {
        public readonly string ArquivoSchema;
        private readonly bool _producao;
        private readonly string _uf;

        public RetRecepcao(INFeContexto nfe)
            : base(nfe)
        {
            ArquivoSchema = "inutNFe_v" + nfe.Versao.VersaoString + ".xsd";
            _producao = nfe.Producao;
            _uf = nfe.Uf;
        }

        public Retorno.RetRecepcao Enviar(string numeroRecibo)
        {
            //Monta corpo do xml de envio
            var xmlString = new StringBuilder();
            xmlString.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xmlString.Append("<consReciNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" +
                             NFeContexto.Versao.VersaoString + "\">");
            xmlString.Append("    <tpAmb>" + (NFeContexto.Producao ? "1" : "2") + "</tpAmb>");
            xmlString.Append("    <nRec>" + numeroRecibo + "</nRec>");
            xmlString.Append("</consReciNFe>");

            XmlNode consultaXml = Xml.StringToXml(xmlString.ToString());

            if (_producao)
            {
                var nfeRetRecepcao2 = new nfeRecepcaoEvento.RecepcaoEvento();
                var nfeCabecalho = new nfeCabecMsg
                {
                    cUF = _uf,
                    versaoDados = NFeContexto.Versao.VersaoString
                };

                //Informa dados no WS de cabecalho

                nfeRetRecepcao2.nfeCabecMsgValue = nfeCabecalho;
                nfeRetRecepcao2.ClientCertificates.Add(NFeContexto.Certificado);


                Retorno.RetRecepcao retorno;
                XmlNode respostaXml;

                var isEmProcessamento = true;

                //Verifica a resposta de envio da sefaz e aguarda até quando estiver processado
                do
                {
                    respostaXml = nfeRetRecepcao2.nfeRecepcaoEvento(consultaXml);

                    //Esse e o resultado só do lote (cabeçalho)
                    var status = respostaXml["cStat"].InnerText;
                    var motivo = respostaXml["xMotivo"].InnerText;
                    retorno = new Retorno.RetRecepcao("", "", status, motivo);

                    if (retorno.Status != "105")
                        isEmProcessamento = false;
                    else
                        Thread.Sleep(5000);
                } while (isEmProcessamento);


                if (retorno.Status != "225")
                {
                    //Isso aqui é o resultado de CADA NFe, mas como por enquanto pra cada lote só manda 1 nota, entao segue assim por enquanto #todo
                    if ((retorno.Status != "100") && (retorno.Status != "104"))
                        throw new Exception("Lote não processado: " + retorno.Status + " - " + retorno.Motivo);
                    var protocolo = "";
                    var status = "";
                    var motivo = "";

                    try
                    {
                        status = respostaXml["protNFe"]["infProt"]["cStat"].InnerText;
                        protocolo = respostaXml["protNFe"]["infProt"]["nProt"].InnerText;
                        motivo = respostaXml["protNFe"]["infProt"]["xMotivo"].InnerText;
                    }
                    catch
                    {
                        //ignora o erro
                    }

                    //Caso deu algum problema e nao veio o protocolo, mas veio a descrição do problema
                    if (string.IsNullOrEmpty(protocolo) && !string.IsNullOrEmpty(status) &&
                        !string.IsNullOrEmpty(motivo))
                        throw new Exception("Erro de retorno: " + status + " - " + motivo);

                    try
                    {
                        var numeroNota = respostaXml["protNFe"]["infProt"]["chNFe"].InnerText;
                        protocolo = respostaXml["protNFe"]["infProt"]["nProt"].InnerText;
                        status = respostaXml["protNFe"]["infProt"]["cStat"].InnerText;
                        motivo = respostaXml["protNFe"]["infProt"]["xMotivo"].InnerText;

                        return new Retorno.RetRecepcao(numeroNota, protocolo, status, motivo);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Erro ler resposta de envio: " + e.Message);
                    }
                }
                throw new Exception("Erro ao enviar lote XML: " + retorno.Motivo);
            }
            else
            {
                var nfeRetRecepcao2 = new NfeRecepcaoEvento1.RecepcaoEvento();
                var nfeCabecalho = new NfeRecepcaoEvento1.nfeCabecMsg
                {
                    cUF = _uf,
                    versaoDados = NFeContexto.Versao.VersaoString
                };

                //Informa dados no WS de cabecalho

                nfeRetRecepcao2.nfeCabecMsgValue = nfeCabecalho;
                nfeRetRecepcao2.ClientCertificates.Add(NFeContexto.Certificado);


                Retorno.RetRecepcao retorno;
                XmlNode respostaXml = null;

                var isEmProcessamento = true;

                //Verifica a resposta de envio da sefaz e aguarda até quando estiver processado
                do
                {
                    respostaXml = nfeRetRecepcao2.nfeRecepcaoEvento(consultaXml);

                    //Esse e o resultado só do lote (cabeçalho)
                    var status = respostaXml["cStat"].InnerText;
                    var motivo = respostaXml["xMotivo"].InnerText;
                    retorno = new Retorno.RetRecepcao("", "", status, motivo);

                    if (retorno.Status != "105")
                        isEmProcessamento = false;
                    else
                        Thread.Sleep(5000);
                } while (isEmProcessamento);


                if (retorno.Status != "225")
                {
                    //Isso aqui é o resultado de CADA NFe, mas como por enquanto pra cada lote só manda 1 nota, entao segue assim por enquanto #todo
                    if ((retorno.Status != "100") && (retorno.Status != "104"))
                        throw new Exception("Lote não processado: " + retorno.Status + " - " + retorno.Motivo);
                    var protocolo = "";
                    var status = "";
                    var motivo = "";

                    try
                    {
                        status = respostaXml["protNFe"]["infProt"]["cStat"].InnerText;
                        protocolo = respostaXml["protNFe"]["infProt"]["nProt"].InnerText;
                        motivo = respostaXml["protNFe"]["infProt"]["xMotivo"].InnerText;
                    }
                    catch
                    {
                        //ignora o erro
                    }

                    //Caso deu algum problema e nao veio o protocolo, mas veio a descrição do problema
                    if (string.IsNullOrEmpty(protocolo) && !string.IsNullOrEmpty(status) &&
                        !string.IsNullOrEmpty(motivo))
                        throw new Exception("Erro de retorno: " + status + " - " + motivo);

                    try
                    {
                        var numeroNota = respostaXml["protNFe"]["infProt"]["chNFe"].InnerText;
                        protocolo = respostaXml["protNFe"]["infProt"]["nProt"].InnerText;
                        status = respostaXml["protNFe"]["infProt"]["cStat"].InnerText;
                        motivo = respostaXml["protNFe"]["infProt"]["xMotivo"].InnerText;

                        return new Retorno.RetRecepcao(numeroNota, protocolo, status, motivo);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Erro ler resposta de envio: " + e.Message);
                    }
                }
                throw new Exception("Erro ao enviar lote XML: " + retorno.Motivo);
            }
        }
    }
}
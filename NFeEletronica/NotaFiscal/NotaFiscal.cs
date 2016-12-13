#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NFeEletronica.Contexto;
using NFeEletronica.Utils;
using NFeEletronica.Versao;

#endregion

namespace NFeEletronica.NotaFiscal
{
    public class Nota
    {
        private readonly INFeContexto nFeContexto;
        private readonly StringBuilder xmlString;
        public string ArquivoNome = "";
        public string CaminhoFisico = "";
        public string ConteudoXml = "";
        public List<Det> detList;
        public string NotaId = "";

        public Nota(INFeContexto nFeContexto)
        {
            this.nFeContexto = nFeContexto;

            ide = new Ide();
            emit = new Emit();
            dest = new Dest();
            detList = new List<Det>();
            total = new Total();
            transp = new Transp();
            cobr = new Cobr();

            xmlString = new StringBuilder();

            if (this.nFeContexto.Producao)
                ide.tpAmb = "1";
            else
                ide.tpAmb = "2";
        }

        public Nota(string arquivoNotaXml)
        {
            if (!File.Exists(arquivoNotaXml))
                throw new Exception("O arquivo de nota para envio não existe: " + arquivoNotaXml);

            detList = new List<Det>();

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(arquivoNotaXml);

            CaminhoFisico = arquivoNotaXml;
            ConteudoXml = xmlDoc.ToString();
        }

        public Ide ide { get; set; }
        public Emit emit { get; set; }
        public Dest dest { get; set; }
        public Total total { get; set; }
        public Transp transp { get; set; }
        public Cobr cobr { get; set; }
        public string infAdic { get; set; }

        public void AddDet(Det det)
        {
            detList.Add(det);
        }

        public string GerarCodigoDaNota()
        {
            if (!nFeContexto.Producao)
            {
                emit.xNome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";
                dest.xNome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";
            }

            var random = new Random();
            var codigoNumerico = random.Next(10000000, 99999999).ToString("D8");
            ide.cNF = codigoNumerico;

            var result = ide.cUF + ide.dhEmi.Replace("-", "").Substring(2, 4) + emit.CNPJ +
                         int.Parse(ide.mod).ToString("D2") + int.Parse(ide.serie).ToString("D3") +
                         int.Parse(ide.nNF).ToString("D9") + int.Parse(ide.tpEmis).ToString("D1") +
                         codigoNumerico;
            var digitoVerificador = Util.GerarModulo11(result);

            result = result + digitoVerificador;
            NotaId = result;

            ide.cDV = digitoVerificador;

            return NotaId;
        }

        public void SalvarNota(string caminho)
        {
            //GerarCodigoDaNota();


            xmlString.Append("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
            xmlString.Append("   <infNFe Id=\"NFe" + NotaId + "\" versao=\"" + nFeContexto.Versao.VersaoString + "\">");

            MontaIDE();
            MontaEMIT();
            MontaDEST();
            MontaDET();
            MontaTOTAL();
            MontaTRANSP();
            if (cobr != null)
                MontaCOBR();

            if (!string.IsNullOrEmpty(infAdic))
            {
                xmlString.Append("<infAdic>");
                xmlString.Append("	<infCpl>");
                xmlString.Append(infAdic);
                xmlString.Append("	</infCpl>");
                xmlString.Append("</infAdic>");
            }

            xmlString.Append("   </infNFe>");
            //this.XmlString.Append("   <Signature></Signature>"); acho que não precisa disso
            xmlString.Append("</NFe>");

            var xmlDocument = new XmlDocument();

            try
            {
                xmlDocument.LoadXml(xmlString.ToString().Replace("&", "&amp;"));
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar a nota como XML 2: " + e.Message);
            }

            try
            {
                xmlDocument.Save(caminho);
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao salvar a nota como XML: " + e.Message);
            }

            CaminhoFisico = caminho;
            ConteudoXml = xmlString.ToString();
        }

        private void MontaIDE()
        {
            xmlString.Append("<ide>");
            xmlString.Append("	<cUF>" + ide.cUF + "</cUF>");
            xmlString.Append("	<cNF>" + ide.cNF + "</cNF>");
            xmlString.Append("	<natOp>" + ide.natOp + "</natOp>");
            xmlString.Append("	<indPag>" + ide.indPag + "</indPag>"); //0 - a vista, 1 - a prazo, 2 - outros
            xmlString.Append("	<mod>" + ide.mod + "</mod>");
            xmlString.Append("	<serie>" + ide.serie + "</serie>");
            xmlString.Append("	<nNF>" + ide.nNF + "</nNF>");

            if (nFeContexto.Versao == NFeVersao.VERSAO_3_1_0)
                xmlString.Append("	<dhEmi>" + ide.dhEmi + "</dhEmi>");
            else
                xmlString.Append("	<dEmi>" + ide.dEmi + "</dEmi>");

            if (!string.IsNullOrEmpty(ide.dhSaiEnt))
                xmlString.Append("	<dhSaiEnt>" + ide.dhSaiEnt + "</dhSaiEnt>");

            xmlString.Append("	<tpNF>" + ide.tpNF + "</tpNF>");

            if (nFeContexto.Versao == NFeVersao.VERSAO_3_1_0)
                xmlString.Append("	<idDest>" + ide.idDest + "</idDest>");

            xmlString.Append("	<cMunFG>" + ide.cMunFG + "</cMunFG>");
            xmlString.Append("	<tpImp>" + ide.tpImp + "</tpImp>");
            xmlString.Append("	<tpEmis>" + ide.tpEmis + "</tpEmis>");
            xmlString.Append("	<cDV>" + ide.cDV + "</cDV>");
            xmlString.Append("	<tpAmb>" + ide.tpAmb + "</tpAmb>");

            xmlString.Append("	<finNFe>" + ide.finNFe + "</finNFe>");

            if (nFeContexto.Versao == NFeVersao.VERSAO_3_1_0)
            {
                //XmlString.Append("	<indFinal>" + ide.indFinal + "</indFinal>");
                //XmlString.Append("	<indPres>" + ide.indPres + "</indPres>");
            }

            xmlString.Append("	<procEmi>" + ide.procEmi + "</procEmi>");
            xmlString.Append("	<verProc>1</verProc>");
            xmlString.Append("</ide>");
        }

        private void MontaEMIT()
        {
            xmlString.Append("<emit>");

            if (!string.IsNullOrEmpty(emit.CPF))
                xmlString.Append("	<CPF>" + emit.CPF + "</CPF>");
            if (!string.IsNullOrEmpty(emit.CNPJ))
                xmlString.Append("	<CNPJ>" + emit.CNPJ + "</CNPJ>");

            xmlString.Append("	<xNome>" + emit.xNome + "</xNome>");
            xmlString.Append("	<enderEmit>");
            xmlString.Append("		<xLgr>" + emit.EnderEmit.xLgr + "</xLgr>");
            xmlString.Append("		<nro>" + emit.EnderEmit.nro + "</nro>");
            xmlString.Append("		<xBairro>" + emit.EnderEmit.xBairro + "</xBairro>");
            xmlString.Append("		<cMun>" + emit.EnderEmit.cMun + "</cMun>");
            xmlString.Append("		<xMun>" + emit.EnderEmit.xMun + "</xMun>");
            xmlString.Append("		<UF>" + emit.EnderEmit.UF.Trim() + "</UF>");
            xmlString.Append("		<CEP>" + emit.EnderEmit.CEP + "</CEP>");
            xmlString.Append("		<cPais>1058</cPais>");
            xmlString.Append("		<xPais>BRASIL</xPais>");

            if (!string.IsNullOrEmpty(emit.EnderEmit.fone))
                xmlString.Append("		<fone>" + emit.EnderEmit.fone + "</fone>");

            xmlString.Append("	</enderEmit>");

            xmlString.Append("	<IE>" + emit.IE + "</IE>");
            xmlString.Append("	<CRT>" + emit.CRT + "</CRT>");
            xmlString.Append("</emit>");
        }

        private void MontaDEST()
        {
            xmlString.Append("<dest>");

            if (!string.IsNullOrEmpty(dest.CPF))
                xmlString.Append("	<CPF>" + dest.CPF + "</CPF>");

            if (!string.IsNullOrEmpty(dest.CNPJ))
                xmlString.Append("	<CNPJ>" + dest.CNPJ + "</CNPJ>");

            if (!string.IsNullOrEmpty(dest.idEstrangeiro))
                xmlString.Append("	<idEstrangeiro>" + dest.idEstrangeiro + "</idEstrangeiro>");

            xmlString.Append("	<xNome>" + dest.xNome + "</xNome>");
            xmlString.Append("	<enderDest>");
            xmlString.Append("		<xLgr>" + dest.EnderDest.xLgr + "</xLgr>");
            xmlString.Append("		<nro>" + dest.EnderDest.nro + "</nro>");

            if (!string.IsNullOrEmpty(dest.EnderDest.xCpl))
                xmlString.Append("		<xCpl>" + dest.EnderDest.xCpl + "</xCpl>");

            xmlString.Append("		<xBairro>" + dest.EnderDest.xBairro + "</xBairro>");
            xmlString.Append("		<cMun>" + dest.EnderDest.cMun + "</cMun>");
            xmlString.Append("		<xMun>" + dest.EnderDest.xMun + "</xMun>");
            xmlString.Append("		<UF>" + dest.EnderDest.UF.Trim() + "</UF>");

            if (!string.IsNullOrEmpty(dest.EnderDest.CEP))
                xmlString.Append("		<CEP>" + dest.EnderDest.CEP + "</CEP>");

            xmlString.Append("		<cPais>1058</cPais>");
            xmlString.Append("		<xPais>BRASIL</xPais>");

            if (!string.IsNullOrEmpty(dest.fone))
                xmlString.Append("		<fone>" + dest.fone + "</fone>");

            xmlString.Append("	</enderDest>");

            if (nFeContexto.Versao == NFeVersao.VERSAO_3_1_0)
                xmlString.Append("	<indIEDest>" + dest.indIEDest + "</indIEDest>");


            xmlString.Append("	<IE>" + dest.IE + "</IE>");


            if ((nFeContexto.Versao == NFeVersao.VERSAO_3_1_0) && !string.IsNullOrEmpty(dest.email))
                xmlString.Append("	<email>" + dest.email + "</email>");

            xmlString.Append("</dest>");
        }

        private void MontaDET()
        {
            for (var i = 0; i < detList.Count; i++)
            {
                xmlString.Append("<det nItem=\"" + (i + 1) + "\">");
                xmlString.Append("	<prod>");
                xmlString.Append("		<cProd>" + detList[i].Prod.cProd.Trim() + "</cProd>");
                xmlString.Append("		<cEAN>" + detList[i].Prod.cEAN + "</cEAN>");
                xmlString.Append("		<xProd>" + detList[i].Prod.xProd + "</xProd>");
                xmlString.Append("		<NCM>" + detList[i].Prod.NCM + "</NCM>");
                xmlString.Append("		<CFOP>" + detList[i].Prod.CFOP + "</CFOP>");
                xmlString.Append("		<uCom>" + detList[i].Prod.uCom + "</uCom>");
                xmlString.Append("		<qCom>" + detList[i].Prod.qCom + "</qCom>");
                xmlString.Append("		<vUnCom>" + detList[i].Prod.vUnCom + "</vUnCom>");
                xmlString.Append("		<vProd>" + detList[i].Prod.vProd + "</vProd>");
                xmlString.Append("		<cEANTrib>" + detList[i].Prod.cEANTrib + "</cEANTrib>");
                xmlString.Append("		<uTrib>" + detList[i].Prod.uTrib + "</uTrib>");
                xmlString.Append("		<qTrib>" + detList[i].Prod.qTrib + "</qTrib>");
                xmlString.Append("		<vUnTrib>" + detList[i].Prod.vUnTrib + "</vUnTrib>");

                if (!string.IsNullOrEmpty(detList[i].Prod.vFrete))
                    xmlString.Append("		<vFrete>" + detList[i].Prod.vFrete + "</vFrete>");
                if (!string.IsNullOrEmpty(detList[i].Prod.vDesc))
                    xmlString.Append("		<vDesc>" + detList[i].Prod.vDesc + "</vDesc>");

                xmlString.Append("		<indTot>" + detList[i].Prod.indTot + "</indTot>");
                xmlString.Append("	</prod>");

                xmlString.Append("	<imposto>");

                if (!string.IsNullOrEmpty(detList[i].Prod.vTotTrib))
                    xmlString.Append("	<vTotTrib>" + detList[i].Prod.vTotTrib + "</vTotTrib>");

                if (detList[i].Imposto.Icms != null)
                    MontaDET_ICMS(detList[i]);
                if (detList[i].Imposto.Ipi != null)
                    MontaDET_IPI(detList[i]);
                if (detList[i].Imposto.Pis != null)
                    MontaDET_PIS(detList[i]);
                if (detList[i].Imposto.Cofins != null)
                    MontaDET_COFINS(detList[i]);

                if (detList[i].Imposto.Issqn != null)
                    MontaDET_ISQN(detList[i]);
                xmlString.Append("	</imposto>");

                xmlString.Append("</det>");
            }
        }

        private void MontaDET_ICMS(Det det)
        {
            xmlString.Append("<ICMS>");

            switch (det.icms)
            {
                case GetIcms.ICMS00:
                    xmlString.Append("<ICMS00>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");
                    xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    xmlString.Append("</ICMS00>");
                    break;
                case GetIcms.ICMS10:
                    xmlString.Append("<ICMS10>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");
                    xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pMVAST))
                        xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");

                    xmlString.Append("</ICMS10>");
                    break;
                case GetIcms.ICMS20:
                    xmlString.Append("<ICMS20>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    xmlString.Append("    <pRedBC>" + det.Imposto.Icms.pRedBC + "</pRedBC>");
                    xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");
                    xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    xmlString.Append("</ICMS20>");
                    break;
                case GetIcms.ICMS30:
                    xmlString.Append("<ICMS30>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");
                    xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");
                    xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");
                    xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");
                    xmlString.Append("</ICMS30>");
                    break;
                case GetIcms.ICMS40_50:
                    xmlString.Append("<ICMS40>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    xmlString.Append("</ICMS40>");
                    break;
                case GetIcms.ICMS51:
                    xmlString.Append("<ICMS51>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    xmlString.Append("</ICMS51>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <pRedBC>" + det.Imposto.Icms.pRedBC + "</pRedBC>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    break;
                case GetIcms.ICMS60:
                    xmlString.Append("<ICMS60>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    xmlString.Append("</ICMS60>");
                    break;
                case GetIcms.ICMS70:
                    xmlString.Append("<ICMS70>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    xmlString.Append("    <pRedBC>" + det.Imposto.Icms.pRedBC + "</pRedBC>");
                    xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");
                    xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pMVAST))
                        xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");
                    xmlString.Append("</ICMS70>");
                    break;
                case GetIcms.ICMS90:
                    xmlString.Append("<ICMS90>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBC))
                        xmlString.Append("    <pRedBC>" + det.Imposto.Icms.pRedBC + "</pRedBC>");

                    xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pMVAST))
                        xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");
                    xmlString.Append("</ICMS90>");
                    break;
                case GetIcms.ICMS101:
                    xmlString.Append("<ICMSSN101>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    xmlString.Append("    <pCredSN>" + det.Imposto.Icms.pCredSN + "</pCredSN>");
                    xmlString.Append("    <vCredICMSSN>" + det.Imposto.Icms.vCredICMSSN + "</vCredICMSSN>");
                    xmlString.Append("</ICMSSN101>");
                    break;
                case GetIcms.ICMS102_400:
                    xmlString.Append("<ICMSSN102>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    xmlString.Append("</ICMSSN102>");
                    break;
                case GetIcms.ICMS201:
                    xmlString.Append("<ICMSSN201>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");

                    xmlString.Append("</ICMSSN201>");
                    break;
                case GetIcms.ICMS202:
                    xmlString.Append("<ICMSSN202>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pMVAST))
                        xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");

                    xmlString.Append("</ICMSSN202>");
                    break;
                case GetIcms.ICMS500:
                    xmlString.Append("<ICMSSN500>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    xmlString.Append("    <vBCSTRet>" + det.Imposto.Icms.vBCSTRet + "</vBCSTRet>");
                    xmlString.Append("    <vICMSSTRet>" + det.Imposto.Icms.vICMSSTRet + "</vICMSSTRet>");
                    xmlString.Append("</ICMSSN500>");
                    break;
                case GetIcms.ICMS900:
                    xmlString.Append("<ICMSSN900>");
                    xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBC))
                        xmlString.Append("    <pRedBC>" + det.Imposto.Icms.pRedBC + "</pRedBC>");

                    xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pMVAST))
                        xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");
                    xmlString.Append("    <pCredSN>" + det.Imposto.Icms.pCredSN + "</pCredSN>");
                    xmlString.Append("    <vCredICMSSN>" + det.Imposto.Icms.vCredICMSSN + "</vCredICMSSN>");
                    xmlString.Append("</ICMSSN900>");
                    break;
            }

            xmlString.Append("</ICMS>");
        }

        private void MontaDET_IPI(Det det)
        {
            if (!string.IsNullOrEmpty(det.Imposto.Ipi.cIEnq))
            {
                xmlString.Append("<IPI>");

                xmlString.Append("<cEnq>" + det.Imposto.Ipi.cIEnq + "</cEnq>");

                switch (det.ipi)
                {
                    case GetIpi.IPI00_49_50_99:
                        xmlString.Append("<IPITrib>");

                        xmlString.Append("    <CST>" + det.Imposto.Ipi.CST + "</CST>");

                        if (!string.IsNullOrEmpty(det.Imposto.Ipi.vBC))
                        {
                            xmlString.Append("    <vBC>" + det.Imposto.Ipi.vBC + "</vBC>");
                            xmlString.Append("    <pIPI>" + det.Imposto.Ipi.pIPI + "</pIPI>");
                        }
                        else
                        {
                            xmlString.Append("    <qUnid>" + det.Imposto.Ipi.qUnid + "</qUnid>");
                            xmlString.Append("    <vUnid>" + det.Imposto.Ipi.vUnid + "</vUnid>");
                        }

                        xmlString.Append("    <vIPI>" + det.Imposto.Ipi.vIPI + "</vIPI>");

                        xmlString.Append("</IPITrib>");
                        break;
                    case GetIpi.IPI01_55:
                        xmlString.Append("<IPINT>");
                        xmlString.Append("    <CST>" + det.Imposto.Ipi.CST + "</CST>");
                        xmlString.Append("</IPINT>");
                        break;
                }

                xmlString.Append("</IPI>");
            }
        }

        private void MontaDET_PIS(Det det)
        {
            xmlString.Append("<PIS>");

            switch (det.pis)
            {
                case GetPis.PIS01_02:
                    xmlString.Append("<PISAliq>");
                    xmlString.Append("    <CST>" + det.Imposto.Pis.CST + "</CST>");
                    xmlString.Append("    <vBC>" + det.Imposto.Pis.vBC + "</vBC>");
                    xmlString.Append("    <pPIS>" + det.Imposto.Pis.pPIS + "</pPIS>");
                    xmlString.Append("    <vPIS>" + det.Imposto.Pis.vPIS + "</vPIS>");
                    xmlString.Append("</PISAliq>");
                    break;
                case GetPis.PIS03:
                    xmlString.Append("<PISQtde>");
                    xmlString.Append("    <CST>" + det.Imposto.Pis.CST + "</CST>");
                    xmlString.Append("    <qBCProd>" + det.Imposto.Pis.qBCProd + "</qBCProd>");
                    xmlString.Append("    <vAliqProd>" + det.Imposto.Pis.vAliqProd + "</vAliqProd>");
                    xmlString.Append("    <vPIS>" + det.Imposto.Pis.vPIS + "</vPIS>");
                    xmlString.Append("</PISQtde>");
                    break;
                case GetPis.PIS04_09:
                    xmlString.Append("<PISNT>");
                    xmlString.Append("    <CST>" + det.Imposto.Pis.CST + "</CST>");
                    xmlString.Append("</PISNT>");
                    break;
                case GetPis.PIS99:
                    xmlString.Append("<PISOutr>");
                    xmlString.Append("    <CST>" + det.Imposto.Pis.CST + "</CST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Pis.vBC) && !string.IsNullOrEmpty(det.Imposto.Pis.pPIS))
                    {
                        xmlString.Append("    <vBC>" + det.Imposto.Pis.vBC + "</vBC>");
                        xmlString.Append("    <pPIS>" + det.Imposto.Pis.pPIS + "</pPIS>");
                    }
                    else
                    {
                        xmlString.Append("    <qBCProd>" + det.Imposto.Pis.qBCProd + "</qBCProd>");
                        xmlString.Append("    <vAliqProd>" + det.Imposto.Pis.vAliqProd + "</vAliqProd>");
                    }

                    xmlString.Append("    <vPIS>" + det.Imposto.Pis.vPIS + "</vPIS>");
                    xmlString.Append("</PISOutr>");
                    break;
            }

            xmlString.Append("</PIS>");
        }

        private void MontaDET_COFINS(Det det)
        {
            xmlString.Append("<COFINS>");

            switch (det.cofins)
            {
                case GetCofins.CST01_02:
                    xmlString.Append("<COFINSAliq>");
                    xmlString.Append("    <CST>" + det.Imposto.Cofins.CST + "</CST>");
                    xmlString.Append("    <vBC>" + det.Imposto.Cofins.vBC + "</vBC>");
                    xmlString.Append("    <pCOFINS>" + det.Imposto.Cofins.pCOFINS + "</pCOFINS>");
                    xmlString.Append("    <vCOFINS>" + det.Imposto.Cofins.vCOFINS + "</vCOFINS>");
                    xmlString.Append("</COFINSAliq>");
                    break;
                case GetCofins.CST03:
                    xmlString.Append("<COFINSQtde>");
                    xmlString.Append("    <CST>" + det.Imposto.Cofins.CST + "</CST>");
                    xmlString.Append("    <qBCProd>" + det.Imposto.Cofins.qBCProd + "</qBCProd>");
                    xmlString.Append("    <vAliqProd>" + det.Imposto.Cofins.vAliqProd + "</vAliqProd>");
                    xmlString.Append("    <vCOFINS>" + det.Imposto.Cofins.vCOFINS + "</vCOFINS>");
                    xmlString.Append("</COFINSQtde>");
                    break;
                case GetCofins.CST04_09:
                    xmlString.Append("<COFINSNT>");
                    xmlString.Append("    <CST>" + det.Imposto.Cofins.CST + "</CST>");
                    xmlString.Append("</COFINSNT>");
                    break;
                case GetCofins.CST99:
                    xmlString.Append("<COFINSOutr>");
                    xmlString.Append("    <CST>" + det.Imposto.Cofins.CST + "</CST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Cofins.vBC) &&
                        !string.IsNullOrEmpty(det.Imposto.Cofins.pCOFINS))
                    {
                        xmlString.Append("    <vBC>" + det.Imposto.Cofins.vBC + "</vBC>");
                        xmlString.Append("    <pCOFINS>" + det.Imposto.Cofins.pCOFINS + "</pCOFINS>");
                    }
                    else
                    {
                        xmlString.Append("    <qBCProd>" + det.Imposto.Cofins.qBCProd + "</qBCProd>");
                        xmlString.Append("    <vAliqProd>" + det.Imposto.Cofins.vAliqProd + "</vAliqProd>");
                    }

                    xmlString.Append("    <vCOFINS>" + det.Imposto.Cofins.vCOFINS + "</vCOFINS>");
                    xmlString.Append("</COFINSOutr>");
                    break;
            }

            xmlString.Append("</COFINS>");
        }

        private void MontaDET_ISQN(Det det)
        {
            xmlString.Append("<ISSQN>");
            xmlString.Append("    <vBC>" + det.Imposto.Issqn.vBC + "</vBC>");
            xmlString.Append("    <vAliq>" + det.Imposto.Issqn.vAliq + "</vAliq>");
            xmlString.Append("    <vISSQN>" + det.Imposto.Issqn.vISSQN + "</vISSQN>");
            xmlString.Append("    <cMunFG>" + det.Imposto.Issqn.cMunFG + "</cMunFG>");
            xmlString.Append("    <cListServ>" + det.Imposto.Issqn.cListServ + "</cListServ>");
            xmlString.Append("    <indISS>" + det.Imposto.Issqn.indISS + "</indISS>");
            xmlString.Append("    <indIncentivo>" + det.Imposto.Issqn.indIncentivo + "</indIncentivo>");
            xmlString.Append("</ISSQN>");
        }

        private void MontaTOTAL()
        {
            xmlString.Append("<total>");
            if (total.IcmsTotal != null)
            {
                xmlString.Append("	<ICMSTot>");
                xmlString.Append("		<vBC>" + total.IcmsTotal.vBC + "</vBC>");
                xmlString.Append("		<vICMS>" + total.IcmsTotal.vICMS + "</vICMS>");
                if (nFeContexto.Versao == NFeVersao.VERSAO_3_1_0)
                    xmlString.Append("		<vICMSDeson>" + total.IcmsTotal.vICMSDeson + "</vICMSDeson>");
                xmlString.Append("		<vBCST>" + total.IcmsTotal.vBCST + "</vBCST>");
                xmlString.Append("		<vST>" + total.IcmsTotal.vST + "</vST>");
                xmlString.Append("		<vProd>" + total.IcmsTotal.vProd + "</vProd>");
                xmlString.Append("		<vFrete>" + total.IcmsTotal.vFrete + "</vFrete>");
                xmlString.Append("		<vSeg>" + total.IcmsTotal.vSeg + "</vSeg>");
                xmlString.Append("		<vDesc>" + total.IcmsTotal.vDesc + "</vDesc>");
                xmlString.Append("		<vII>0.00</vII>");
                xmlString.Append("		<vIPI>" + total.IcmsTotal.vIPI + "</vIPI>");
                xmlString.Append("		<vPIS>0.00</vPIS>");
                xmlString.Append("		<vCOFINS>0.00</vCOFINS>");
                xmlString.Append("		<vOutro>" + total.IcmsTotal.vOutro + "</vOutro>");
                xmlString.Append("		<vNF>" + total.IcmsTotal.vNF + "</vNF>");

                if (!string.IsNullOrEmpty(total.IcmsTotal.vTotTrib))
                    xmlString.Append("		<vTotTrib>" + total.IcmsTotal.vTotTrib + "</vTotTrib>");

                xmlString.Append("	</ICMSTot>");
            }

            if (total.IssqnTot != null)
            {
                xmlString.Append("	<ISSQNtot>");
                xmlString.Append("		<vServ>" + total.IssqnTot.vServ + "</vServ>");
                xmlString.Append("		<vBC>" + total.IssqnTot.vBC + "</vBC>");
                xmlString.Append("		<dCompet>" + total.IssqnTot.dCompet + "</dCompet>");
                xmlString.Append("		<cRegTrib>" + total.IssqnTot.cRegTrib + "</cRegTrib>");
                xmlString.Append("	</ISSQNtot>");
            }
            xmlString.Append("</total>");
        }

        private void MontaTRANSP()
        {
            xmlString.Append("<transp>");
            xmlString.Append("	<modFrete>" + transp.modFrete + "</modFrete>");

            if (!string.IsNullOrEmpty(transp.CNPJ))
            {
                xmlString.Append("	<transporta>");

                if (!string.IsNullOrEmpty(transp.CNPJ))
                    xmlString.Append("		<CNPJ>" + transp.CNPJ + "</CNPJ>");

                /*
                if(!String.IsNullOrEmpty(this.transp.CPF))
                    this.XmlString.Append("		<CPF>" + this.transp.CPF + "</CPF>");
                */

                if (!string.IsNullOrEmpty(transp.xNome))
                    xmlString.Append("		<xNome>" + transp.xNome + "</xNome>");

                if (!string.IsNullOrEmpty(transp.IE))
                    xmlString.Append("		<IE>" + transp.IE + "</IE>");

                if (!string.IsNullOrEmpty(transp.xEnder))
                    xmlString.Append("		<xEnder>" + transp.xEnder + "</xEnder>");

                if (!string.IsNullOrEmpty(transp.xMun))
                    xmlString.Append("		<xMun>" + transp.xMun + "</xMun>");

                if (!string.IsNullOrEmpty(transp.UF))
                    xmlString.Append("		<UF>" + transp.UF.Trim() + "</UF>");

                xmlString.Append("	</transporta>");
            }
            if (!string.IsNullOrEmpty(transp.veic_placa))
            {
                xmlString.Append("	<veicTransp>");
                xmlString.Append("		<placa>" + transp.veic_placa + "</placa>");
                xmlString.Append("		<UF>" + transp.veic_UF + "</UF>");
                xmlString.Append("	</veicTransp>");
            }

            if ((transp.Vol != null) && !string.IsNullOrEmpty(transp.Vol.qVol))
            {
                xmlString.Append("	<vol>");
                xmlString.Append("		<qVol>" + transp.Vol.qVol + "</qVol>");
                xmlString.Append("		<esp>" + transp.Vol.esp + "</esp>");


                if (!string.IsNullOrEmpty(transp.Vol.marca))
                    xmlString.Append("		<marca>" + transp.Vol.marca + "</marca>");

                if (!string.IsNullOrEmpty(transp.Vol.nVol))
                    xmlString.Append("		<nVol>" + transp.Vol.nVol + "</nVol>");

                if (!string.IsNullOrEmpty(transp.Vol.pesoL))
                    xmlString.Append("		<pesoL>" + transp.Vol.pesoL + "</pesoL>");

                if (!string.IsNullOrEmpty(transp.Vol.pesoB))
                    xmlString.Append("		<pesoB>" + transp.Vol.pesoB + "</pesoB>");

                xmlString.Append("	</vol>");
            }
            xmlString.Append("</transp>");
        }

        private void MontaCOBR()
        {
            if ((cobr.Fat != null) && !string.IsNullOrEmpty(cobr.Fat.nFat))
            {
                xmlString.Append("<cobr>");
                xmlString.Append("	<fat>");
                xmlString.Append("		<nFat>" + cobr.Fat.nFat + "</nFat>");
                xmlString.Append("		<vOrig>" + cobr.Fat.vOrig + "</vOrig>");
                xmlString.Append("		<vLiq>" + cobr.Fat.vLiq + "</vLiq>");
                xmlString.Append("	</fat>");

                for (var i = 0; i < cobr.dup.Count; i++)
                {
                    xmlString.Append("	<dup>");
                    xmlString.Append("		<nDup>" + cobr.dup[i].nDup + "</nDup>");
                    xmlString.Append("		<dVenc>" + cobr.dup[i].dVenc + "</dVenc>");
                    xmlString.Append("		<vDup>" + cobr.dup[i].vDup + "</vDup>");
                    xmlString.Append("	</dup>");
                }

                xmlString.Append("</cobr>");
            }
        }
    }
}
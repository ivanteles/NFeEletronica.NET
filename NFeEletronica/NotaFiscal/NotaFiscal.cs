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
        private readonly INFeContexto _nFeContexto;
        private readonly StringBuilder _xmlString;
        public string ArquivoNome = "";
        public string CaminhoFisico = "";
        public string ConteudoXml = "";
        public List<Det> DetList;
        public string NotaId = "";

        public Nota(INFeContexto nFeContexto)
        {
            _nFeContexto = nFeContexto;

            Ide = new Ide();
            Emit = new Emit();
            Dest = new Dest();
            DetList = new List<Det>();
            Total = new Total();
            Transp = new Transp();
            Cobr = new Cobr();

            _xmlString = new StringBuilder();

            Ide.tpAmb = _nFeContexto.Producao ? "1" : "2";
        }

        public Nota(string arquivoNotaXml)
        {
            if (!File.Exists(arquivoNotaXml))
                throw new Exception("O arquivo de nota para envio não existe: " + arquivoNotaXml);

            DetList = new List<Det>();

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(arquivoNotaXml);

            CaminhoFisico = arquivoNotaXml;
            ConteudoXml = xmlDoc.ToString();
        }

        public Ide Ide { get; set; }
        public Emit Emit { get; set; }
        public Dest Dest { get; set; }
        public Total Total { get; set; }
        public Transp Transp { get; set; }
        public Cobr Cobr { get; set; }
        public string InfAdic { get; set; }

        public void AddDet(Det det)
        {
            DetList.Add(det);
        }

        public string GerarCodigoDaNota()
        {
            if (!_nFeContexto.Producao)
            {
                Emit.xNome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";
                Dest.xNome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";
            }

            var random = new Random();
            var codigoNumerico = random.Next(10000000, 99999999).ToString("D8");
            Ide.cNF = codigoNumerico;

            var result = Ide.cUF + Ide.dhEmi.Replace("-", "").Substring(2, 4) + Emit.CNPJ +
                         int.Parse(Ide.mod).ToString("D2") + int.Parse(Ide.serie).ToString("D3") +
                         int.Parse(Ide.nNF).ToString("D9") + int.Parse(Ide.tpEmis).ToString("D1") +
                         codigoNumerico;
            var digitoVerificador = Util.GerarModulo11(result);

            result = result + digitoVerificador;
            NotaId = result;

            Ide.cDV = digitoVerificador;

            return NotaId;
        }

        public void SalvarNota(string caminho)
        {
            //GerarCodigoDaNota();


            _xmlString.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            _xmlString.Append("<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" +
                             _nFeContexto.Versao.VersaoString + "\">");
            _xmlString.Append("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
            _xmlString.Append("   <infNFe Id=\"NFe" + NotaId + "\" versao=\"" + _nFeContexto.Versao.VersaoString + "\">");

            MontaIde();
            MontaEmit();
            MontaDest();
            MontaDet();
            MontaTotal();
            MontaTransp();
            if (Cobr != null)
                MontaCobr();

            if (!string.IsNullOrEmpty(InfAdic))
            {
                _xmlString.Append("<infAdic>");
                _xmlString.Append("	<infCpl>");
                _xmlString.Append(InfAdic);
                _xmlString.Append("	</infCpl>");
                _xmlString.Append("</infAdic>");
            }

            _xmlString.Append("   </infNFe>");
            //this.XmlString.Append("   <Signature></Signature>"); acho que não precisa disso
            _xmlString.Append("</NFe>");
            _xmlString.Append("</nfeProc>");

            var xmlDocument = new XmlDocument();

            try
            {
                xmlDocument.LoadXml(_xmlString.ToString().Replace("&", "&amp;"));
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
            ConteudoXml = _xmlString.ToString();
        }

        private void MontaIde()
        {
            _xmlString.Append("<ide>");
            _xmlString.Append("	<cUF>" + Ide.cUF + "</cUF>");
            _xmlString.Append("	<cNF>" + Ide.cNF + "</cNF>");
            _xmlString.Append("	<natOp>" + Ide.natOp + "</natOp>");
            _xmlString.Append("	<indPag>" + Ide.indPag + "</indPag>"); //0 - a vista, 1 - a prazo, 2 - outros
            _xmlString.Append("	<mod>" + Ide.mod + "</mod>");
            _xmlString.Append("	<serie>" + Ide.serie + "</serie>");
            _xmlString.Append("	<nNF>" + Ide.nNF + "</nNF>");

            if (_nFeContexto.Versao == NFeVersao.Versao310)
                _xmlString.Append("	<dhEmi>" + Ide.dhEmi + "</dhEmi>");
            else
                _xmlString.Append("	<dEmi>" + Ide.dEmi + "</dEmi>");

            if (!string.IsNullOrEmpty(Ide.dhSaiEnt))
                _xmlString.Append("	<dhSaiEnt>" + Ide.dhSaiEnt + "</dhSaiEnt>");

            _xmlString.Append("	<tpNF>" + Ide.tpNF + "</tpNF>");

            if (_nFeContexto.Versao == NFeVersao.Versao310)
                _xmlString.Append("	<idDest>" + Ide.idDest + "</idDest>");

            _xmlString.Append("	<cMunFG>" + Ide.cMunFG + "</cMunFG>");
            _xmlString.Append("	<tpImp>" + Ide.tpImp + "</tpImp>");
            _xmlString.Append("	<tpEmis>" + Ide.tpEmis + "</tpEmis>");
            _xmlString.Append("	<cDV>" + Ide.cDV + "</cDV>");
            _xmlString.Append("	<tpAmb>" + Ide.tpAmb + "</tpAmb>");

            _xmlString.Append("	<finNFe>" + Ide.finNFe + "</finNFe>");

            if (_nFeContexto.Versao == NFeVersao.Versao310)
            {
                //XmlString.Append("	<indFinal>" + ide.indFinal + "</indFinal>");
                //XmlString.Append("	<indPres>" + ide.indPres + "</indPres>");
            }

            _xmlString.Append("	<procEmi>" + Ide.procEmi + "</procEmi>");
            _xmlString.Append("	<verProc>1</verProc>");
            _xmlString.Append("</ide>");
        }

        private void MontaEmit()
        {
            _xmlString.Append("<emit>");

            if (!string.IsNullOrEmpty(Emit.CPF))
                _xmlString.Append("	<CPF>" + Emit.CPF + "</CPF>");
            if (!string.IsNullOrEmpty(Emit.CNPJ))
                _xmlString.Append("	<CNPJ>" + Emit.CNPJ + "</CNPJ>");

            _xmlString.Append("	<xNome>" + Emit.xNome + "</xNome>");
            _xmlString.Append("	<enderEmit>");
            _xmlString.Append("		<xLgr>" + Emit.EnderEmit.xLgr + "</xLgr>");
            _xmlString.Append("		<nro>" + Emit.EnderEmit.nro + "</nro>");
            _xmlString.Append("		<xBairro>" + Emit.EnderEmit.xBairro + "</xBairro>");
            _xmlString.Append("		<cMun>" + Emit.EnderEmit.cMun + "</cMun>");
            _xmlString.Append("		<xMun>" + Emit.EnderEmit.xMun + "</xMun>");
            _xmlString.Append("		<UF>" + Emit.EnderEmit.UF.Trim() + "</UF>");
            _xmlString.Append("		<CEP>" + Emit.EnderEmit.CEP + "</CEP>");
            _xmlString.Append("		<cPais>1058</cPais>");
            _xmlString.Append("		<xPais>BRASIL</xPais>");

            if (!string.IsNullOrEmpty(Emit.EnderEmit.fone))
                _xmlString.Append("		<fone>" + Emit.EnderEmit.fone + "</fone>");

            _xmlString.Append("	</enderEmit>");

            _xmlString.Append("	<IE>" + Emit.IE + "</IE>");
            _xmlString.Append("	<CRT>" + Emit.CRT + "</CRT>");
            _xmlString.Append("</emit>");
        }

        private void MontaDest()
        {
            _xmlString.Append("<dest>");

            if (!string.IsNullOrEmpty(Dest.CPF))
                _xmlString.Append("	<CPF>" + Dest.CPF + "</CPF>");

            if (!string.IsNullOrEmpty(Dest.CNPJ))
                _xmlString.Append("	<CNPJ>" + Dest.CNPJ + "</CNPJ>");

            if (!string.IsNullOrEmpty(Dest.idEstrangeiro))
                _xmlString.Append("	<idEstrangeiro>" + Dest.idEstrangeiro + "</idEstrangeiro>");

            _xmlString.Append("	<xNome>" + Dest.xNome + "</xNome>");
            _xmlString.Append("	<enderDest>");
            _xmlString.Append("		<xLgr>" + Dest.EnderDest.xLgr + "</xLgr>");
            _xmlString.Append("		<nro>" + Dest.EnderDest.nro + "</nro>");

            if (!string.IsNullOrEmpty(Dest.EnderDest.xCpl))
                _xmlString.Append("		<xCpl>" + Dest.EnderDest.xCpl + "</xCpl>");

            _xmlString.Append("		<xBairro>" + Dest.EnderDest.xBairro + "</xBairro>");
            _xmlString.Append("		<cMun>" + Dest.EnderDest.cMun + "</cMun>");
            _xmlString.Append("		<xMun>" + Dest.EnderDest.xMun + "</xMun>");
            _xmlString.Append("		<UF>" + Dest.EnderDest.UF.Trim() + "</UF>");

            if (!string.IsNullOrEmpty(Dest.EnderDest.CEP))
                _xmlString.Append("		<CEP>" + Dest.EnderDest.CEP + "</CEP>");

            _xmlString.Append("		<cPais>1058</cPais>");
            _xmlString.Append("		<xPais>BRASIL</xPais>");

            if (!string.IsNullOrEmpty(Dest.fone))
                _xmlString.Append("		<fone>" + Dest.fone + "</fone>");

            _xmlString.Append("	</enderDest>");

            if (_nFeContexto.Versao == NFeVersao.Versao310)
                _xmlString.Append("	<indIEDest>" + Dest.indIEDest + "</indIEDest>");


            _xmlString.Append("	<IE>" + Dest.IE + "</IE>");


            if ((_nFeContexto.Versao == NFeVersao.Versao310) && !string.IsNullOrEmpty(Dest.email))
                _xmlString.Append("	<email>" + Dest.email + "</email>");

            _xmlString.Append("</dest>");
        }

        private void MontaDet()
        {
            for (var i = 0; i < DetList.Count; i++)
            {
                _xmlString.Append("<det nItem=\"" + (i + 1) + "\">");
                _xmlString.Append("	<prod>");
                _xmlString.Append("		<cProd>" + DetList[i].Prod.cProd.Trim() + "</cProd>");
                _xmlString.Append("		<cEAN>" + DetList[i].Prod.cEAN + "</cEAN>");
                _xmlString.Append("		<xProd>" + DetList[i].Prod.xProd + "</xProd>");
                _xmlString.Append("		<NCM>" + DetList[i].Prod.NCM + "</NCM>");
                _xmlString.Append("		<CFOP>" + DetList[i].Prod.CFOP + "</CFOP>");
                _xmlString.Append("		<uCom>" + DetList[i].Prod.uCom + "</uCom>");
                _xmlString.Append("		<qCom>" + DetList[i].Prod.qCom + "</qCom>");
                _xmlString.Append("		<vUnCom>" + DetList[i].Prod.vUnCom + "</vUnCom>");
                _xmlString.Append("		<vProd>" + DetList[i].Prod.vProd + "</vProd>");
                _xmlString.Append("		<cEANTrib>" + DetList[i].Prod.cEANTrib + "</cEANTrib>");
                _xmlString.Append("		<uTrib>" + DetList[i].Prod.uTrib + "</uTrib>");
                _xmlString.Append("		<qTrib>" + DetList[i].Prod.qTrib + "</qTrib>");
                _xmlString.Append("		<vUnTrib>" + DetList[i].Prod.vUnTrib + "</vUnTrib>");

                if (!string.IsNullOrEmpty(DetList[i].Prod.vFrete))
                    _xmlString.Append("		<vFrete>" + DetList[i].Prod.vFrete + "</vFrete>");
                if (!string.IsNullOrEmpty(DetList[i].Prod.vDesc))
                    _xmlString.Append("		<vDesc>" + DetList[i].Prod.vDesc + "</vDesc>");

                _xmlString.Append("		<indTot>" + DetList[i].Prod.indTot + "</indTot>");
                _xmlString.Append("	</prod>");

                _xmlString.Append("	<imposto>");

                if (!string.IsNullOrEmpty(DetList[i].Prod.vTotTrib))
                    _xmlString.Append("	<vTotTrib>" + DetList[i].Prod.vTotTrib + "</vTotTrib>");

                if (DetList[i].Imposto.Icms != null)
                    MontaDET_ICMS(DetList[i]);
                if (DetList[i].Imposto.Ipi != null)
                    MontaDET_IPI(DetList[i]);
                if (DetList[i].Imposto.Pis != null)
                    MontaDET_PIS(DetList[i]);
                if (DetList[i].Imposto.Cofins != null)
                    MontaDET_COFINS(DetList[i]);

                if (DetList[i].Imposto.Issqn != null)
                    MontaDET_ISQN(DetList[i]);
                _xmlString.Append("	</imposto>");

                _xmlString.Append("</det>");
            }
        }

        private void MontaDET_ICMS(Det det)
        {
            _xmlString.Append("<ICMS>");

            switch (det.icms)
            {
                case GetIcms.ICMS00:
                    _xmlString.Append("<ICMS00>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    _xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    _xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");
                    _xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    _xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    _xmlString.Append("</ICMS00>");
                    break;
                case GetIcms.ICMS10:
                    _xmlString.Append("<ICMS10>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    _xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    _xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");
                    _xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    _xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    _xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pMVAST))
                        _xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    _xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    _xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    _xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");

                    _xmlString.Append("</ICMS10>");
                    break;
                case GetIcms.ICMS20:
                    _xmlString.Append("<ICMS20>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    _xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    _xmlString.Append("    <pRedBC>" + det.Imposto.Icms.pRedBC + "</pRedBC>");
                    _xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");
                    _xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    _xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    _xmlString.Append("</ICMS20>");
                    break;
                case GetIcms.ICMS30:
                    _xmlString.Append("<ICMS30>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    _xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");
                    _xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");
                    _xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");
                    _xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    _xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    _xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");
                    _xmlString.Append("</ICMS30>");
                    break;
                case GetIcms.ICMS40_50:
                    _xmlString.Append("<ICMS40>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    _xmlString.Append("</ICMS40>");
                    break;
                case GetIcms.ICMS51:
                    _xmlString.Append("<ICMS51>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    _xmlString.Append("</ICMS51>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <pRedBC>" + det.Imposto.Icms.pRedBC + "</pRedBC>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    break;
                case GetIcms.ICMS60:
                    _xmlString.Append("<ICMS60>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    _xmlString.Append("</ICMS60>");
                    break;
                case GetIcms.ICMS70:
                    _xmlString.Append("<ICMS70>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    _xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    _xmlString.Append("    <pRedBC>" + det.Imposto.Icms.pRedBC + "</pRedBC>");
                    _xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");
                    _xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    _xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    _xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pMVAST))
                        _xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    _xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    _xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    _xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");
                    _xmlString.Append("</ICMS70>");
                    break;
                case GetIcms.ICMS90:
                    _xmlString.Append("<ICMS90>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CST>" + det.Imposto.Icms.CST + "</CST>");
                    _xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    _xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBC))
                        _xmlString.Append("    <pRedBC>" + det.Imposto.Icms.pRedBC + "</pRedBC>");

                    _xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    _xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    _xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pMVAST))
                        _xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    _xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    _xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    _xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");
                    _xmlString.Append("</ICMS90>");
                    break;
                case GetIcms.ICMS101:
                    _xmlString.Append("<ICMSSN101>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    _xmlString.Append("    <pCredSN>" + det.Imposto.Icms.pCredSN + "</pCredSN>");
                    _xmlString.Append("    <vCredICMSSN>" + det.Imposto.Icms.vCredICMSSN + "</vCredICMSSN>");
                    _xmlString.Append("</ICMSSN101>");
                    break;
                case GetIcms.ICMS102_400:
                    _xmlString.Append("<ICMSSN102>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    _xmlString.Append("</ICMSSN102>");
                    break;
                case GetIcms.ICMS201:
                    _xmlString.Append("<ICMSSN201>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    _xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    _xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    _xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    _xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");

                    _xmlString.Append("</ICMSSN201>");
                    break;
                case GetIcms.ICMS202:
                    _xmlString.Append("<ICMSSN202>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    _xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pMVAST))
                        _xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    _xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    _xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    _xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");

                    _xmlString.Append("</ICMSSN202>");
                    break;
                case GetIcms.ICMS500:
                    _xmlString.Append("<ICMSSN500>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    _xmlString.Append("    <vBCSTRet>" + det.Imposto.Icms.vBCSTRet + "</vBCSTRet>");
                    _xmlString.Append("    <vICMSSTRet>" + det.Imposto.Icms.vICMSSTRet + "</vICMSSTRet>");
                    _xmlString.Append("</ICMSSN500>");
                    break;
                case GetIcms.ICMS900:
                    _xmlString.Append("<ICMSSN900>");
                    _xmlString.Append("    <orig>" + det.Imposto.Icms.orig + "</orig>");
                    _xmlString.Append("    <CSOSN>" + det.Imposto.Icms.CSOSN + "</CSOSN>");
                    _xmlString.Append("    <modBC>" + det.Imposto.Icms.modBC + "</modBC>");
                    _xmlString.Append("    <vBC>" + det.Imposto.Icms.vBC + "</vBC>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBC))
                        _xmlString.Append("    <pRedBC>" + det.Imposto.Icms.pRedBC + "</pRedBC>");

                    _xmlString.Append("    <pICMS>" + det.Imposto.Icms.pICMS + "</pICMS>");
                    _xmlString.Append("    <vICMS>" + det.Imposto.Icms.vICMS + "</vICMS>");
                    _xmlString.Append("    <modBCST>" + det.Imposto.Icms.modBCST + "</modBCST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pMVAST))
                        _xmlString.Append("    <pMVAST>" + det.Imposto.Icms.pMVAST + "</pMVAST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Icms.pRedBCST))
                        _xmlString.Append("    <pRedBCST>" + det.Imposto.Icms.pRedBCST + "</pRedBCST>");

                    _xmlString.Append("    <vBCST>" + det.Imposto.Icms.vBCST + "</vBCST>");
                    _xmlString.Append("    <pICMSST>" + det.Imposto.Icms.pICMSST + "</pICMSST>");
                    _xmlString.Append("    <vICMSST>" + det.Imposto.Icms.vICMSST + "</vICMSST>");
                    _xmlString.Append("    <pCredSN>" + det.Imposto.Icms.pCredSN + "</pCredSN>");
                    _xmlString.Append("    <vCredICMSSN>" + det.Imposto.Icms.vCredICMSSN + "</vCredICMSSN>");
                    _xmlString.Append("</ICMSSN900>");
                    break;
            }

            _xmlString.Append("</ICMS>");
        }

        private void MontaDET_IPI(Det det)
        {
            if (!string.IsNullOrEmpty(det.Imposto.Ipi.cIEnq))
            {
                _xmlString.Append("<IPI>");

                _xmlString.Append("<cEnq>" + det.Imposto.Ipi.cIEnq + "</cEnq>");

                switch (det.ipi)
                {
                    case GetIpi.IPI00_49_50_99:
                        _xmlString.Append("<IPITrib>");

                        _xmlString.Append("    <CST>" + det.Imposto.Ipi.CST + "</CST>");

                        if (!string.IsNullOrEmpty(det.Imposto.Ipi.vBC))
                        {
                            _xmlString.Append("    <vBC>" + det.Imposto.Ipi.vBC + "</vBC>");
                            _xmlString.Append("    <pIPI>" + det.Imposto.Ipi.pIPI + "</pIPI>");
                        }
                        else
                        {
                            _xmlString.Append("    <qUnid>" + det.Imposto.Ipi.qUnid + "</qUnid>");
                            _xmlString.Append("    <vUnid>" + det.Imposto.Ipi.vUnid + "</vUnid>");
                        }

                        _xmlString.Append("    <vIPI>" + det.Imposto.Ipi.vIPI + "</vIPI>");

                        _xmlString.Append("</IPITrib>");
                        break;
                    case GetIpi.IPI01_55:
                        _xmlString.Append("<IPINT>");
                        _xmlString.Append("    <CST>" + det.Imposto.Ipi.CST + "</CST>");
                        _xmlString.Append("</IPINT>");
                        break;
                }

                _xmlString.Append("</IPI>");
            }
        }

        private void MontaDET_PIS(Det det)
        {
            _xmlString.Append("<PIS>");

            switch (det.pis)
            {
                case GetPis.PIS01_02:
                    _xmlString.Append("<PISAliq>");
                    _xmlString.Append("    <CST>" + det.Imposto.Pis.CST + "</CST>");
                    _xmlString.Append("    <vBC>" + det.Imposto.Pis.vBC + "</vBC>");
                    _xmlString.Append("    <pPIS>" + det.Imposto.Pis.pPIS + "</pPIS>");
                    _xmlString.Append("    <vPIS>" + det.Imposto.Pis.vPIS + "</vPIS>");
                    _xmlString.Append("</PISAliq>");
                    break;
                case GetPis.PIS03:
                    _xmlString.Append("<PISQtde>");
                    _xmlString.Append("    <CST>" + det.Imposto.Pis.CST + "</CST>");
                    _xmlString.Append("    <qBCProd>" + det.Imposto.Pis.qBCProd + "</qBCProd>");
                    _xmlString.Append("    <vAliqProd>" + det.Imposto.Pis.vAliqProd + "</vAliqProd>");
                    _xmlString.Append("    <vPIS>" + det.Imposto.Pis.vPIS + "</vPIS>");
                    _xmlString.Append("</PISQtde>");
                    break;
                case GetPis.PIS04_09:
                    _xmlString.Append("<PISNT>");
                    _xmlString.Append("    <CST>" + det.Imposto.Pis.CST + "</CST>");
                    _xmlString.Append("</PISNT>");
                    break;
                case GetPis.PIS99:
                    _xmlString.Append("<PISOutr>");
                    _xmlString.Append("    <CST>" + det.Imposto.Pis.CST + "</CST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Pis.vBC) && !string.IsNullOrEmpty(det.Imposto.Pis.pPIS))
                    {
                        _xmlString.Append("    <vBC>" + det.Imposto.Pis.vBC + "</vBC>");
                        _xmlString.Append("    <pPIS>" + det.Imposto.Pis.pPIS + "</pPIS>");
                    }
                    else
                    {
                        _xmlString.Append("    <qBCProd>" + det.Imposto.Pis.qBCProd + "</qBCProd>");
                        _xmlString.Append("    <vAliqProd>" + det.Imposto.Pis.vAliqProd + "</vAliqProd>");
                    }

                    _xmlString.Append("    <vPIS>" + det.Imposto.Pis.vPIS + "</vPIS>");
                    _xmlString.Append("</PISOutr>");
                    break;
            }

            _xmlString.Append("</PIS>");
        }

        private void MontaDET_COFINS(Det det)
        {
            _xmlString.Append("<COFINS>");

            switch (det.cofins)
            {
                case GetCofins.CST01_02:
                    _xmlString.Append("<COFINSAliq>");
                    _xmlString.Append("    <CST>" + det.Imposto.Cofins.CST + "</CST>");
                    _xmlString.Append("    <vBC>" + det.Imposto.Cofins.vBC + "</vBC>");
                    _xmlString.Append("    <pCOFINS>" + det.Imposto.Cofins.pCOFINS + "</pCOFINS>");
                    _xmlString.Append("    <vCOFINS>" + det.Imposto.Cofins.vCOFINS + "</vCOFINS>");
                    _xmlString.Append("</COFINSAliq>");
                    break;
                case GetCofins.CST03:
                    _xmlString.Append("<COFINSQtde>");
                    _xmlString.Append("    <CST>" + det.Imposto.Cofins.CST + "</CST>");
                    _xmlString.Append("    <qBCProd>" + det.Imposto.Cofins.qBCProd + "</qBCProd>");
                    _xmlString.Append("    <vAliqProd>" + det.Imposto.Cofins.vAliqProd + "</vAliqProd>");
                    _xmlString.Append("    <vCOFINS>" + det.Imposto.Cofins.vCOFINS + "</vCOFINS>");
                    _xmlString.Append("</COFINSQtde>");
                    break;
                case GetCofins.CST04_09:
                    _xmlString.Append("<COFINSNT>");
                    _xmlString.Append("    <CST>" + det.Imposto.Cofins.CST + "</CST>");
                    _xmlString.Append("</COFINSNT>");
                    break;
                case GetCofins.CST99:
                    _xmlString.Append("<COFINSOutr>");
                    _xmlString.Append("    <CST>" + det.Imposto.Cofins.CST + "</CST>");

                    if (!string.IsNullOrEmpty(det.Imposto.Cofins.vBC) &&
                        !string.IsNullOrEmpty(det.Imposto.Cofins.pCOFINS))
                    {
                        _xmlString.Append("    <vBC>" + det.Imposto.Cofins.vBC + "</vBC>");
                        _xmlString.Append("    <pCOFINS>" + det.Imposto.Cofins.pCOFINS + "</pCOFINS>");
                    }
                    else
                    {
                        _xmlString.Append("    <qBCProd>" + det.Imposto.Cofins.qBCProd + "</qBCProd>");
                        _xmlString.Append("    <vAliqProd>" + det.Imposto.Cofins.vAliqProd + "</vAliqProd>");
                    }

                    _xmlString.Append("    <vCOFINS>" + det.Imposto.Cofins.vCOFINS + "</vCOFINS>");
                    _xmlString.Append("</COFINSOutr>");
                    break;
            }

            _xmlString.Append("</COFINS>");
        }

        private void MontaDET_ISQN(Det det)
        {
            _xmlString.Append("<ISSQN>");
            _xmlString.Append("    <vBC>" + det.Imposto.Issqn.vBC + "</vBC>");
            _xmlString.Append("    <vAliq>" + det.Imposto.Issqn.vAliq + "</vAliq>");
            _xmlString.Append("    <vISSQN>" + det.Imposto.Issqn.vISSQN + "</vISSQN>");
            _xmlString.Append("    <cMunFG>" + det.Imposto.Issqn.cMunFG + "</cMunFG>");
            _xmlString.Append("    <cListServ>" + det.Imposto.Issqn.cListServ + "</cListServ>");
            _xmlString.Append("    <indISS>" + det.Imposto.Issqn.indISS + "</indISS>");
            _xmlString.Append("    <indIncentivo>" + det.Imposto.Issqn.indIncentivo + "</indIncentivo>");
            _xmlString.Append("</ISSQN>");
        }

        private void MontaTotal()
        {
            _xmlString.Append("<total>");
            if (Total.IcmsTotal != null)
            {
                _xmlString.Append("	<ICMSTot>");
                _xmlString.Append("		<vBC>" + Total.IcmsTotal.vBC + "</vBC>");
                _xmlString.Append("		<vICMS>" + Total.IcmsTotal.vICMS + "</vICMS>");
                if (_nFeContexto.Versao == NFeVersao.Versao310)
                    _xmlString.Append("		<vICMSDeson>" + Total.IcmsTotal.vICMSDeson + "</vICMSDeson>");
                _xmlString.Append("		<vBCST>" + Total.IcmsTotal.vBCST + "</vBCST>");
                _xmlString.Append("		<vST>" + Total.IcmsTotal.vST + "</vST>");
                _xmlString.Append("		<vProd>" + Total.IcmsTotal.vProd + "</vProd>");
                _xmlString.Append("		<vFrete>" + Total.IcmsTotal.vFrete + "</vFrete>");
                _xmlString.Append("		<vSeg>" + Total.IcmsTotal.vSeg + "</vSeg>");
                _xmlString.Append("		<vDesc>" + Total.IcmsTotal.vDesc + "</vDesc>");
                _xmlString.Append("		<vII>0.00</vII>");
                _xmlString.Append("		<vIPI>" + Total.IcmsTotal.vIPI + "</vIPI>");
                _xmlString.Append("		<vPIS>0.00</vPIS>");
                _xmlString.Append("		<vCOFINS>0.00</vCOFINS>");
                _xmlString.Append("		<vOutro>" + Total.IcmsTotal.vOutro + "</vOutro>");
                _xmlString.Append("		<vNF>" + Total.IcmsTotal.vNF + "</vNF>");

                if (!string.IsNullOrEmpty(Total.IcmsTotal.vTotTrib))
                    _xmlString.Append("		<vTotTrib>" + Total.IcmsTotal.vTotTrib + "</vTotTrib>");

                _xmlString.Append("	</ICMSTot>");
            }

            if (Total.IssqnTot != null)
            {
                _xmlString.Append("	<ISSQNtot>");
                _xmlString.Append("		<vServ>" + Total.IssqnTot.vServ + "</vServ>");
                _xmlString.Append("		<vBC>" + Total.IssqnTot.vBC + "</vBC>");
                _xmlString.Append("		<dCompet>" + Total.IssqnTot.dCompet + "</dCompet>");
                _xmlString.Append("		<cRegTrib>" + Total.IssqnTot.cRegTrib + "</cRegTrib>");
                _xmlString.Append("	</ISSQNtot>");
            }
            _xmlString.Append("</total>");
        }

        private void MontaTransp()
        {
            _xmlString.Append("<transp>");
            _xmlString.Append("	<modFrete>" + Transp.modFrete + "</modFrete>");

            if (!string.IsNullOrEmpty(Transp.CNPJ))
            {
                _xmlString.Append("	<transporta>");

                if (!string.IsNullOrEmpty(Transp.CNPJ))
                    _xmlString.Append("		<CNPJ>" + Transp.CNPJ + "</CNPJ>");

                /*
                if(!String.IsNullOrEmpty(this.transp.CPF))
                    this.XmlString.Append("		<CPF>" + this.transp.CPF + "</CPF>");
                */

                if (!string.IsNullOrEmpty(Transp.xNome))
                    _xmlString.Append("		<xNome>" + Transp.xNome + "</xNome>");

                if (!string.IsNullOrEmpty(Transp.IE))
                    _xmlString.Append("		<IE>" + Transp.IE + "</IE>");

                if (!string.IsNullOrEmpty(Transp.xEnder))
                    _xmlString.Append("		<xEnder>" + Transp.xEnder + "</xEnder>");

                if (!string.IsNullOrEmpty(Transp.xMun))
                    _xmlString.Append("		<xMun>" + Transp.xMun + "</xMun>");

                if (!string.IsNullOrEmpty(Transp.UF))
                    _xmlString.Append("		<UF>" + Transp.UF.Trim() + "</UF>");

                _xmlString.Append("	</transporta>");
            }
            if (!string.IsNullOrEmpty(Transp.veic_placa))
            {
                _xmlString.Append("	<veicTransp>");
                _xmlString.Append("		<placa>" + Transp.veic_placa + "</placa>");
                _xmlString.Append("		<UF>" + Transp.veic_UF + "</UF>");
                _xmlString.Append("	</veicTransp>");
            }

            if (!string.IsNullOrEmpty(Transp.Vol?.qVol))
            {
                _xmlString.Append("	<vol>");
                _xmlString.Append("		<qVol>" + Transp.Vol.qVol + "</qVol>");
                _xmlString.Append("		<esp>" + Transp.Vol.esp + "</esp>");


                if (!string.IsNullOrEmpty(Transp.Vol.marca))
                    _xmlString.Append("		<marca>" + Transp.Vol.marca + "</marca>");

                if (!string.IsNullOrEmpty(Transp.Vol.nVol))
                    _xmlString.Append("		<nVol>" + Transp.Vol.nVol + "</nVol>");

                if (!string.IsNullOrEmpty(Transp.Vol.pesoL))
                    _xmlString.Append("		<pesoL>" + Transp.Vol.pesoL + "</pesoL>");

                if (!string.IsNullOrEmpty(Transp.Vol.pesoB))
                    _xmlString.Append("		<pesoB>" + Transp.Vol.pesoB + "</pesoB>");

                _xmlString.Append("	</vol>");
            }
            _xmlString.Append("</transp>");
        }

        private void MontaCobr()
        {
            if (!string.IsNullOrEmpty(Cobr.Fat?.nFat))
            {
                _xmlString.Append("<cobr>");
                _xmlString.Append("	<fat>");
                _xmlString.Append("		<nFat>" + Cobr.Fat.nFat + "</nFat>");
                _xmlString.Append("		<vOrig>" + Cobr.Fat.vOrig + "</vOrig>");
                _xmlString.Append("		<vLiq>" + Cobr.Fat.vLiq + "</vLiq>");
                _xmlString.Append("	</fat>");

                foreach (DUP t in Cobr.dup)
                {
                    _xmlString.Append("	<dup>");
                    _xmlString.Append("		<nDup>" + t.nDup + "</nDup>");
                    _xmlString.Append("		<dVenc>" + t.dVenc + "</dVenc>");
                    _xmlString.Append("		<vDup>" + t.vDup + "</vDup>");
                    _xmlString.Append("	</dup>");
                }

                _xmlString.Append("</cobr>");
            }
        }
    }
}
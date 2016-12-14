using System;
using System.Security.Cryptography.X509Certificates;
using NFeEletronica.Certificado;
using NFeEletronica.Versao;

namespace NFeEletronica.Contexto
{
    public class NFeContexto : INFeContexto
    {
        public string Uf { get; }
        public bool Producao { get; }
        public BaseVersao Versao { get; }
        public X509Certificate2 Certificado { get; }
        
        public NFeContexto(bool producao, NFeVersao versao, string uf, IGerenciadorDeCertificado gerenciadorDeCertificado = null)
        {
            if (versao == NFeVersao.Versao310)
            {
                Versao = new Versao310();
            }
            else
            {
                Versao = new Versao200();
            }

            Producao = producao;
            Uf = uf;
            //Abre uma janela para selecionar o certificado instalado no computador
            if (gerenciadorDeCertificado == null) gerenciadorDeCertificado = new GerenciadorDeCertificado();
            Certificado = gerenciadorDeCertificado.SelecionarPorWindows();

            if (Certificado == null)
                throw new Exception(
                    "Nenhum certificado encontrado, não será possível prosseguir com a Nota Fiscal Eletrônica.");
        }
    }
}

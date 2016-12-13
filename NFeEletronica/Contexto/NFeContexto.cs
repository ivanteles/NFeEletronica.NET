﻿using System;
using System.Security.Cryptography.X509Certificates;
using NFeEletronica.Certificado;
using NFeEletronica.Versao;

namespace NFeEletronica.Contexto
{
    public class NFeContexto : INFeContexto
    {
        public bool Producao { get; } = false;
        public BaseVersao Versao { get; }
        public X509Certificate2 Certificado { get; }
        
        public NFeContexto(bool producao, NFeVersao versao, IGerenciadorDeCertificado gerenciadorDeCertificado = null)
        {
            if (versao == NFeVersao.Versao310)
            {
                this.Versao = new Versao310();
            }
            else
            {
                this.Versao = new Versao200();
            }

            //Abre uma janela para selecionar o certificado instalado no computador
            if (gerenciadorDeCertificado == null) gerenciadorDeCertificado = new GerenciadorDeCertificado();
            this.Certificado = gerenciadorDeCertificado.SelecionarPorWindows();

            if (this.Certificado == null)
                throw new Exception(
                    "Nenhum certificado encontrado, não será possível prosseguir com a Nota Fiscal Eletrônica.");
        }
    }
}

using System.Security.Cryptography.X509Certificates;
using NFeEletronica.Versao;

namespace NFeEletronica.Contexto
{
    public interface INFeContexto
    {
        string Uf { get; }
        bool Producao { get; }
        BaseVersao Versao { get; }
        X509Certificate2 Certificado { get; }
    }
}
#region

using System;

#endregion

namespace NFeEletronica.Consulta
{
    public class CartaCorrecao
    {
        public CartaCorrecao(string numeroLote, string notaChaveAcesso, string correcao, string cnpj, string codigoUf)
        {
            NumeroLote = numeroLote;
            NotaChaveAcesso = notaChaveAcesso;
            Correcao = correcao;
            Cnpj = cnpj;
            CodigoUf = codigoUf;
        }

        public string NumeroLote { get; private set; }
        public string NotaChaveAcesso { get; private set; }
        public string Correcao { get; private set; }
        public string Cnpj { get; private set; }
        public string CodigoUf { get; private set; }
        public DateTime DataEvento { get; set; }
    }
}
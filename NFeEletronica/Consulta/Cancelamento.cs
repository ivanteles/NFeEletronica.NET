#region

using System;

#endregion

namespace NFeEletronica.Consulta
{
    public class Cancelamento
    {
        public Cancelamento(string numeroLote, string notaChaveAcesso, string justificativa, string protocolo,
            string cnpj)
        {
            NumeroLote = numeroLote;
            NotaChaveAcesso = notaChaveAcesso;
            Justificativa = justificativa;
            Protocolo = protocolo;
            Cnpj = cnpj;
        }

        public string NumeroLote { get; private set; }
        public string NotaChaveAcesso { get; private set; }
        public string Justificativa { get; private set; }
        public string Protocolo { get; private set; }
        public string Cnpj { get; private set; }
        public DateTime DataEvento { get; set; }
    }
}
namespace NFeEletronica.Retorno
{
    public class RetRecepcao : IRetorno
    {
        public RetRecepcao(string numeroNota, string protocolo, string status = "", string motivo = "")
        {
            NumeroNota = numeroNota;
            Protocolo = protocolo;
            Status = status;
            Motivo = motivo;
        }

        public string NumeroNota { get; private set; }
        public string Protocolo { get; private set; }
        public string Status { get; }
        public string Motivo { get; }
    }
}
namespace NFeEletronica.Retorno
{
    public class Recepcao : IRetorno
    {
        public Recepcao(string recibo, string status, string motivo)
        {
            Recibo = recibo;
            Status = status;
            Motivo = motivo;
        }

        public string Recibo { get; private set; }
        public string Status { get; }
        public string Motivo { get; }
    }
}
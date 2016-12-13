namespace NFeEletronica.Retorno
{
    public class RetornoSimples : IRetorno
    {
        public RetornoSimples(string status, string motivo)
        {
            Status = status;
            Motivo = motivo;
        }

        public string Status { get; }
        public string Motivo { get; }
    }
}
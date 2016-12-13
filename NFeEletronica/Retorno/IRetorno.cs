namespace NFeEletronica.Retorno
{
    public interface IRetorno
    {
        string Status { get; }
        string Motivo { get; }
    }
}
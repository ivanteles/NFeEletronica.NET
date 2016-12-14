namespace NFeEletronica.Consulta
{
    public class Inutilizacao
    {
        public Inutilizacao(string justificativa, string cnpj, string uf)
        {
            Justificativa = justificativa;
            Cnpj = cnpj;
            Uf = uf;
        }

        public string Justificativa { get; private set; }
        public string Cnpj { get; private set; }
        public string Uf { get; private set; }
        public string Ano { get; set; }
        public string Mod { get; set; }
        public string Serie { get; set; }
        public string NumeroNfeInicial { get; set; }
        public string NumeroNfeFinal { get; set; }
    }
}
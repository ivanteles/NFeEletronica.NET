using System;

namespace NFeEletronica.Versao
{
    public abstract class BaseVersao
    {
        protected bool Equals(BaseVersao other)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BaseVersao) obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public abstract NFeVersao Versao { get; }
        public abstract String VersaoString { get; }
        public abstract String PastaXml { get; }

        public static bool operator ==(BaseVersao versaoAbstract, NFeVersao nfeVersao)
        {
            return versaoAbstract != null && (versaoAbstract.Versao == nfeVersao);
        }

        public static bool operator !=(BaseVersao versaoAbstract, NFeVersao nfeVersao)
        {
            return versaoAbstract != null && (versaoAbstract.Versao != nfeVersao);
        }

        public bool Equals(NFeVersao obj)
        {
            return (obj == Versao);
        }
    }
}
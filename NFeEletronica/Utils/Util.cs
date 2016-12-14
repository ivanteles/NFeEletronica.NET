#region

using System.Configuration;
using System.IO;
using System.Web.Hosting;

#endregion

namespace NFeEletronica.Utils
{
    public class Util
    {
        public static string ContentFolderSchemaValidacao
        {
            get
            {
                if (ConfigurationManager.AppSettings["NFeEletronica.Pasta.SchemaValidacao"] != null)
                    return ConfigurationManager.AppSettings["NFeEletronica.Pasta.SchemaValidacao"];
                var caminho = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "NFeSchemas");
                return caminho;
            }
        }

        public static string GerarModulo11(string chaveAcesso)
        {
            var total = 0;
            var multiplier = 2;

            for (var i = chaveAcesso.Length - 1; i >= 0; i--)
            {
                if (9 < multiplier)
                    multiplier = 2;

                var digit = int.Parse(chaveAcesso.Substring(i, 1));
                total += digit*multiplier++;
            }

            var remainder = total%11;

            if ((0 == remainder) || (1 == remainder))
                return "0";

            return (11 - remainder).ToString();
        }
    }
}
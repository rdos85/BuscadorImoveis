using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Util
{
    public static class HtmlStringExtensions_
    {
        public static string LimparString(this string textoHtml)
        {
            var partes = textoHtml.Split("\n").ToList();
            var partesAjustadas = new string[partes.Count];
            for (int i = 0; i < partes.Count; i++)
                partesAjustadas[i] = partes[i].Trim().Replace("\n", "");

            return string.Join(" ", partesAjustadas.Where(p => p != ""));
        }
    }
}

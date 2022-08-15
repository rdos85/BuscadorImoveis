using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Servicos
{
    public class AvaliacaoRequest
    {
        public AvaliacaoRequest(string tiposImoveis, string urlBusca)
        {
            TiposImoveis = tiposImoveis;
            UrlBusca = urlBusca;
        }

        public string TiposImoveis { get; set; }
        public string UrlBusca { get; set; }
    }
}

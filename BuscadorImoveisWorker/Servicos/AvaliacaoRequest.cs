using BuscadorImoveisWorker.Buscadores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Servicos
{
    public class AvaliacaoRequest
    {
        public AvaliacaoRequest(IBuscadorImoveis buscadorImoveis, string tiposImoveis, string urlBusca)
        {
            BuscadorImoveis = buscadorImoveis;
            TiposImoveis = tiposImoveis;
            UrlBusca = urlBusca;
        }

        public IBuscadorImoveis BuscadorImoveis { get; set; }
        public string TiposImoveis { get; set; }
        public string UrlBusca { get; set; }
    }
}

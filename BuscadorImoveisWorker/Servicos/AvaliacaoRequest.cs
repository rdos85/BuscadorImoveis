using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Servicos
{
    public class AvaliacaoRequest
    {
        public AvaliacaoRequest(string tiposImoveis, IAvaliadorImoveis avaliadorImoveis, string urlBusca)
        {
            TiposImoveis = tiposImoveis;
            AvaliadorImoveis = avaliadorImoveis;
            UrlBusca = urlBusca;
        }

        public string TiposImoveis { get; set; }
        public IAvaliadorImoveis AvaliadorImoveis { get; set; }
        public string UrlBusca { get; set; }
    }
}

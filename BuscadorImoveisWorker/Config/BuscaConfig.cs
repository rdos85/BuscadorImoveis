using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Config
{
    public class BuscaConfig
    {
        public string IdBusca { get; set; }
        public string TipoBuscadorImoveis { get; set; }
        public string UrlPesquisa { get; set; }
        public bool Ativo { get; set; }

    }
}

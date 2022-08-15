using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Servicos
{
    public interface IAvaliadorImoveis
    {
        Task<int> ExecutarAsync(AvaliacaoRequest avaliacaoRequest);
    }
}

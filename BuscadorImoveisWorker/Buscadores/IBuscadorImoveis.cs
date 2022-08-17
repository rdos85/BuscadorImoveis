using BuscadorImoveisWorker.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Buscadores
{
    public interface IBuscadorImoveis
    {
        string Origem { get; }
        Task<IList<Imovel>> BuscarImoveisAsync(string tipoBusca, string url);
    }
}

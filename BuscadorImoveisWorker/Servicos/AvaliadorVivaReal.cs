using BuscadorImoveisWorker.Buscadores;
using BuscadorImoveisWorker.Entidades;
using BuscadorImoveisWorker.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Servicos
{
    public class AvaliadorVivaReal : IAvaliadorImoveis
    {
        private readonly BuscadorVivaReal buscadorVivaReal;
        private readonly ImoveisDbContext imoveisDbContext;
        private readonly NotificadorTelegram notificadorTelegram;

        public AvaliadorVivaReal(BuscadorVivaReal buscadorVivaReal, ImoveisDbContext imoveisDbContext, NotificadorTelegram notificadorTelegram)
        {
            this.buscadorVivaReal = buscadorVivaReal;
            this.imoveisDbContext = imoveisDbContext;
            this.notificadorTelegram = notificadorTelegram;
        }

        public async Task<int> ExecutarAsync(string tipoBusca, string urlBusca)
        {
            var imoveisEncontrados = await buscadorVivaReal.BuscarImoveisAsync(tipoBusca, urlBusca);

            var novidades = new List<ImovelVivaReal>();
            foreach (var imovelEncontrado in imoveisEncontrados)
            {
                var dadosImovelAtual = imoveisDbContext.ImovelVivaReal.Where(i => i.Id == imovelEncontrado.Id).FirstOrDefault();
                if (dadosImovelAtual == null)
                {
                    imoveisDbContext.ImovelVivaReal.Add(imovelEncontrado);
                    novidades.Add(imovelEncontrado);
                }
                else if (MudouPreco(dadosImovelAtual, imovelEncontrado))
                {
                    novidades.Add(imovelEncontrado);
                    dadosImovelAtual.ValorAluguel = imovelEncontrado.ValorAluguel;
                    dadosImovelAtual.ValorCondominio = imovelEncontrado.ValorCondominio;
                }

                imoveisDbContext.SaveChanges();
            }

            await NotificarNovidadesAsync(tipoBusca, novidades);

            return novidades.Count();
        }

        private bool MudouPreco(ImovelVivaReal valorAnterior, ImovelVivaReal valorAtual)
        {
            if (valorAnterior.ValorAluguel != valorAtual.ValorAluguel ||
                valorAnterior.ValorCondominio != valorAtual.ValorCondominio)
                return true;

            return false;
        }

        private async Task NotificarNovidadesAsync(string tipoImoveis, IEnumerable<IImovel> novidadesNetImoveis)
        {
            Console.WriteLine($"Foram encontrados [{novidadesNetImoveis.Count()}] novos imóveis em [{tipoImoveis}]");

            foreach (var item in novidadesNetImoveis)
                Console.WriteLine($"{item.Titulo} - {item.Endereco} | {item.ValorAluguel} {item.ValorCondominio}");

            await notificadorTelegram.NotificarGrupoNovidades(tipoImoveis, novidadesNetImoveis);
        }
    }
}

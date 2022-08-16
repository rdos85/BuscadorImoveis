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
    public class AvaliadorCasaMineira : IAvaliadorImoveis
    {
        private readonly BuscadorCasaMineira buscadorImoveis;
        private readonly ImoveisDbContext imoveisDbContext;
        private readonly NotificadorTelegram notificadorTelegram;

        public AvaliadorCasaMineira(BuscadorCasaMineira buscadorImoveis, ImoveisDbContext imoveisDbContext, NotificadorTelegram notificadorTelegram)
        {
            this.buscadorImoveis = buscadorImoveis;
            this.imoveisDbContext = imoveisDbContext;
            this.notificadorTelegram = notificadorTelegram;
        }

        public async Task<int> ExecutarAsync(string tipoBusca, string urlBusca)
        {
            var imoveisEncontrados = await buscadorImoveis.BuscarImoveisAsync(tipoBusca, urlBusca);
            
            var novidades = new List<ImovelCasaMineira>();
            foreach (var imovelEncontrado in imoveisEncontrados)
            {
                var databaseCriado = imoveisDbContext.Database.EnsureCreated();

                var dadosImovelAtual = imoveisDbContext.ImovelCasaMineira.Where(i => i.Id == imovelEncontrado.Id).FirstOrDefault();
                if (dadosImovelAtual == null)
                {
                    imoveisDbContext.ImovelCasaMineira.Add(imovelEncontrado);
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

        private bool MudouPreco(ImovelCasaMineira valorAnterior, ImovelCasaMineira valorAtual)
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

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
    public class AvaliadorImoveis
    {
        private readonly ImoveisDbContext imoveisDbContext;
        private readonly NotificadorTelegram notificadorTelegram;

        public AvaliadorImoveis(ImoveisDbContext imoveisDbContext, NotificadorTelegram notificadorTelegram)
        {
            this.imoveisDbContext = imoveisDbContext;
            this.notificadorTelegram = notificadorTelegram;
        }

        public async Task<int> ExecutarAsync(AvaliacaoRequest avaliacaoRequest)
        {
            var imoveisEncontrados = await avaliacaoRequest.BuscadorImoveis.BuscarImoveisAsync(avaliacaoRequest.TiposImoveis, avaliacaoRequest.UrlBusca);

            var novidades = new List<Imovel>();
            foreach (var imovelEncontrado in imoveisEncontrados)
            {
                var dadosImovelAtual = imoveisDbContext.Imovel.Where(i => i.Id == imovelEncontrado.Id && i.Origem == imovelEncontrado.Origem).FirstOrDefault();
                if (dadosImovelAtual == null)
                {
                    imovelEncontrado.DataInclusao = DateTime.Now;
                    imoveisDbContext.Imovel.Add(imovelEncontrado);
                    novidades.Add(imovelEncontrado);
                }
                else if (MudouPreco(dadosImovelAtual, imovelEncontrado))
                {
                    dadosImovelAtual.DataModificacao = DateTime.Now;
                    dadosImovelAtual.ValorAluguel = imovelEncontrado.ValorAluguel;
                    dadosImovelAtual.ValorCondominio = imovelEncontrado.ValorCondominio;
                    novidades.Add(imovelEncontrado);
                }

                imoveisDbContext.SaveChanges();
            }

            await NotificarNovidadesAsync(avaliacaoRequest.TiposImoveis, novidades);

            return novidades.Count();
        }

        private bool MudouPreco(Imovel valorAnterior, Imovel valorAtual)
        {
            if (valorAnterior.ValorAluguel != valorAtual.ValorAluguel ||
                valorAnterior.ValorCondominio != valorAtual.ValorCondominio)
                return true;

            return false;
        }

        private async Task NotificarNovidadesAsync(string tipoImoveis, IEnumerable<Imovel> novidadesNetImoveis)
        {
            Console.WriteLine($"Foram encontrados [{novidadesNetImoveis.Count()}] novos imóveis em [{tipoImoveis}]");

            foreach (var item in novidadesNetImoveis)
                Console.WriteLine($"{item.Titulo} - {item.Endereco} | {item.ValorAluguel} {item.ValorCondominio}");

            await notificadorTelegram.NotificarGrupoNovidadesAsync(tipoImoveis, novidadesNetImoveis);
        }
    }
}

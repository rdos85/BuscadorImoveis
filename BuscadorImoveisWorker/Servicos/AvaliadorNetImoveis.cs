﻿using BuscadorImoveisWorker.Buscadores;
using BuscadorImoveisWorker.Entidades;
using BuscadorImoveisWorker.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Servicos
{
    public class AvaliadorNetImoveis : IAvaliadorImoveis
    {
        private readonly BuscadorNetImoveis buscadorNetImoveis;
        private readonly ImoveisDbContext imoveisDbContext;
        private readonly NotificadorTelegram notificadorTelegram;

        public AvaliadorNetImoveis(BuscadorNetImoveis buscadorNetImoveis, ImoveisDbContext imoveisDbContext, NotificadorTelegram notificadorTelegram)
        {
            this.buscadorNetImoveis = buscadorNetImoveis;
            this.imoveisDbContext = imoveisDbContext;
            this.notificadorTelegram = notificadorTelegram;
        }

        public async Task<int> ExecutarAsync(AvaliacaoRequest avaliacaoRequest)
        {
            var imoveisEncontrados = await buscadorNetImoveis.BuscarImoveisAsync(avaliacaoRequest.TiposImoveis, avaliacaoRequest.UrlBusca);

            var novidades = new List<ImovelNetImoveis>();
            foreach (var imovelEncontrado in imoveisEncontrados)
            {
                var dadosImovelAtual = imoveisDbContext.ImovelNetImoveis.Where(i => i.Id == imovelEncontrado.Id).FirstOrDefault();
                if (dadosImovelAtual == null)
                {
                    imoveisDbContext.ImovelNetImoveis.Add(imovelEncontrado);
                    novidades.Add(imovelEncontrado);
                }
                else if (MudouPreco(dadosImovelAtual, imovelEncontrado))
                {
                    novidades.Add(imovelEncontrado);
                    dadosImovelAtual = imovelEncontrado;
                }

                imoveisDbContext.SaveChanges();
            }

            await NotificarNovidadesAsync(avaliacaoRequest.TiposImoveis, novidades);

            return novidades.Count();
        }

        private bool MudouPreco(ImovelNetImoveis valorAnterior, ImovelNetImoveis valorAtual)
        {
            if (valorAnterior.ValorAluguel != valorAtual.ValorAluguel ||
                valorAnterior.ValorCondominio != valorAtual.ValorCondominio)
                return true;

            return false;
        }

        private async Task NotificarNovidadesAsync(string tipoImoveis, IEnumerable<IImovel> novidadesNetImoveis)
        {
            Console.WriteLine($"Foram encontrados [{novidadesNetImoveis.Count()}] novos imóveis em [{BuscadorNetImoveis.Origem}]");

            foreach (var item in novidadesNetImoveis)
                Console.WriteLine($"{item.Titulo} - {item.Endereco} | {item.ValorAluguel} {item.ValorCondominio}");

            await notificadorTelegram.NotificarGrupoNovidades(tipoImoveis, novidadesNetImoveis);
        }
    }
}

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
    public class AvaliadorZapImoveis : IAvaliadorImoveis
    {
        private readonly BuscadorZapImoveis buscadorZapImoveis;
        private readonly ImoveisDbContext imoveisDbContext;
        private readonly NotificadorTelegram notificadorTelegram;

        public AvaliadorZapImoveis(BuscadorZapImoveis buscadorZapImoveis, ImoveisDbContext imoveisDbContext, NotificadorTelegram notificadorTelegram)
        {
            this.buscadorZapImoveis = buscadorZapImoveis;
            this.imoveisDbContext = imoveisDbContext;
            this.notificadorTelegram = notificadorTelegram;
        }

        public async Task<int> ExecutarAsync(string tipoBusca, string urlBusca)
        {
            var imoveisEncontrados = await buscadorZapImoveis.BuscarImoveisAsync(tipoBusca, urlBusca);

            var novidades = new List<ImovelZapImoveis>();
            foreach (var imovelEncontrado in imoveisEncontrados)
            {
                var dadosImovelAtual = imoveisDbContext.ImovelZapImoveis.Where(i => i.Id == imovelEncontrado.Id).FirstOrDefault();
                if (dadosImovelAtual == null)
                {
                    imoveisDbContext.ImovelZapImoveis.Add(imovelEncontrado);
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

        private bool MudouPreco(ImovelZapImoveis valorAnterior, ImovelZapImoveis valorAtual)
        {
            if (valorAnterior.ValorAluguel != valorAtual.ValorAluguel ||
                valorAnterior.ValorCondominio != valorAtual.ValorCondominio)
                return true;

            return false;
        }

        private async Task NotificarNovidadesAsync(string tipoImoveis, IEnumerable<IImovel> novidadesZapImoveis)
        {
            Console.WriteLine($"Foram encontrados [{novidadesZapImoveis.Count()}] novos imóveis em [{tipoImoveis}]");

            foreach (var item in novidadesZapImoveis)
                Console.WriteLine($"{item.Titulo} - {item.Endereco} | {item.ValorAluguel} {item.ValorCondominio}");

            await notificadorTelegram.NotificarGrupoNovidades(tipoImoveis, novidadesZapImoveis);
        }
    }
}

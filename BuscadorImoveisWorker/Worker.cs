using BuscadorImoveisWorker.Buscadores;
using BuscadorImoveisWorker.Config;
using BuscadorImoveisWorker.Servicos;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace BuscadorImoveisWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEnumerable<BuscaConfig> buscasConfig;
        private readonly IServiceProvider serviceProvider;
        private readonly NotificadorTelegram notificadorTelegram;
        private readonly IConfiguration configuration;
        private readonly AvaliadorImoveis avaliadorImoveis;

        public Worker(ILogger<Worker> logger, IEnumerable<BuscaConfig> buscasConfig, IServiceProvider serviceProvider, NotificadorTelegram notificadorTelegram, IConfiguration configuration, AvaliadorImoveis avaliadorImoveis)
        {
            _logger = logger;
            this.buscasConfig = buscasConfig;
            this.serviceProvider = serviceProvider;
            this.notificadorTelegram = notificadorTelegram;
            this.configuration = configuration;
            this.avaliadorImoveis = avaliadorImoveis;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            LogInicio();

            var intervaloEntreBuscas = configuration.GetValue<int>("IntervaloEntreBuscasMinutos");

            TimeSpan intervaloBuscas = TimeSpan.FromMinutes(intervaloEntreBuscas);

            var avaliacoesParaFazer = new List<AvaliacaoRequest>();
            foreach (var busca in buscasConfig.Where(b => b.Ativo))
            {
                Type buscadorImoveisType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name.Contains(busca.TipoBuscadorImoveis));
                var buscadorImoveis = serviceProvider.GetService(buscadorImoveisType) as IBuscadorImoveis;
                avaliacoesParaFazer.Add(new AvaliacaoRequest(buscadorImoveis, busca.IdBusca, busca.UrlPesquisa));
            }

            while (!stoppingToken.IsCancellationRequested)
            {

                Console.WriteLine("******************************************************************");
                Console.WriteLine($"[{DateTime.Now}] INICIANDO NOVA RODADA DE BUSCAS...");
                Console.WriteLine("******************************************************************");

                var relatorioParcial = $"Log de execu��o de [{DateTime.Now}]";

                foreach (var avaliacaoRequest in avaliacoesParaFazer)
                {
                    try
                    {
                        Console.WriteLine($"Iniciando avalia��o de [{avaliacaoRequest.TiposImoveis}]...");

                        var totalNovidades = await avaliadorImoveis.ExecutarAsync(avaliacaoRequest);

                        Console.WriteLine($"Finalizada avalia��o de [{avaliacaoRequest.TiposImoveis}]. Novos im�veis encontrados: {totalNovidades}");
                        relatorioParcial += $"\n[{avaliacaoRequest.TiposImoveis}] - Novos Im�veis: {totalNovidades}";
                    }
                    catch (Exception ex)
                    {
                        var mensagem = $"Falha ao tentar avaliar [{avaliacaoRequest.TiposImoveis}]: {ex.Message}";
                        Console.WriteLine(mensagem);
                        await notificadorTelegram.NotificarChatLog(mensagem);
                        relatorioParcial += $"\n[{avaliacaoRequest.TiposImoveis}] - Erro: {ex.Message}";
                    }
                }

                LogFimBusca(relatorioParcial);

                await notificadorTelegram.NotificarChatLog(relatorioParcial);

                Console.WriteLine($"Aguardando intervalo entre buscas... --> Pr�xima execu��o prevista para [{DateTime.Now.Add(intervaloBuscas)}]");

                await Task.Delay(intervaloBuscas, stoppingToken);
            }
        }

        private void LogInicio()
        {
            Console.WriteLine("******************************************************************");
            Console.WriteLine("            B U S C A D O R    D E    I M O V E I S               ");
            Console.WriteLine("******************************************************************");
        }

        private void LogFimBusca(string relatorioParcial)
        {
            Console.WriteLine("******************************************************************");
            Console.WriteLine($"[{DateTime.Now}] FINALIZADA BUSCA DE IM�VEIS.");
            Console.WriteLine(relatorioParcial);
            Console.WriteLine("******************************************************************");
        }
    }
}
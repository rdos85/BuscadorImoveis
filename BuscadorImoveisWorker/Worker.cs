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
        //private readonly AvaliadorNetImoveis avaliadorNetImoveis;
        //private readonly AvaliadorZapImoveis avaliadorZapImoveis;
        //private readonly AvaliadorCasaMineira avaliadorCasaMineira;
        private readonly NotificadorTelegram notificadorTelegram;

        public Worker(ILogger<Worker> logger, IEnumerable<BuscaConfig> buscasConfig, IServiceProvider serviceProvider, NotificadorTelegram notificadorTelegram)
        {
            _logger = logger;
            this.buscasConfig = buscasConfig;
            this.serviceProvider = serviceProvider;
            this.notificadorTelegram = notificadorTelegram;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            LogInicio();

            TimeSpan intervaloBuscas = TimeSpan.FromMinutes(30);

            var avaliacoesParaFazer = new List<AvaliacaoRequest>();
            foreach (var busca in buscasConfig)
            {
                Type avaliadorType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name.Contains(busca.TipoAvaliador));
                var avaliador = serviceProvider.GetService(avaliadorType) as IAvaliadorImoveis;
                avaliacoesParaFazer.Add(new AvaliacaoRequest(busca.IdBusca, avaliador, busca.UrlPesquisa));
            }

            while (!stoppingToken.IsCancellationRequested)
            {

                Console.WriteLine("******************************************************************");
                Console.WriteLine($"[{DateTime.Now}] INICIANDO NOVA RODADA DE BUSCAS...");
                Console.WriteLine("******************************************************************");

                var relatorioParcial = $"Log de execução de [{DateTime.Now}]";

                foreach (var avaliacaoRequest in avaliacoesParaFazer)
                {
                    try
                    {
                        Console.WriteLine($"Iniciando avaliação de [{avaliacaoRequest.TiposImoveis}]...");

                        var totalNovidades = await avaliacaoRequest.AvaliadorImoveis.ExecutarAsync(avaliacaoRequest.TiposImoveis, avaliacaoRequest.UrlBusca);

                        Console.WriteLine($"Finalizada avaliação de [{avaliacaoRequest.TiposImoveis}]. Novos imóveis encontrados: {totalNovidades}");
                        relatorioParcial += $"\n[{avaliacaoRequest.TiposImoveis}] - Novos Imóveis: {totalNovidades}";
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

                Console.WriteLine($"Aguardando intervalo entre buscas... --> Próxima execução prevista para [{DateTime.Now.Add(intervaloBuscas)}]");

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
            Console.WriteLine($"[{DateTime.Now}] FINALIZADA BUSCA DE IMÓVEIS.");
            Console.WriteLine(relatorioParcial);
            Console.WriteLine("******************************************************************");
        }
    }
}
using BuscadorImoveisWorker.Buscadores;
using BuscadorImoveisWorker.Servicos;

namespace BuscadorImoveisWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AvaliadorNetImoveis avaliadorNetImoveis;
        private readonly AvaliadorZapImoveis avaliadorZapImoveis;
        private readonly NotificadorTelegram notificadorTelegram;

        public Worker(ILogger<Worker> logger, AvaliadorNetImoveis avaliadorNetImoveis, AvaliadorZapImoveis avaliadorZapImoveis, NotificadorTelegram notificadorTelegram)
        {
            _logger = logger;
            this.avaliadorNetImoveis = avaliadorNetImoveis;
            this.avaliadorZapImoveis = avaliadorZapImoveis;
            this.notificadorTelegram = notificadorTelegram;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            LogInicio();

            TimeSpan intervaloBuscas = TimeSpan.FromMinutes(30);

            var avaliacoesParaFazer = new Dictionary<IAvaliadorImoveis, AvaliacaoRequest>()
            {
                { avaliadorNetImoveis, new AvaliacaoRequest("NetImoveis Coberturas", BuscadorNetImoveis.UrlBuscaCoberturas) },
                { avaliadorZapImoveis, new AvaliacaoRequest("ZapImoveis Coberturas", BuscadorZapImoveis.UrlBuscaCoberturas) }
            };

            while (!stoppingToken.IsCancellationRequested)
            {

                Console.WriteLine("******************************************************************");
                Console.WriteLine($"[{DateTime.Now}] INICIANDO NOVA BUSCA...");
                Console.WriteLine("******************************************************************");

                var relatorioParcial = $"Log de execução de [{DateTime.Now}]";

                foreach (var avaliacao in avaliacoesParaFazer)
                {
                    var avaliador = avaliacao.Key;
                    var request = avaliacao.Value;

                    try
                    {
                        Console.WriteLine($"Iniciando avaliação de [{request.TiposImoveis}]...");

                        var totalNovidades = await avaliacao.Key.ExecutarAsync(avaliacao.Value);

                        Console.WriteLine($"Finalizada avaliação de [{request.TiposImoveis}]. Novos imóveis encontrados: {totalNovidades}");
                        relatorioParcial += $"\n[{request.TiposImoveis}] - Novos Imóveis: {totalNovidades}";
                    }
                    catch (Exception ex)
                    {
                        var mensagem = $"Falha ao tentar avaliar [{avaliacao.Key}]: {ex.Message}";
                        Console.WriteLine(mensagem);
                        await notificadorTelegram.NotificarChatLog(mensagem);
                        relatorioParcial += $"\n[{request.TiposImoveis}] - Erro: {ex.Message}";
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
using BuscadorImoveisWorker;
using BuscadorImoveisWorker.Buscadores;
using BuscadorImoveisWorker.Config;
using BuscadorImoveisWorker.Infra;
using BuscadorImoveisWorker.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<BuscadorNetImoveis>();
        services.AddSingleton<BuscadorZapImoveis>();
        services.AddSingleton<AvaliadorNetImoveis>();
        services.AddSingleton<AvaliadorZapImoveis>();
        services.AddSingleton<NotificadorTelegram>();
        services.AddSingleton<TelegramBotClient>(services =>
        {
            var configuration = services.GetService<IConfiguration>();
            return new TelegramBotClient(configuration.GetValue<string>("TelegramConfig:Token"));
        });
        services.AddDbContext<ImoveisDbContext>(op => op.UseSqlServer("Server=::1,1433;Database=master;User Id=SA;Password=Senha01*;"),
                                                      ServiceLifetime.Singleton);
    })
    .Build();



await host.RunAsync();

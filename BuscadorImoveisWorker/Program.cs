using BuscadorImoveisWorker;
using BuscadorImoveisWorker.Buscadores;
using BuscadorImoveisWorker.Config;
using BuscadorImoveisWorker.Infra;
using BuscadorImoveisWorker.Servicos;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton(context.Configuration.GetSection("TelegramConfig").Get<TelegramConfig>());
        services.AddSingleton(context.Configuration.GetSection("BuscaConfig").Get<IEnumerable<BuscaConfig>>());
        services.AddSingleton<AvaliadorImoveis>();
        services.AddSingleton<BuscadorNetImoveis>();
        services.AddSingleton<BuscadorZapImoveis>();
        services.AddSingleton<BuscadorCasaMineira>();
        services.AddSingleton<BuscadorVivaReal>();
        services.AddSingleton<BuscadorImovelWeb>();
        services.AddSingleton<NotificadorTelegram>();
        services.AddSingleton<ITelegramBotClient>(services =>
        {
            var configuration = services.GetService<IConfiguration>();
            return new TelegramBotClient(configuration.GetValue<string>("TelegramConfig:Token"));
        });
        services.AddHangfire(hangfireConfig =>
        {
            hangfireConfig.UseSqlServerStorage(context.Configuration.GetConnectionString("SqlServer"));
        });
        services.AddHangfireServer();
        services.AddDbContext<ImoveisDbContext>(op => op.UseSqlServer(context.Configuration.GetConnectionString("SqlServer")),
                                                      ServiceLifetime.Singleton);
    })
    .Build();



await host.RunAsync();

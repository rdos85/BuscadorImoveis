using Polly;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using BuscadorImoveisWorker.Entidades;
using Microsoft.Extensions.Options;
using BuscadorImoveisWorker.Config;

namespace BuscadorImoveisWorker.Servicos
{
    public class NotificadorTelegram
    {
        private readonly TelegramConfig telegramConfig;
        private readonly TelegramBotClient telegramBotClient;
        
        private readonly ChatId grupoCoberturaChatId;
        private readonly ChatId chatLogId;
        private readonly AsyncPolicy retryPolicy;

        public NotificadorTelegram(TelegramConfig telegramConfig, TelegramBotClient telegramBotClient)
        {
            this.telegramConfig = telegramConfig;
            this.telegramBotClient = telegramBotClient;

            grupoCoberturaChatId = new ChatId(telegramConfig.GrupoNotificacaoId);
            chatLogId = new ChatId(telegramConfig.ChatLogId);

            retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3, (retryCount) => TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
        }

        public async Task NotificarGrupoNovidades(string tipoImoveis, IEnumerable<IImovel> novidades)
        {
            if (!novidades.Any())
                return;

            string mensagem = $"ENCONTRADOS EM [{tipoImoveis}]\n\n";

            foreach (var imovel in novidades)
                mensagem += $"\n• {imovel.CriarMensagemTelegram()}\n";

            var sendMessageRequest = new SendMessageRequest(grupoCoberturaChatId, mensagem);

            sendMessageRequest.ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html;

            _ = await retryPolicy.ExecuteAsync(async () => _ = await telegramBotClient.MakeRequestAsync(sendMessageRequest));
        }

        public async Task NotificarChatLog(string mensagem)
        {
            var sendMessageRequest = new SendMessageRequest(chatLogId, mensagem);
            _ = await retryPolicy.ExecuteAsync(async () => _ = await telegramBotClient.MakeRequestAsync(sendMessageRequest));
        }

    }
}

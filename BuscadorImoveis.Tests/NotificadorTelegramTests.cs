using BuscadorImoveisWorker.Config;
using BuscadorImoveisWorker.Entidades;
using BuscadorImoveisWorker.Servicos;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Requests;

namespace BuscadorImoveis.Tests
{
    public class NotificadorTelegramTests
    {
        private TelegramConfig telegramConfig;
        private Mock<ITelegramBotClient> mockTelegramClient;

        public NotificadorTelegramTests()
        {
            telegramConfig = new TelegramConfig
            {
                ChatLogId = "1",
                GrupoNotificacaoId = "1",
                DisparoLogsAtivo = true,
                DisparoNovidadesAtivo = true,
                Token = "000"
            };

            mockTelegramClient = new Mock<ITelegramBotClient>();
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(NotificadorTelegram.ImoveisPorMensagem - 1, 1)]
        [InlineData(NotificadorTelegram.ImoveisPorMensagem, 1)]
        [InlineData(NotificadorTelegram.ImoveisPorMensagem + 1, 2)]
        [InlineData(NotificadorTelegram.ImoveisPorMensagem * 3, 3)]
        [InlineData(NotificadorTelegram.ImoveisPorMensagem * 2.5, 3)]
        public async Task NotificarGrupoNovidades_TestarQuantidadeMensagensPorVolumeDeImoveis(int quantidadeImoveis, int numeroMensagensEsperadas)
        {
            var notificador = new NotificadorTelegram(telegramConfig, mockTelegramClient.Object);
            await notificador.NotificarGrupoNovidadesAsync("xxx", CriarFakeImoveis(quantidadeImoveis));

            mockTelegramClient.Verify(m => m.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(numeroMensagensEsperadas));
        }

        private IEnumerable<Imovel> CriarFakeImoveis(int quantidade)
        {
            var fakeImoveis = new List<Imovel>();

            for (int i = 0; i < quantidade; i++)
            {
                fakeImoveis.Add(new Imovel
                {
                    Id = i.ToString(),
                    Origem = "Teste",
                    DataInclusao = DateTime.Now,
                    DataModificacao = DateTime.MinValue,
                    Endereco = $"Endereco {i}",
                    Titulo = $"Título do Endereço {i}",
                    Quartos = new Random().Next(5).ToString(),
                    Vagas = new Random().Next(5).ToString(),
                    ValorAluguel = new Random().Next(5).ToString(),
                    ValorCondominio = new Random().Next(5).ToString(),
                });
            }

            return fakeImoveis;
        }
    }
}

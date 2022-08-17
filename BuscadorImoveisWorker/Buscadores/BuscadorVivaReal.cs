using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BuscadorImoveisWorker.Entidades;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Buscadores
{
    public class BuscadorVivaReal
    {
        public const string Origem = "Viva Real";
        public const string UrlBase = @"https://www.vivareal.com.br";

        public async Task<IList<ImovelVivaReal>> BuscarImoveisAsync(string tipoBusca, string url)
        {
            IList<ImovelVivaReal> imoveis = new List<ImovelVivaReal>();

            using var chrome = new ChromeDriver(@"C:\chromedriver");

            try
            {
                Console.WriteLine($"Iniciando buscas em [{Origem}] por [{tipoBusca}]...");

                chrome.Manage().Window.Maximize();
                chrome.Navigate().GoToUrl(url);
                var parser = new HtmlParser();

                while (true)
                {
                    var trintaSegundosStoppingToken = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    var parsedDocument = await AguardarPaginaCarregarAsync(chrome, trintaSegundosStoppingToken.Token);

                    var resultListElements = parsedDocument.GetElementsByClassName("results-list");

                    if (!resultListElements.Any())
                        break;

                    var imoveisElements = resultListElements[0].Children;

                    foreach (var imovelElement in imoveisElements)//imoveisElements.Children)
                    {
                        var id = imovelElement?.FirstElementChild?.GetAttribute("id");
                        if (id == null || !long.TryParse(id, out long idResult))
                            continue;

                        var link = UrlBase + imovelElement.GetElementsByTagName("a").First().GetAttribute("href");

                        var dadosElement = imovelElement.GetElementsByClassName("property-card__content")[0];

                        var titulo = dadosElement.GetElementsByClassName("property-card__title")[0].TextContent.Trim();
                        var endereco = dadosElement.GetElementsByClassName("property-card__address")[0].TextContent.Trim();
                        var quartos = dadosElement.GetElementsByClassName("property-card__detail-room")[0].TextContent.Trim();
                        var banheiros = dadosElement.GetElementsByClassName("property-card__detail-bathroom")[0].TextContent.Trim();
                        var vagas = dadosElement.GetElementsByClassName("property-card__detail-garage")[0].TextContent.Trim();
                        var valorAluguel = dadosElement.GetElementsByClassName("property-card__price")[0].TextContent.Trim();
                        var valorCondominio = "Não informado";
                        if (dadosElement.GetElementsByClassName("property-card__price-details--condo").Any())
                            valorCondominio = dadosElement.GetElementsByClassName("property-card__price-details--condo")[0].TextContent.Trim();

                        var imovel = new ImovelVivaReal
                        {
                            Id = id,
                            Link = link,
                            Endereco = endereco,
                            Quartos = quartos,
                            Vagas = vagas,
                            Titulo = titulo,
                            ValorAluguel = valorAluguel,
                            ValorCondominio = valorCondominio
                        };

                        imoveis.Add(imovel);
                    }

                    // Tenta avançar para próxima página.
                    AceitarCookies(chrome);
                    var botaoPaginacao = chrome.FindElements(By.ClassName("js-change-page")).LastOrDefault();

                    if (botaoPaginacao == null || botaoPaginacao.GetAttribute("data-disabled") != null)
                        break;
                    
                    var urlAntesClick = chrome.Url;

                    botaoPaginacao.Click();

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                
                Console.WriteLine($"Encontrados [{imoveis.Count}] imóveis em [{Origem}]...");
                return imoveis;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao tentar buscar imóveis {Origem}: {ex.Message}");
                throw;
            }
            finally
            {
                Console.WriteLine("Closing Chrome...");
                chrome.Close();
                Console.WriteLine("Chrome closed.");
            }
        }

        public void AceitarCookies(ChromeDriver chrome)
        {
            var botaoAceitarCookies = chrome.FindElements(By.ClassName("cookie-notifier__cta"));

            if (botaoAceitarCookies.Any())
                botaoAceitarCookies.First().Click();
        }

        public async Task<IHtmlDocument> AguardarPaginaCarregarAsync(ChromeDriver chrome, CancellationToken stoppingToken)
        {
            var parser = new HtmlParser();

            var parsedDocument = parser.ParseDocument(chrome.PageSource);

            // Enquanto não encontrar um iframe com atributo "sandbox", indica que a página ainda está carregando. 
            while (!parsedDocument.GetElementsByTagName("iframe").Any(iframe => iframe.GetAttribute("sandbox") != null))
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await Task.Delay(3000);
                parsedDocument = parser.ParseDocument(chrome.PageSource);
            }

            return parsedDocument;
        }
    }
}

using AngleSharp.Html.Parser;
using BuscadorImoveisWorker.Entidades;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Buscadores
{
    public class BuscadorCasaMineira
    {
        public const string UrlBuscaImoveis = $"https://www.casamineira.com.br/aluguel/apartamento/belo-horizonte_mg/3-quartos+2-banheiros+2-vagas+ordem-mais-recente+preco-minimo-2000+preco-maximo-5000";
        public const string Origem = "Casa Mineira";
        public const string BaseUrl = "https://www.casamineira.com.br";

        public async Task<IList<ImovelCasaMineira>> BuscarImoveisAsync(string tipoBusca, string url)
        {
            var imoveis = new List<ImovelCasaMineira>();

            using var chrome = new ChromeDriver(@"C:\chromedriver");
            try
            {
                Console.WriteLine($"Iniciando buscas em [{Origem}] por [{tipoBusca}]...");

                chrome.Manage().Window.Maximize();
                chrome.Navigate().GoToUrl(url);

                while (true)
                {
                    var content = chrome.PageSource;
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(content);

                    var elementRowImoveis = document.GetElementsByClassName("components__CardContainer-sc-1tt2vbg-3");
                    if (!elementRowImoveis.Any())
                        break;

                    foreach (var elementImovel in elementRowImoveis)
                    {
                        var elementIdLink = elementImovel.FirstElementChild;
                        var id = elementIdLink.GetAttribute("data-id");
                        var link = BaseUrl + elementIdLink.GetAttribute("data-to-posting");

                        var dadosImovelElement = elementImovel.GetElementsByClassName("postingCardstyles__PostingTop-sc-i1odl-3");

                        var valorAluguel = dadosImovelElement[0].GetElementsByClassName("components__Price-sc-12dh9kl-4")[0].TextContent;
                        var valorCondominio = "Não informado";
                        if (dadosImovelElement[0].GetElementsByClassName("components__Expenses-sc-12dh9kl-2").Any())
                            valorCondominio = dadosImovelElement[0].GetElementsByClassName("components__Expenses-sc-12dh9kl-2")[0].TextContent;
                        var rua = dadosImovelElement[0].GetElementsByClassName("components__LocationAddress-sc-ge2uzh-0")[0].TextContent;
                        var bairro = dadosImovelElement[0].GetElementsByClassName("components__LocationLocation-sc-ge2uzh-2")[0].TextContent;

                        var detalhesElementSpan = dadosImovelElement[0].GetElementsByClassName("components__PostingMainFeaturesBlock-sc-1uhtbxc-0")[0].Children;

                        var quartos = detalhesElementSpan[2].GetElementsByTagName("span")[0].TextContent;
                        var banheiros = detalhesElementSpan[3].GetElementsByTagName("span")[0].TextContent;
                        var vagas = detalhesElementSpan[4].GetElementsByTagName("span")[0].TextContent;

                        imoveis.Add(new ImovelCasaMineira
                        {
                            Id = id,
                            Link = link,
                            Titulo = rua,
                            Endereco = rua + ", " + bairro,
                            Quartos = quartos,
                            Vagas = vagas,
                            ValorAluguel = valorAluguel,
                            ValorCondominio = valorCondominio
                        });
                    }

                    // Click no botão "Próximo"
                    var pagingButtons = chrome.FindElements(By.ClassName("stylespaging__PageArrow-sc-n5babu-2"));

                    var nextPageButton = pagingButtons.FirstOrDefault(b => b.GetAttribute("data-qa") == "PAGING_NEXT");

                    if (nextPageButton == null)
                        break;

                    var urlAnterior = chrome.Url;

                    nextPageButton.Click();

                    await Task.Delay(2000);

                    var urlPosClick = chrome.Url;

                    // Se ao clicar no botão a URL não se alterar, indica que não há mais páginas.
                    if (urlAnterior == urlPosClick)
                        break;
                }

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

    }
}

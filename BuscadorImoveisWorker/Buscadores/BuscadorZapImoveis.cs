using AngleSharp.Html.Parser;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using BuscadorImoveisWorker.Util;
using BuscadorImoveisWorker.Entidades;

namespace BuscadorImoveisWorker.Buscadores
{
    public class BuscadorZapImoveis
    {
        public const string Origem = "ZapImoveis";
        public const string BaseUrl = "https://www.zapimoveis.com.br";

        public async Task<List<ImovelZapImoveis>> BuscarImoveisAsync(string tipoBusca, string url)
        {
            var imoveis = new List<ImovelZapImoveis>();

            using var chrome = new ChromeDriver(@"C:\chromedriver");
            Actions action = new Actions(chrome);
            var parser = new HtmlParser();

            try
            {
                chrome.Manage().Window.Maximize();

                chrome.Navigate().GoToUrl(url);

                while (true)
                {
                    AceitarCookies(chrome);

                    FecharPopUpPesquisa(chrome);

                    var document = parser.ParseDocument(chrome.PageSource);

                    var imoveisElements = document.GetElementsByClassName("simple-card__box");

                    if (!imoveisElements.Any())
                    {
                        Console.WriteLine("Nenhum imóvel encontrado.");
                        break;
                    }

                    foreach (var imovelElement in imoveisElements)
                    {
                        var titulosEnderecosPrecos = imovelElement.GetElementsByTagName("p");

                        var titulo = titulosEnderecosPrecos[0].TextContent.LimparString();
                        var endereco = titulosEnderecosPrecos[1].TextContent.LimparString();
                        var valorAluguel = titulosEnderecosPrecos[2].TextContent.LimparString();

                        var valores = imovelElement.GetElementsByClassName("card-price__value");
                        var condominio = "Não informado";
                        var iptu = "Não informado";

                        if (valores.Count() >= 2)
                        {
                            condominio = valores[0].TextContent.LimparString();
                            iptu = valores[1].TextContent.LimparString();
                        }

                        var detalhlesElements = imovelElement.GetElementsByClassName("simple-card__amenities")[0].GetElementsByTagName("span");

                        var quartos = detalhlesElements[3].TextContent.LimparString();
                        var vagas = detalhlesElements[5].TextContent.LimparString();

                        var imovel = new ImovelZapImoveis
                        {
                            Titulo = titulo,
                            Endereco = endereco,
                            ValorAluguel = valorAluguel,
                            ValorCondominio = condominio,
                            Iptu = iptu,
                            Quartos = quartos,
                            Vagas = vagas
                        };

                        imovel.CriarId();
                        imovel.CriarLink();

                        imoveis.Add(imovel);
                    }

                    var nextPageButtons = chrome.FindElements(By.ClassName("pagination__button--next"));

                    if (!nextPageButtons.Any())
                        break;

                    action.Click(nextPageButtons[0]).Perform();

                    await Task.Delay(5000);
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

        private void FecharPopUpPesquisa(ChromeDriver chrome)
        {
            var popupPesquisa = chrome.FindElements(By.ClassName("QSIWebResponsiveDialog-Layout1-SI_008OOpWkfLXlB6C_close-btn"));

            if (popupPesquisa.Any())
                popupPesquisa[0].Click();
        }

        private void AceitarCookies(ChromeDriver chrome)
        {
            var aceiteCookiesButtons = chrome.FindElements(By.ClassName("cookie-notifier__cta"));

            if (aceiteCookiesButtons.Any())
                aceiteCookiesButtons[0].Click();
        }
    }
}

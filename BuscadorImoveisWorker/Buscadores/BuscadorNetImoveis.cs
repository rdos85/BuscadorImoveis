using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BuscadorImoveisWorker.Entidades;
using BuscadorImoveisWorker.Util;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Buscadores
{
    public class BuscadorNetImoveis
    {
        public const string Origem = "NetImoveis";
        public const string BaseUrl = "https://www.netimoveis.com";

        public async Task<IList<ImovelNetImoveis>> BuscarImoveisAsync(string tipoBusca, string url)
        {
            var imoveis = new List<ImovelNetImoveis>();

            using var chrome = new ChromeDriver(@"C:\chromedriver");
            try
            {
                Console.WriteLine($"Iniciando buscas em [{Origem}] por [{tipoBusca}]...");

                chrome.Navigate().GoToUrl(url);

                while (true)
                {
                    var content = chrome.PageSource;
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(content);

                    var elementRowImoveis = document.GetElementsByClassName("row imoveis");
                    if (!elementRowImoveis.Any())
                        break;

                    var elementImoveis = elementRowImoveis[0].QuerySelectorAll("article");

                    foreach (var elementImovel in elementImoveis)
                    {
                        var idImovel = elementImovel.GetAttribute("id");
                        var href = elementImovel.GetElementsByClassName("link-imovel")[0].GetAttribute("href");
                        var titulo = elementImovel.GetElementsByTagName("h2")[0].TextContent.LimparString();
                        var endereco = elementImovel.GetElementsByClassName("endereco")[0].TextContent.LimparString();
                        var quartos = elementImovel.GetElementsByClassName("caracteristica quartos")[0].TextContent.LimparString();
                        var vagas = elementImovel.GetElementsByClassName("caracteristica vagas")[0].TextContent.LimparString();
                        var elementImovelValor = elementImovel.GetElementsByClassName("imovel-valor")[0];
                        var valorAluguel = elementImovelValor.GetElementsByClassName("valor")[0].TextContent.LimparString();

                        var elementCondominio = elementImovelValor.GetElementsByClassName("condominio");
                        var valorCondominio = "Não informado";
                        if (elementCondominio.Any())
                            valorCondominio = elementCondominio[0].TextContent.LimparString();

                        imoveis.Add(new ImovelNetImoveis
                        {
                            Origem = Origem,
                            Id = idImovel,
                            Link = BaseUrl + href,
                            Titulo = titulo,
                            Endereco = endereco,
                            Quartos = quartos,
                            Vagas = vagas,
                            ValorAluguel = valorAluguel,
                            ValorCondominio = valorCondominio
                        });
                    }

                    // Click no botão "Próximo"
                    var nextPageLink = chrome.FindElement(By.ClassName("clnext"));

                    var urlAnterior = chrome.Url;

                    nextPageLink.Click();

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

using AngleSharp.Html.Parser;
using BuscadorImoveisWorker.Entidades;
using BuscadorImoveisWorker.Util;
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
    public class BuscadorImovelWeb : IBuscadorImoveis
    {
        public const string BaseUrl = @"https://www.imovelweb.com.br";
        public string Origem => "ImovelWeb";

        public async Task<IList<Imovel>> BuscarImoveisAsync(string tipoBusca, string urlOriginal)
        {
            var imoveis = new List<Imovel>();

            try
            {
                Console.WriteLine($"Iniciando buscas em [{Origem}] por [{tipoBusca}]...");

                var pagina = 1;

                while (true)
                {
                    using var chrome = new ChromeDriver(@"C:\chromedriver");
                    chrome.Manage().Window.Maximize();

                    var url = urlOriginal;

                    if (pagina > 1)
                        url = urlOriginal.Replace(".html", $"-pagina-{pagina}.html");

                    chrome.Navigate().GoToUrl(url);

                    await Task.Delay(5000);

                    if (chrome.Url != url)
                        break;

                    var content = chrome.PageSource;
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(content);

                    var elementImoveis = document.GetElementsByClassName("postingCardstyles__PostingCardLayout-sc-i1odl-0");
                    if (!elementImoveis.Any())
                        break;

                    foreach (var elementImovel in elementImoveis)
                    {
                        var id = elementImovel.GetAttribute("data-id");
                        var link = elementImovel.GetAttribute("data-to-posting");
                        var titulo = elementImovel.GetElementsByClassName("postingCardstyles__PostingTitleLink-sc-i1odl-11")[0].TextContent.LimparString();

                        var valorAluguel = elementImovel.GetElementsByClassName("components__Price-sc-12dh9kl-4")[0].TextContent.LimparString();
                        var valorCondominio = "Não informado";
                        if (elementImovel.GetElementsByClassName("components__Expenses-sc-12dh9kl-2").Any())
                            valorCondominio = elementImovel.GetElementsByClassName("components__Expenses-sc-12dh9kl-2")[0].TextContent.LimparString();

                        var endereco = "";
                        if (elementImovel.GetElementsByClassName("components__LocationAddress-sc-ge2uzh-0 gVRwwW").Any())
                            endereco += elementImovel.GetElementsByClassName("components__LocationAddress-sc-ge2uzh-0 gVRwwW")[0].TextContent.LimparString();
                        if (elementImovel.GetElementsByClassName("components__LocationLocation-sc-ge2uzh-2 gKegYU").Any())
                            endereco += elementImovel.GetElementsByClassName("components__LocationLocation-sc-ge2uzh-2 gKegYU")[0].TextContent.LimparString();

                        var detalhesElement = elementImovel.GetElementsByClassName("components__PostingMainFeaturesBlock-sc-1uhtbxc-0")[0];

                        var quartos = detalhesElement.Children[1].TextContent.LimparString();
                        var vagas = detalhesElement.Children[3].TextContent.LimparString();

                        imoveis.Add(new Imovel
                        {
                            Id = id,
                            Origem = Origem,
                            Link = BaseUrl + link,
                            Titulo = titulo,
                            Endereco = endereco,
                            Quartos = quartos,
                            Vagas = vagas,
                            ValorAluguel = valorAluguel,
                            ValorCondominio = valorCondominio,
                            Iptu = "Não Informado"
                        });
                    }

                    // O site tem algum mecanismo que impede o click no botão "Próxima Página".
                    // Por isso pra cada página fecha e abre o chrome. 
                    // Se tentar acessar a url sem fechar o chrome, ele suspeita de bot e abre captcha. 
                    pagina++;
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
                
            }
        }
    }
}

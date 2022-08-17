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
using System.IO.Compression;

namespace BuscadorImoveisWorker.Buscadores
{
    public class BuscadorZapImoveis : IBuscadorImoveis
    {
        public const string BaseUrl = "https://www.zapimoveis.com.br";

        public string Origem => "ZapImoveis";

        public async Task<IList<Imovel>> BuscarImoveisAsync(string tipoBusca, string url)
        {
            var imoveis = new List<Imovel>();

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

                        var imovel = new Imovel
                        {
                            Origem = Origem,
                            Link = "",
                            Titulo = titulo,
                            Endereco = endereco,
                            ValorAluguel = valorAluguel,
                            ValorCondominio = condominio,
                            Iptu = iptu,
                            Quartos = quartos,
                            Vagas = vagas
                        };

                        CriarIdImovel(imovel);

                        imoveis.Add(imovel);
                    }

                    var nextPageButtons = chrome.FindElements(By.ClassName("pagination__button--next"));

                    if (!nextPageButtons.Any())
                        break;

                    action.Click(nextPageButtons[0]).Perform();

                    await Task.Delay(5000);
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

        private void CriarIdImovel(Imovel imovel)
        {
            var imovelString = $"{imovel.Titulo}{imovel.Endereco}{imovel.ValorAluguel}{imovel.ValorCondominio}{imovel.Vagas}";

            var id = Compress(imovelString);
            imovel.Id = id;
        }

        private static string Compress(string uncompressedString)
        {
            byte[] compressedBytes;

            using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString)))
            {
                using (var compressedStream = new MemoryStream())
                {
                    // setting the leaveOpen parameter to true to ensure that compressedStream will not be closed when compressorStream is disposed
                    // this allows compressorStream to close and flush its buffers to compressedStream and guarantees that compressedStream.ToArray() can be called afterward
                    // although MSDN documentation states that ToArray() can be called on a closed MemoryStream, I don't want to rely on that very odd behavior should it ever change
                    using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Optimal, true))
                    {
                        uncompressedStream.CopyTo(compressorStream);
                    }

                    // call compressedStream.ToArray() after the enclosing DeflateStream has closed and flushed its buffer to compressedStream
                    compressedBytes = compressedStream.ToArray();
                }
            }

            return Convert.ToBase64String(compressedBytes);
        }
    }
}

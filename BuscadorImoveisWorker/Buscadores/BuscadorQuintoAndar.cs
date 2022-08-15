using AngleSharp.Html.Parser;
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
    public class BuscadorQuintoAndar
    {
        public const string UrlBuscaCoberturas = @"https://www.quintoandar.com.br/alugar/imovel/belo-horizonte-mg-brasil/apartamento/de-500-a-5100-aluguel/3-4-quartos/2-3-vagas/apartamento-cobertura";
        public const string Origem = "NetImoveis";

        public string ImoveisClass = "MuiPaper-root MuiCard-root";

        public async Task Teste()
        {
            using var chrome = new ChromeDriver(@"C:\chromedriver");
            chrome.Manage().Window.Maximize();
            Actions action = new Actions(chrome);
            var parser = new HtmlParser();

            chrome.Navigate().GoToUrl(UrlBuscaCoberturas);

            while (true)
            {
                var lastElement = chrome.FindElements(By.ClassName("MuiPaper-root")).Last();

                var idAntesScroll = lastElement.GetHashCode();

                action.ScrollToElement(lastElement).Perform();

                var idAposScroll = chrome.FindElements(By.ClassName("MuiPaper-root")).Last().GetHashCode();

                if (idAntesScroll == idAposScroll)
                    break;

                await Task.Delay(1000);
            }

            //var document = parser.ParseDocument(chrome.PageSource);

            /*var apsElements = document.GetElementsByClassName(ImoveisClass);

            foreach (var item in apsElements)
            {
                var x = item.InnerHtml;
            }*/

            var html = chrome.ExecuteScript("return document.documentElement.outerHTML;");

            var parsedDocument = parser.ParseDocument(html.ToString());

            //var imoveis = chrome.FindElements(By.ClassName("MuiCard-root"));

            var imoveis = parsedDocument.GetElementsByClassName("MuiCard-root");

            foreach (var imovelElement in imoveis)
            {
                var elementEndereco = parsedDocument.GetElementsByClassName("CozyTooltipWrapper");

                if (!elementEndereco.Any())
                    continue;

                var endereco = elementEndereco.First().TextContent;

                Console.WriteLine(endereco);

                /*var elementoEndereco = imovelElement.FindElements(By.ClassName("CozyTooltipWrapper"));
                if (!elementoEndereco.Any())
                    continue;

                var endereco = elementoEndereco[0].FindElement(By.TagName("p")).Text;*/
            }
        }
    }
}

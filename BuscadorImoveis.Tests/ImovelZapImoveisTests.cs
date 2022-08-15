
using BuscadorImoveisWorker.Entidades;
using System.Security.Cryptography;
using System.Text;

namespace BuscadorImoveis.Tests
{
    public class ImovelZapImoveisTests
    {
        [Fact]
        public void Test1()
        {
            var imovel = new ImovelZapImoveis
            {
                Titulo = "Um imóvel bom demais",
                Endereco = "Com uma localização espetacular",
                Link = "http://www.zapimoveis.com.br",
                Vagas = "4",
                Quartos = "3",
                Iptu = "R$ 500",
                ValorAluguel = "R$ 1000",
                ValorCondominio = "R$ 200"
            };


            imovel.CriarId();
            var id1 = imovel.Id;

            imovel.CriarId();
            var id2 = imovel.Id;

            Assert.Equal(id1, id2);
        }
    }
}
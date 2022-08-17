using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Entidades
{
    public class ImovelZapImoveis : IImovel
    {
        [Key]
        [JsonIgnore]
        public string Id { get; set; }
        public string Link { get; set; }
        public string Titulo { get; set; }
        public string Endereco { get; set; }
        public string ValorAluguel { get; set; }
        public string ValorCondominio { get; set; }
        public string Iptu { get; set; }
        public string Quartos { get; set; }
        public string Vagas { get; set; }

        public void CriarId()
        {
            var imovelJson = System.Text.Json.JsonSerializer.Serialize(this);
            var stringId = Encoding.UTF8.GetBytes(imovelJson);
            var idBytes = SHA256.Create().ComputeHash(stringId);
            var idString = Encoding.UTF8.GetString(idBytes);

            Id = idString;
        }

        public void CriarLink()
        {
            Link = @$"http://www.google.com/search?q=www.zapimoveis.com.br+{Titulo}+{Endereco}+{ValorAluguel}".Replace(" ", "+");
        }

        public string CriarMensagemTelegram()
        {
            return $"{Titulo} - {Endereco} - {ValorAluguel} + Condomínio {ValorCondominio} + Iptu {Iptu}";
        }
    }
}

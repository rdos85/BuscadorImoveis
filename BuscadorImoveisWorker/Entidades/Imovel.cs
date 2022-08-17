using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Entidades
{
    public class Imovel
    {
        [JsonIgnore]
        public string Id { get; set; }
        public string Origem { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime DataModificacao { get; set; }
        public string Link { get; set; }
        public string Titulo { get; set; }
        public string Endereco { get; set; }
        public string ValorAluguel { get; set; }
        public string ValorCondominio { get; set; }
        public string Quartos { get; set; }
        public string Vagas { get; set; }
        public string Iptu { get; set; }

        public string CriarMensagemTelegram()
        {
            var texto = $"{Titulo} - {Endereco} - {ValorAluguel} {ValorCondominio}";
            if (string.IsNullOrEmpty(Link))
                return texto;
            else
                return $"<a href='{Link}'>{texto}</a>";
        }
    }
}

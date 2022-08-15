using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BuscadorImoveisWorker.Entidades
{
    public class ImovelNetImoveis : IImovel
    {
        public string Origem { get; set; }
        [Key]
        public string Id { get; set; }
        public string Link { get; set; }
        public string Titulo { get; set; }
        public string Endereco { get; set; }
        public string Quartos { get; set; }
        public string Vagas { get; set; }
        public string ValorAluguel { get; set; }
        public string ValorCondominio { get; set; }

        public string CriarMensagemTelegram()
        {
            return $"<a href='{Link}'>{Titulo} - {Endereco} - {ValorAluguel} {ValorCondominio}</a>";
        }
    }
}

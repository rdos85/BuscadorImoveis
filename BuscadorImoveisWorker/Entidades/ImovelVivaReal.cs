using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Entidades
{
    public class ImovelVivaReal : IImovel
    {
        [Key]
        public string Id { get; set; }
        public string Link { get; set; }
        public string Titulo { get; set; }
        public string Endereco { get; set; }
        public string ValorAluguel { get; set; }
        public string ValorCondominio { get; set; }
        public string Quartos { get; set; }
        public string Vagas { get; set; }

        public string CriarMensagemTelegram()
        {
            return $"<a href='{Link}'>{Titulo} - {Endereco} - {ValorAluguel} {ValorCondominio}</a>";
        }
    }
}

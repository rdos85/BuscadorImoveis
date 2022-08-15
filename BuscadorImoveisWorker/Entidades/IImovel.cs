using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Entidades
{
    public interface IImovel
    {
        public string Id { get; set; }
        public string Link { get; set; }
        public string Titulo { get; set; }
        public string Endereco { get; set; }
        public string ValorAluguel { get; set; }
        public string ValorCondominio { get; set; }

        string CriarMensagemTelegram();
    }
}

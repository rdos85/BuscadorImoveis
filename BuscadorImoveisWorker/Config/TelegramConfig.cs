using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuscadorImoveisWorker.Config
{
    public class TelegramConfig
    {
        public string Token { get; set; }
        public string ChatLogId { get; set; }
        public string GrupoNotificacaoId { get; set; }
        public bool DisparoNovidadesAtivo { get; set; }
        public bool DisparoLogsAtivo { get; set; }
    }
}

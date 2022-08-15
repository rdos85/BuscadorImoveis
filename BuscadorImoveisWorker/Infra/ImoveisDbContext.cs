using BuscadorImoveisWorker.Entidades;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuscadorImoveisWorker.Infra
{
    public class ImoveisDbContext : DbContext
    {
        public ImoveisDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ImovelNetImoveis> ImovelNetImoveis { get; set; }
        public DbSet<ImovelZapImoveis> ImovelZapImoveis { get; set; }
        public DbSet<ImovelCasaMineira> ImovelCasaMineira { get; set; }
    }
}

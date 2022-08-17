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
        public DbSet<Imovel> Imovel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Imovel>().HasKey(x => new { x.Id, x.Origem });
        }
    }
}

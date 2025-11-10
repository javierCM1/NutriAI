using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Entidad.Context
{
    //esto lo use para migracion. habia creado una clase pero bueno la termine descartando xd. lo dejo aca por las dudas igual
    public class NutriAIContextFactory : IDesignTimeDbContextFactory<NutriAIContext>
    {
        public NutriAIContext CreateDbContext(string[] args)
        {
            // Todo el código va DENTRO del método
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..\\NutriAI")) // Ruta relativa a tu appsettings.json
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<NutriAIContext>();
            var connectionString = configuration.GetConnectionString("NutriAIConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new NutriAIContext(optionsBuilder.Options);
        }
    }
}


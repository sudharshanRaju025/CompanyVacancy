using CompanyVacancy.Models;

using Microsoft.EntityFrameworkCore;


namespace CompanyVacancy
{

    public class AddDbFile : DbContext

    {
        public AddDbFile(DbContextOptions<AddDbFile> options)
            : base(options)
        {

        }
        public DbSet<CompanyDetails> CompanyDetails { get; set; }
    }
}
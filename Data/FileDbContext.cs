using File.Models;
using Microsoft.EntityFrameworkCore;

namespace File.Data
{
    public class FileDbContext: DbContext
    {
        public FileDbContext(DbContextOptions<FileDbContext> options)
            : base(options)
        {

        }
        public DbSet<Person> person { get; set; }
        public DbSet<Files> files { get; set; }
    }
}

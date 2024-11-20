using Microsoft.EntityFrameworkCore;
using shared.Model;

namespace Data
{
    public class OrdinationContext : DbContext
    {
        public DbSet<Patient> Patienter => Set<Patient>();
        public DbSet<PN> PNs => Set<PN>();
        public DbSet<DagligFast> DagligFaste => Set<DagligFast>();
        public DbSet<DagligSkaev> DagligSkaeve => Set<DagligSkaev>();
        public DbSet<Laegemiddel> Laegemiddler => Set<Laegemiddel>();
        public DbSet<Ordination> Ordinationer => Set<Ordination>();

        public OrdinationContext (DbContextOptions<OrdinationContext> options)
            : base(options)
        {
        }
    }
}
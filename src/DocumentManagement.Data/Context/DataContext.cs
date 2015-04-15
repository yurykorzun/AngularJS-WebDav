using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using DocumentManagement.Data.DataEntities;

namespace DocumentManagement.Data.Context
{
    public class DataContext : DbContext
    {
        public DataContext() : base("Name=DataContext")
        {
            Configuration.ProxyCreationEnabled = false; 
        }

        public IDbSet<File> File { get; set; }
        public IDbSet<Folder> Folder { get; set; }
        public IDbSet<Lock> Lock { get; set; }
        public IDbSet<LockToken> LockToken { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingEntitySetNameConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
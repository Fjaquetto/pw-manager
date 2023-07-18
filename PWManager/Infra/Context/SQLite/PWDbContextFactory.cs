using PWManager.Infra.Context.SQLite.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWManager.Infra.Context.SQLite
{
    public class PWDbContextFactory : IPWDbContextFactory
    {
        private readonly string _dbPath;

        public PWDbContextFactory(string dbPath)
        {
            _dbPath = dbPath;
        }

        public PWDbContext CreateDbContext()
        {
            return new PWDbContext(_dbPath);
        }
    }
}

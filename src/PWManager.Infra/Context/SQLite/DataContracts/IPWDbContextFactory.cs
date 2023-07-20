using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PWManager.Infra.Context.SQLite;

namespace PWManager.Infra.Context.SQLite.DataContracts
{
    public interface IPWDbContextFactory
    {
        PWDbContext CreateDbContext();
    }
}

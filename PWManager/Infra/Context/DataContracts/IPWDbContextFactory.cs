using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWManager.Infra.Context.DataContracts
{
    public interface IPWDbContextFactory
    {
        PWDbContext CreateDbContext();
    }
}

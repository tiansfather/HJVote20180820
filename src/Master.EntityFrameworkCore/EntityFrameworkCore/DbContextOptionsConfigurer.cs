using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System.Data.Common;

namespace Master.EntityFrameworkCore
{
    public static class DbContextOptionsConfigurer
    {
        public static readonly LoggerFactory MyLoggerFactory
    = new LoggerFactory(new[] { new DebugLoggerProvider((_, __) => true) });
        public static void Configure(
            DbContextOptionsBuilder<MasterDbContext> dbContextOptions, 
            string connectionString
            )
        {
            //dbContextOptions.UseLazyLoadingProxies().UseMySql(connectionString).ConfigureWarnings(warnnngs => warnnngs.Log(CoreEventId.LazyLoadOnDisposedContextWarning)).UseLoggerFactory(MyLoggerFactory);
            dbContextOptions.UseLazyLoadingProxies().UseSqlServer(connectionString,b=>b.UseRowNumberForPaging()).ConfigureWarnings(warnnngs => { warnnngs.Log(CoreEventId.LazyLoadOnDisposedContextWarning); warnnngs.Log(CoreEventId.DetachedLazyLoadingWarning); }).UseLoggerFactory(MyLoggerFactory);
        }

        public static void Configure(DbContextOptionsBuilder<MasterDbContext> dbContextOptions, DbConnection connection)
        {
            dbContextOptions.UseLazyLoadingProxies().UseSqlServer(connection,b => b.UseRowNumberForPaging()).ConfigureWarnings(warnnngs => { warnnngs.Log(CoreEventId.LazyLoadOnDisposedContextWarning); warnnngs.Log(CoreEventId.DetachedLazyLoadingWarning); }).UseLoggerFactory(MyLoggerFactory);
            //builder.UseSqlServer(connection, b => b.UseRowNumberForPaging());
        }
    }
}

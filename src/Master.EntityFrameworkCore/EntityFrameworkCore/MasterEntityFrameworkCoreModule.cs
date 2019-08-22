using Abp.AutoMapper;
using Abp.EntityFramework;
using Abp.EntityFrameworkCore;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Master.EntityFrameworkCore.EntityFinder;
using Abp.Configuration.Startup;
using Abp.Domain.Uow;
using Castle.MicroKernel.Registration;
using Master.MultiTenancy;
using Master.EntityFrameworkCore.Seed;
using Abp.EntityFrameworkCore.Configuration;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;

namespace Master.EntityFrameworkCore
{
    [DependsOn(
        typeof(MasterCoreModule), 
        typeof(AbpEntityFrameworkCoreModule),
        typeof(AbpAutoMapperModule))]
    public class MasterEntityFrameworkCoreModule : AbpModule
    {
        public bool SkipDbSeed { get; set; }
        public override void PreInitialize()
        {
            Configuration.ReplaceService(typeof(IConnectionStringResolver), () =>
            {
                IocManager.IocContainer.Register(
                    Component.For<IConnectionStringResolver, IDbPerTenantConnectionStringResolver>()
                        .ImplementedBy<DbPerTenantConnectionStringResolver>()
                        .LifestyleTransient()
                    );
            });
            //使用自定义的实体查找类代替默认的
            Configuration.ReplaceService<IDbContextEntityFinder, MasterDbContextEntityFinder>();

            Configuration.Modules.AbpEfCore().AddDbContext<MasterDbContext>(options =>
            {
                //此行会减缓执行速度，仅供调试使用
                //options.DbContextOptions.UseLoggerFactory(Mlogger);
                if (options.ExistingConnection != null)
                {
                    DbContextOptionsConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                }
                else
                {
                    DbContextOptionsConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                }
            });
        }
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MasterEntityFrameworkCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            using (var migratorWrapper = IocManager.ResolveAsDisposable<DbMigrator>())
            {
                migratorWrapper.Object.CreateOrMigrateForHost(SeedHelper.SeedHostDb);
            }
            var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
            workManager.Add(IocManager.Resolve<BackUpWorker>());
            //if (!SkipDbSeed)
            //{
            //    SeedHelper.SeedHostDb(IocManager);
            //}
        }
    }
}
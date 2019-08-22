using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Master.Configuration;
using Master.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Master.Web.Startup
{
    [DependsOn(typeof(MasterWebCoreModule))]
    public class MasterWebModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;

        public MasterWebModule(IHostingEnvironment env)
        {
            _appConfiguration = AppConfigurations.Get(env.ContentRootPath, env.EnvironmentName);
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(MasterConsts.ConnectionStringName);

            //Configuration.Navigation.Providers.Add<MasterNavigationProvider>();

            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(
                    typeof(MasterApplicationModule).GetAssembly()
                );

            //Configuration.Auditing.IsEnabled = false;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MasterWebModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            
        }
    }
}
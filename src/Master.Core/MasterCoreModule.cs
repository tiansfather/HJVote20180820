using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Services;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Timing;
using Master.Localization;
using Master.Timing;
using Abp.Configuration.Startup;
using System;
using Master.Configuration;
using Abp.MultiTenancy;
using Master.MultiTenancy;
using Abp.Reflection;
using System.Collections.Generic;
using Abp.Auditing;
using Master.Auditing;

namespace Master
{
    public class MasterCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            IocManager.Register<MasterConfiguration>();

            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            //启用多租户
            Configuration.MultiTenancy.IsEnabled = true;

            //使所有领域服务方法成为工作单元
            Configuration.UnitOfWork.ConventionalUowSelectors.Add(type => typeof(DomainService).IsAssignableFrom(type));

            Configuration.ReplaceService<ITenantStore, TenantStore>(DependencyLifeStyle.Transient);
            //配置缓存2小时
            Configuration.Caching.ConfigureAll(cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromHours(2);
            });

            //Setting提供者
            Configuration.Settings.Providers.Add<MasterSettingProvider>();

            //替换默认的权限检查
            Configuration.ReplaceService<IPermissionChecker, Master.Authentication.PermissionChecker>(DependencyLifeStyle.Transient);
            Configuration.ReplaceService<IPermissionManager, Master.Authentication.PermissionManager>(DependencyLifeStyle.Transient);

            Configuration.ReplaceService<IAuditingStore, AuditingManager>(DependencyLifeStyle.Transient);

            MasterLocalizationConfigurer.Configure(Configuration.Localization);

        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MasterCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {

            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;

        }

    }
}
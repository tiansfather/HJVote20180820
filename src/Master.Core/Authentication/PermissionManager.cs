using Abp;
using Abp.Application.Features;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Dependency;
using Abp.Domain.Services;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Master.Menu;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Master.Authentication
{
    /// <summary>
    /// 替换原有的权限管理类
    /// </summary>
    public class PermissionManager : DomainService, IPermissionManager
    {
        public IAbpSession AbpSession { get; set; }
        private readonly IIocManager _iocManager;
        //private readonly IAuthorizationConfiguration _authorizationConfiguration;
        private readonly IMenuManager _menuManager;
        //private readonly IModuleInfoManager _moduleInfoManager;

        public PermissionManager(
            IIocManager iocManager,
            IMenuManager menuManager
            //IAuthorizationConfiguration authorizationConfiguration,
            //IModuleInfoManager moduleInfoManager
            )
        {
            _iocManager = iocManager;
            //_authorizationConfiguration = authorizationConfiguration;
            _menuManager = menuManager;
            //_moduleInfoManager = moduleInfoManager;

            AbpSession = NullAbpSession.Instance;
        }

        public IList<Permission> GetAllPermissions()
        {
            var permissions = new List<Permission>();
            //加入导航权限
            permissions.AddRange(_menuManager.GetAllMenuPermissions());
            //todo:加入菜单对应的资源权限
            //加入模块权限
            //permissions.AddRange(_moduleInfoManager.GetAllModulePermissions().Result);
            return permissions;
        }

        public IReadOnlyList<Permission> GetAllPermissions(bool tenancyFilter = true)
        {
            using (var featureDependencyContext = _iocManager.ResolveAsDisposable<FeatureDependencyContext>())
            {
                var featureDependencyContextObject = featureDependencyContext.Object;
                return GetAllPermissions()
                    .WhereIf(tenancyFilter, p => p.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide))
                    .Where(p =>
                        p.FeatureDependency == null ||
                        AbpSession.MultiTenancySide == MultiTenancySides.Host ||
                        p.FeatureDependency.IsSatisfied(featureDependencyContextObject)
                    ).ToImmutableList();
            }
        }

        public IReadOnlyList<Permission> GetAllPermissions(MultiTenancySides multiTenancySides)
        {
            using (var featureDependencyContext = _iocManager.ResolveAsDisposable<FeatureDependencyContext>())
            {
                var featureDependencyContextObject = featureDependencyContext.Object;
                return GetAllPermissions()
                    .Where(p => p.MultiTenancySides.HasFlag(multiTenancySides))
                    .Where(p =>
                        p.FeatureDependency == null ||
                        AbpSession.MultiTenancySide == MultiTenancySides.Host ||
                        (p.MultiTenancySides.HasFlag(MultiTenancySides.Host) &&
                         multiTenancySides.HasFlag(MultiTenancySides.Host)) ||
                        p.FeatureDependency.IsSatisfied(featureDependencyContextObject)
                    ).ToImmutableList();
            }
        }

        public Permission GetPermission(string name)
        {
            var permission = GetPermissionOrNull(name);
            //if (permission == null)
            //{
            //    throw new AbpException("There is no permission with name: " + name);
            //}

            return permission;
        }

        public Permission GetPermissionOrNull(string name)
        {
            var permission = GetAllPermissions().Where(o => o.Name == name).FirstOrDefault();
            return permission;
        }
    }
}

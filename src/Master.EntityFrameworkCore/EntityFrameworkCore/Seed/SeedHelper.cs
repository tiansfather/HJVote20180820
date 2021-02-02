using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using Master.Authentication;
using Master.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace Master.EntityFrameworkCore.Seed
{
    public static class SeedHelper
    {
        public static void SeedHostDb(IIocResolver iocResolver)
        {
            WithDbContext<MasterDbContext>(iocResolver, SeedHostDb);
        }

        public static void SeedHostDb(MasterDbContext context)
        {
            //context.Database.Migrate();
            context.SuppressAutoSetTenantId = true;

            CreateHostRoleAndUser(context);


        }


        private static void CreateHostRoleAndUser(MasterDbContext context)
        {
            //所有权限
            List<Permission> allPermissions = null;
            using (var provider = IocManager.Instance.ResolveAsDisposable<IPermissionManager>())
            {
                allPermissions = provider.Object.GetAllPermissions()
                    .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Tenant))
                    .ToList();
            }

            var rolesToBeCreated = new string[] { StaticRoleNames.Host.Admin, StaticRoleNames.Host.GroupManager, StaticRoleNames.Host.MatchManager, StaticRoleNames.Host.ProjectReporter, StaticRoleNames.Host.SubManager,StaticRoleNames.Host.MajorManager, StaticRoleNames.Host.SystemManager, StaticRoleNames.Host.Expert,StaticRoleNames.Host.ProjectViewer };
            foreach(var roleDefinition in rolesToBeCreated)
            {
                var roleName = roleDefinition.Split('|')[0];
                var roleDisplayName= roleDefinition.Split('|')[1];
                var role = context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == roleName);
                if (role == null)
                {
                    role = context.Roles.Add(new Role(null, roleName, roleDisplayName) { IsStatic = true }).Entity;
                    context.SaveChanges();
                }
                //该角色需要有的权限 
                var permissionToBeGranted = allPermissions;
                if (role.Name != StaticRoleNames.Host.Admin.Split('|')[0])
                {
                    //非总管理员用户
                    permissionToBeGranted = permissionToBeGranted.Where(o => o.Name.Contains(role.Name.ToLower()+".")).ToList();
                }
                else
                {
                    //管理用户
                    var adminUser = context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == null && u.UserName == User.AdminUserName);
                    if (adminUser == null)
                    {
                        adminUser = User.CreateHostAdminUser();
                        context.Users.Add(adminUser);
                        context.SaveChanges();
                        // Assign Admin role to admin user
                        context.UserRoles.Add(new UserRole(null, adminUser.Id, role.Id));
                        context.SaveChanges();
                    }
                }
                //该角色已经有的权限
                var grantedPermissions = context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == null && p.RoleId == role.Id)
                .Select(p => p.Name)
                .ToList();

                var newGrantedPermissions = permissionToBeGranted.Where(o => !grantedPermissions.Contains(o.Name));
                if (newGrantedPermissions.Any())
                {
                    context.Permissions.AddRange(
                        newGrantedPermissions.Select(permission => new RolePermissionSetting
                        {
                            TenantId = null,
                            Name = permission.Name,
                            IsGranted = true,
                            RoleId = role.Id
                        })
                    );
                    context.SaveChanges();
                }
            }
            
            
            
        }

        private static void CreateDefaultTenant(MasterDbContext context)
        {
            var defaultTenant = context.Set<Tenant>().IgnoreQueryFilters().FirstOrDefault(t => t.TenancyName == Tenant.DefaultTenantName);
            if (defaultTenant == null)
            {
                defaultTenant = new Tenant(Tenant.DefaultTenantName, Tenant.DefaultTenantName);
                //defaultTenant.ConnectionString = SimpleStringCipher.Instance.Encrypt("Server=localhost; Database=MasterDb_Tenant_"+AbpTenantBase.DefaultTenantName+"; User Id=skynetsoft;password=skynetsoft");

                //var defaultEdition = _context.Editions.IgnoreQueryFilters().FirstOrDefault(e => e.Name == EditionManager.DefaultEditionName);
                //if (defaultEdition != null)
                //{
                //    defaultTenant.EditionId = defaultEdition.Id;
                //}
                context.Set<Tenant>().Add(defaultTenant);
                context.SaveChanges();
            }
        }

        private static void WithDbContext<TDbContext>(IIocResolver iocResolver, Action<TDbContext> contextAction)
            where TDbContext : DbContext
        {
            using (var uowManager = iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
            {
                using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
                {
                    var context = uowManager.Object.Current.GetDbContext<TDbContext>(MultiTenancySides.Host);

                    contextAction(context);

                    uow.Complete();
                }
            }
        }
    }
}

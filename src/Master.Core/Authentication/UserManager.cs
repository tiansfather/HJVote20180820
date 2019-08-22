using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.Runtime.Caching;
using Master.Cache;
using Master.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Master.Majors;

namespace Master.Authentication
{
    public class UserManager :DomainServiceBase<User,long>, ITransientDependency
    {
        private readonly IPermissionManager _permissionManager;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<UserSpeciality> _userSpecialityRepository;
        private readonly IRepository<Speciality> _specialityRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserPermissionSetting> _userPermissionSettingRepository;
        private readonly RoleManager _roleManager;
        public IMultiTenancyConfig MultiTenancy { get; set; }
        public UserManager(
            IPermissionManager permissionManager,
            RoleManager roleManager,
            IRepository<UserRole> userRoleRepository,
            IRepository<Role> roleRepository,
            IRepository<UserSpeciality> userSpecialityRepository,
            IRepository<UserPermissionSetting> userPermissionSettingRepository,
            IRepository<Speciality> specialityRepository
            )
        {
            _permissionManager = permissionManager;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _userSpecialityRepository = userSpecialityRepository;
            _userPermissionSettingRepository = userPermissionSettingRepository;
            _roleManager = roleManager;
            _specialityRepository = specialityRepository;
        }
        public virtual async Task SetRoles(User user, int[] roleIds)
        {
            Repository.EnsureCollectionLoaded(user, o => o.Roles);
            var userRolesIds = user.Roles.Select(o => o.RoleId);
            foreach(var roleId in roleIds.Where(o=>!userRolesIds.Contains(o)))
            {
                var userRole = new UserRole(user.TenantId, user.Id, roleId);
                await _userRoleRepository.InsertAsync(userRole);
            }
            foreach(var roleId in userRolesIds.Where(o => !roleIds.Contains(o)))
            {
                await _userRoleRepository.DeleteAsync(o => o.RoleId == roleId && o.TenantId == user.TenantId && o.UserId == user.Id);
            }
            
        }
        public virtual async Task SetSpecialities(User user,int[] specialityIds)
        {
            Repository.EnsureCollectionLoaded(user, o => o.Specialities);
            var userSpecialitiesIds = user.Specialities.Select(o => o.SpecialityId);
            foreach (var specialityId in specialityIds.Where(o => !userSpecialitiesIds.Contains(o)))
            {
                var userSpeciality = new UserSpeciality(user.TenantId, user.Id, specialityId);
                await _userSpecialityRepository.InsertAsync(userSpeciality);
            }
            foreach (var specialityId in userSpecialitiesIds.Where(o => !specialityIds.Contains(o)))
            {
                await _userSpecialityRepository.DeleteAsync(o => o.SpecialityId == specialityId && o.TenantId == user.TenantId && o.UserId == user.Id);
            }
        }
        /// <summary>
        /// 默认隐藏上帝管理员
        /// </summary>
        /// <returns></returns>
        public override IQueryable<User> GetAll()
        {
            return base.GetAll().Where(o=>o.UserName!="boss");
        }
        public virtual async Task SetPassword(User user,string password)
        {
            user.Password = SimpleStringCipher.Instance.Encrypt(password);
            await Repository.UpdateAsync(user);
        }
        /// <summary>
        /// 通过帐套id，及用户名或手机号获取用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public virtual async Task<User> FindByNameOrPhone(int? tenantId, string usernameOrPhone)
        {
            return await Repository.GetAll().Where(o => o.TenantId == tenantId && (o.UserName == usernameOrPhone || o.PhoneNumber == usernameOrPhone)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 检查用户帐户是否正确
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public virtual Task<bool> CheckPasswordAsync(User user, string password)
        {
            return Task.FromResult(SimpleStringCipher.Instance.Encrypt(password) == user.Password);
        }
        #region 权限角色
        /// <summary>
        /// 获取用户拥有所有权限
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual async Task<IReadOnlyList<Permission>> GetGrantedPermissionsAsync(User user)
        {
            var permissionList = new List<Permission>();

            foreach (var permission in _permissionManager.GetAllPermissions())
            {
                if (await IsGrantedAsync(user.Id, permission))
                {
                    permissionList.Add(permission);
                }
            }

            return permissionList;
        }
        /// <summary>
        /// 判断某用户是否有某权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        public virtual async Task<bool> IsGrantedAsync(long userId, string permissionName)
        {
            return await IsGrantedAsync(
                userId,
                _permissionManager.GetPermission(permissionName)
                );
        }
        public virtual Task<bool> IsGrantedAsync(User user, Permission permission)
        {
            return IsGrantedAsync(user.Id, permission);
        }
        /// <summary>
        /// 判断用户是否有权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public virtual async Task<bool> IsGrantedAsync(long userId, Permission permission)
        {
            if (permission == null)
            {
                return false;
            }
            //判断权限归属是否符合Host,Tenant
            if (!permission.MultiTenancySides.HasFlag(GetCurrentMultiTenancySide()))
            {
                return false;
            }

            //Check for depended features
            //if (permission.FeatureDependency != null && GetCurrentMultiTenancySide() == MultiTenancySides.Tenant)
            //{
            //    FeatureDependencyContext.TenantId = GetCurrentTenantId();

            //    if (!await permission.FeatureDependency.IsSatisfiedAsync(FeatureDependencyContext))
            //    {
            //        return false;
            //    }
            //}

            //Get cached user permissions
            var cacheItem = await GetUserPermissionCacheItemAsync(userId);
            if (cacheItem == null)
            {
                return false;
            }

            //Check for user-specific value
            if (cacheItem.GrantedPermissions.Contains(permission.Name))
            {
                return true;
            }

            if (cacheItem.ProhibitedPermissions.Contains(permission.Name))
            {
                return false;
            }

            //Check for roles
            foreach (var roleId in cacheItem.RoleIds)
            {
                if (await _roleManager.IsGrantedAsync(roleId, permission))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 获取用户所有角色
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<List<Role>> GetRolesAsync(User user)
        {
            var query = from userrole in _userRoleRepository.GetAll()
                        join role in _roleRepository.GetAll() on userrole.RoleId equals role.Id
                        where userrole.UserId==user.Id
                        select role;

            return query.ToListAsync();
        }
        /// <summary>
        /// 获取用户所有专业
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<List<Speciality>> GetSpecialitisAsync(User user)
        {
            var query = from userspeciality in _userSpecialityRepository.GetAll()
                        join speciality in _specialityRepository.GetAll() on userspeciality.SpecialityId equals speciality.Id
                        where userspeciality.UserId == user.Id
                        select speciality;

            return query.ToListAsync();
        }
        /// <summary>
        /// 获取针对用户的权限设置信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public virtual async Task<IList<PermissionGrantInfo>> GetPermissionsAsync(long userId)
        {
            return (await _userPermissionSettingRepository.GetAllListAsync(p => p.UserId == userId))
                .Select(p => new PermissionGrantInfo(p.Name, p.IsGranted))
                .ToList();
        }
        /// <summary>
        /// 获取用户权限缓存
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<UserPermissionCacheItem> GetUserPermissionCacheItemAsync(long userId)
        {
            var cacheKey = userId + "@" + (GetCurrentTenantId() ?? 0);
            return await CacheManager.GetUserPermissionCache().GetAsync(cacheKey, async () =>
            {
                var user = await GetByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }

                var newCacheItem = new UserPermissionCacheItem(userId);

                foreach (var role in await GetRolesAsync(user))
                {
                    newCacheItem.RoleIds.Add(role.Id);
                }

                foreach (var permissionInfo in await GetPermissionsAsync(userId))
                {
                    if (permissionInfo.IsGranted)
                    {
                        newCacheItem.GrantedPermissions.Add(permissionInfo.Name);
                    }
                    else
                    {
                        newCacheItem.ProhibitedPermissions.Add(permissionInfo.Name);
                    }
                }

                return newCacheItem;
            });
        }

        /// <summary>
        /// Sets all granted permissions of a user at once.
        /// Prohibits all other permissions.
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="permissions">Permissions</param>
        public virtual async Task SetGrantedPermissionsAsync(User user, IEnumerable<Permission> permissions)
        {
            var oldPermissions = await GetGrantedPermissionsAsync(user);
            var newPermissions = permissions.ToArray();

            foreach (var permission in oldPermissions.Where(p => !newPermissions.Contains(p)))
            {
                await ProhibitPermissionAsync(user, permission);
            }

            foreach (var permission in newPermissions.Where(p => !oldPermissions.Contains(p)))
            {
                await GrantPermissionAsync(user, permission);
            }
        }

        /// <summary>
        /// Prohibits all permissions for a user.
        /// </summary>
        /// <param name="user">User</param>
        public async Task ProhibitAllPermissionsAsync(User user)
        {
            foreach (var permission in _permissionManager.GetAllPermissions())
            {
                await ProhibitPermissionAsync(user, permission);
            }
        }

        /// <summary>
        /// Resets all permission settings for a user.
        /// It removes all permission settings for the user.
        /// User will have permissions according to his roles.
        /// This method does not prohibit all permissions.
        /// For that, use <see cref="ProhibitAllPermissionsAsync"/>.
        /// </summary>
        /// <param name="user">User</param>
        public async Task ResetAllPermissionsAsync(User user)
        {
            await RemoveAllPermissionSettingsAsync(user);
        }

        

        /// <summary>
        /// Grants a permission for a user if not already granted.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="permission">Permission</param>
        public virtual async Task GrantPermissionAsync(User user, Permission permission)
        {
            await RemovePermissionAsync(user, new PermissionGrantInfo(permission.Name, false));

            if (await IsGrantedAsync(user.Id, permission))
            {
                return;
            }

            await AddPermissionAsync(user, new PermissionGrantInfo(permission.Name, true));
        }

        /// <summary>
        /// Prohibits a permission for a user if it's granted.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="permission">Permission</param>
        public virtual async Task ProhibitPermissionAsync(User user, Permission permission)
        {
            await RemovePermissionAsync(user, new PermissionGrantInfo(permission.Name, true));

            if (!await IsGrantedAsync(user.Id, permission))
            {
                return;
            }

            await AddPermissionAsync(user, new PermissionGrantInfo(permission.Name, false));
        }
        private async Task RemoveAllPermissionSettingsAsync(User user)
        {
            await _userPermissionSettingRepository.DeleteAsync(o => o.UserId == user.Id);
        }
        private async Task AddPermissionAsync(User user, PermissionGrantInfo permissionGrant)
        {
            if (await HasPermissionAsync(user.Id, permissionGrant))
            {
                return;
            }

            await _userPermissionSettingRepository.InsertAsync(
                new UserPermissionSetting
                {
                    TenantId = user.TenantId,
                    UserId = user.Id,
                    Name = permissionGrant.Name,
                    IsGranted = permissionGrant.IsGranted
                });
        }
        private async Task RemovePermissionAsync(User user, PermissionGrantInfo permissionGrant)
        {
            await _userPermissionSettingRepository.DeleteAsync(
                permissionSetting => permissionSetting.UserId == user.Id &&
                                     permissionSetting.Name == permissionGrant.Name &&
                                     permissionSetting.IsGranted == permissionGrant.IsGranted
                );
        }
        private async Task<bool> HasPermissionAsync(long userId, PermissionGrantInfo permissionGrant)
        {
            return await _userPermissionSettingRepository.FirstOrDefaultAsync(
                p => p.UserId == userId &&
                     p.Name == permissionGrant.Name &&
                     p.IsGranted == permissionGrant.IsGranted
                ) != null;
        }
        #endregion
        /// <summary>
        /// 获取当前是Host还是Tenant
        /// </summary>
        /// <returns></returns>
        private MultiTenancySides GetCurrentMultiTenancySide()
        {
            if (UnitOfWorkManager.Current != null)
            {
                return MultiTenancy.IsEnabled && !UnitOfWorkManager.Current.GetTenantId().HasValue
                    ? MultiTenancySides.Host
                    : MultiTenancySides.Tenant;
            }

            return AbpSession.MultiTenancySide;
        }
        private int? GetCurrentTenantId()
        {
            if (UnitOfWorkManager.Current != null)
            {
                return UnitOfWorkManager.Current.GetTenantId();
            }

            return AbpSession.TenantId;
        }
    }
}

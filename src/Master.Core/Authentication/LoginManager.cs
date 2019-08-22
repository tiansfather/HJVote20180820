using Abp;
using Abp.Auditing;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Security;
using Abp.Timing;
using Master.MultiTenancy;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Master.Authentication
{
    /// <summary>
    /// 登录
    /// </summary>
    public class LoginManager : ITransientDependency
    {
        public IClientInfoProvider ClientInfoProvider { get; set; }
        private IUnitOfWorkManager UnitOfWorkManager { get; }
        private IMultiTenancyConfig MultiTenancyConfig { get; }
        private IRepository<Tenant> TenantRepository { get; }
        private IRepository<UserLoginAttempt> UserLoginAttemptRepository { get; }
        private UserManager UserManager { get; }
        public LoginManager(
            IMultiTenancyConfig multiTenancyConfig,
            IRepository<Tenant> tenantRepository,
            IRepository<UserLoginAttempt> userLoginAttemptRepository,
            IUnitOfWorkManager unitOfWorkManager,
            UserManager userManager
            )
        {
            UnitOfWorkManager = unitOfWorkManager;
            UserManager = userManager;
            TenantRepository = tenantRepository;
            UserLoginAttemptRepository = userLoginAttemptRepository;
            MultiTenancyConfig = multiTenancyConfig;

            ClientInfoProvider = NullClientInfoProvider.Instance;
        }

        [UnitOfWork]
        public virtual async Task<LoginResult> LoginAsync(string username, string password, string tenancyName = null)
        {
            var result = await LoginAsyncInternal(username, password, tenancyName);
            await SaveLoginAttempt(result, tenancyName, username);
            return result;
        }

        private async Task<LoginResult> LoginAsyncInternal(string username, string password, string tenancyName)
        {
            if (username.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (password.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(password));
            }

            //Get and check tenant
            Tenant tenant = null;
            using (UnitOfWorkManager.Current.SetTenantId(null))
            {
                if (!MultiTenancyConfig.IsEnabled)
                {
                    tenant = await GetDefaultTenantAsync();
                }
                else if (!string.IsNullOrWhiteSpace(tenancyName))
                {
                    tenant = await TenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
                    if (tenant == null)
                    {
                        return new LoginResult(LoginResultType.InvalidTenancyName);
                    }

                    if (!tenant.IsActive)
                    {
                        return new LoginResult(LoginResultType.TenantIsNotActive, tenant);
                    }
                }
            }

            var tenantId = tenant == null ? (int?)null : tenant.Id;

            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                

                var user = await UserManager.FindByNameOrPhone(tenantId, username);
                if (user == null)
                {
                    return new LoginResult(LoginResultType.InvalidUserName, tenant);
                }
                if (!user.IsActive)
                {
                    return new LoginResult(LoginResultType.UserIsNotActive, tenant);
                }
                if (!await UserManager.CheckPasswordAsync(user, password))
                {
                    return new LoginResult(LoginResultType.InvalidPassword, tenant, user);
                }

                user.LastLoginTime = Clock.Now;

                await UserManager.UpdateAsync(user);

                await UnitOfWorkManager.Current.SaveChangesAsync();

                var principal = await CreateClaimsPrincipal(user);
                return new LoginResult(
                    tenant,
                    user,
                    principal.Identity as ClaimsIdentity
                );
            }

            
        }

        private async Task<ClaimsPrincipal> CreateClaimsPrincipal(User user)
        {            
            var identity = new ClaimsIdentity(new Claim[] {
                new Claim(AbpClaimTypes.UserId,user.Id.ToString()),
            },"Bearer");
            if (user.TenantId.HasValue)
            {
                identity.AddClaim(new Claim(AbpClaimTypes.TenantId, user.TenantId.ToString()));
            }
            var roles = await UserManager.GetRolesAsync(user);
            identity.AddClaim(new Claim(AbpClaimTypes.Role, roles[0].Name));
            var principal = new ClaimsPrincipal(identity);
            return principal;
        }
        /// <summary>
        /// 获取默认账套
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<Tenant> GetDefaultTenantAsync()
        {
            var tenant = await TenantRepository.FirstOrDefaultAsync(t => t.TenancyName == Tenant.DefaultTenantName);
            if (tenant == null)
            {
                throw new AbpException("There should be a 'Default' tenant if multi-tenancy is disabled!");
            }

            return tenant;
        }
        /// <summary>
        /// 记录入登录日志
        /// </summary>
        /// <param name="loginResult"></param>
        /// <param name="tenancyName"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        protected virtual async Task SaveLoginAttempt(LoginResult loginResult, string tenancyName, string userName)
        {
            using (var uow = UnitOfWorkManager.Begin(TransactionScopeOption.Suppress))
            {
                var tenantId = loginResult.Tenant != null ? loginResult.Tenant.Id : (int?)null;
                using (UnitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    var loginAttempt = new UserLoginAttempt
                    {
                        TenantId = tenantId,
                        TenancyName = tenancyName,

                        UserId = loginResult.User != null ? loginResult.User.Id : (long?)null,
                        UserNameOrPhoneNumber = userName,

                        Result = loginResult.Result,

                        BrowserInfo = ClientInfoProvider.BrowserInfo,
                        ClientIpAddress = ClientInfoProvider.ClientIpAddress,
                        ClientName = ClientInfoProvider.ComputerName,
                    };

                    await UserLoginAttemptRepository.InsertAsync(loginAttempt);
                    await UnitOfWorkManager.Current.SaveChangesAsync();

                    await uow.CompleteAsync();
                }
            }
        }
    }
}

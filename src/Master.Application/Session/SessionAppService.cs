using Abp.Auditing;
using Abp.Domain.Repositories;
using Master.Authentication;
using Master.Session.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Master.Session
{
    public class SessionAppService : MasterAppServiceBase, ISessionAppService
    {
        private IRepository<User, long> _userRepository;
        private IRepository<UserRole> _userRoleRepository;
        private IRepository<Role> _roleRepository;

        public SessionAppService(
            IRepository<User, long> userRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<Role> roleRepository

            )
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
        }

        [DisableAuditing]
        public async Task<LoginInformationDto> GetCurrentLoginInformations()
        {
            var output = new LoginInformationDto
            {
                Application = new ApplicationInfoDto
                {
                    Version = AppVersionHelper.Version,
                    ReleaseDate = AppVersionHelper.ReleaseDate,
                    Features = new Dictionary<string, bool>
                    {
                        //{ "SignalR", SignalRFeature.IsAvailable },
                        //{ "SignalR.AspNetCore", SignalRFeature.IsAspNetCore }
                    }
                }
            };

            if (AbpSession.TenantId.HasValue)
            {
                output.Tenant = ObjectMapper.Map<TenantLoginInfoDto>(await GetCurrentTenantAsync());
            }

            if (AbpSession.UserId.HasValue)
            {

                var user = await GetCurrentUserAsync();
                output.User = ObjectMapper.Map<UserLoginInfoDto>(user);
                //获取用户的角色
                var roleNameList = (from userrole in _userRoleRepository.GetAll()
                                    join u in _userRoleRepository.GetAll() on userrole.UserId equals u.Id
                                    join role in _roleRepository.GetAll() on userrole.RoleId equals role.Id
                                    where u.Id == user.Id
                                    select new { role.DisplayName ,role.Name}).ToList();
                output.User.RoleNames = roleNameList.Select(o=>o.Name).ToList();
                output.User.RoleDisplayNames = roleNameList.Select(o => o.DisplayName).ToList();
                //获取用户部门
                //var staff = await _staffRepository.FirstOrDefaultAsync(o => o.UserId == user.Id);
                //if (staff != null)
                //{
                //    var departQuery = from uou in _staffOrganizationUnitRepository.GetAll()
                //                      join s in _staffRepository.GetAll() on uou.StaffId equals s.Id
                //                      join ou in _organizationUnitRepository.GetAll() on uou.OrganizationUnitId equals ou.Id
                //                      where s.Id == staff.Id
                //                      select ou.DisplayName;

                //    output.User.DepartNames = departQuery.ToList();
                //}
            }

            return output;
        }

    }
}

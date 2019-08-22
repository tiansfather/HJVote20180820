using Abp.Auditing;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.UI;
using Abp.Web.Models;
using Master.Authentication;
using Master.Domain;
using Master.Dto;
using Master.Majors;
using Master.Matches;
using Master.Reviews;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Users
{
    [DisableAuditing]
    public class UserAppService:MasterAppServiceBase<User,long>
    {
        public MajorManager MajorManager { get; set; }
        private readonly IRepository<UserRole, int> _userRoleRepository;
        private readonly IRepository<Role, int> _roleRepository;
        private readonly IRepository<MajorExpert, int> _majorExpertRepository;
        private readonly IRepository<UserSpeciality, int> _userSpecialityRepository;
        private readonly IRepository<MajorCharger, int> _majorChargerRepository;
        private readonly IRepository<Major, int> _majorRepository;
        private readonly IRepository<MatchInstance, int> _matchInstanceRepository;
        private readonly IRepository<Speciality, int> _specialityRepository;
        public UserAppService(
            IRepository<UserRole, int> userRoleRepository,
            IRepository<MajorExpert, int> majorExpertRepository,
            IRepository<MajorCharger, int> majorChargerRepository,
            IRepository<Role, int> roleRepository,
            IRepository<Major, int> majorRepository,
            IRepository<UserSpeciality, int> userSpecialityRepository,
            IRepository<MatchInstance, int> matchInstanceRepository,
            IRepository<Speciality, int> specialityRepository
            )
        {
            _userRoleRepository = userRoleRepository;
            _majorExpertRepository = majorExpertRepository;
            _majorChargerRepository = majorChargerRepository;
            _roleRepository = roleRepository;
            _majorRepository = majorRepository;
            _matchInstanceRepository = matchInstanceRepository;
            _userSpecialityRepository = userSpecialityRepository;
            _specialityRepository = specialityRepository;
    }
        
        public async Task<string> GetCurrentToken()
        {
            if (!AbpSession.UserId.HasValue)
            {
                return string.Empty;
            }
            var user = await Manager.GetByIdAsync(AbpSession.UserId.Value);
            return user.GetData<string>("currentToken");
        }
        public override DomainServiceBase<User, long> Manager => base.Manager;
        public async override Task FormSubmit(FormSubmitRequestDto request)
        {
            switch (request.Action)
            {
                case "Add":
                    await DoAdd(request);
                    break;
                case "Edit":
                    await DoEdit(request);
                    break;
            }
        }

        private async Task DoEdit(FormSubmitRequestDto request)
        {
            var manager = Manager as UserManager;

            var userid = Convert.ToInt32(request.Datas["id"]);
            var user = await manager.GetByIdAsync(userid);

            user.Name = request.Datas["name"];
            user.UserName = request.Datas["username"];
            user.IsActive = !string.IsNullOrEmpty(request.Datas["isactive"]);
            if (string.IsNullOrEmpty(request.Datas["OrganizationId"]))
            {
                user.OrganizationId = null;
            }
            else
            {
                user.OrganizationId = int.Parse(request.Datas["OrganizationId"]);
            }
            if ((await Repository.CountAsync(o => o.UserName == user.UserName && o.Id!=user.Id)) > 0)
            {
                throw new UserFriendlyException("相同用户名称已存在");
            }
            if (!string.IsNullOrEmpty(request.Datas["password"]))
            {
                await manager.SetPassword(user, request.Datas["password"]);
            }
            if (!string.IsNullOrEmpty(request.Datas["speciality"]))
            {
                var specialityIds = request.Datas["speciality"].Split(',').Select(o => int.Parse(o));
                await manager.SetSpecialities(user, specialityIds.ToArray());
            }
            await manager.UpdateAsync(user);
            await CurrentUnitOfWork.SaveChangesAsync();
            await manager.SetRoles(user, new int[] { Convert.ToInt32(request.Datas["userrole"]) });
        }

        private async Task DoAdd(FormSubmitRequestDto request)
        {
            var manager = Manager as UserManager;

            if (!request.Datas.ContainsKey("userrole"))
            {
                throw new UserFriendlyException("请选择用户角色");
            }
            var user = await manager.DoAdd(request.Datas);
            if((await Repository.CountAsync(o => o.UserName == user.UserName)) > 0)
            {
                throw new UserFriendlyException("相同用户名称已存在");
            }
            
            await manager.InsertAsync(user);
            if (!string.IsNullOrEmpty(request.Datas["speciality"]))
            {
                var specialityIds = request.Datas["speciality"].Split(',').Select(o => int.Parse(o));
                await manager.SetSpecialities(user, specialityIds.ToArray());
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            await manager.SetPassword(user, request.Datas["password"]);
            
            await manager.SetRoles(user, new int[] { Convert.ToInt32(request.Datas["userrole"]) });
        }

        [DontWrapResult]
        public override async  Task<ResultPageDto> GetPageResult(RequestPageDto request)
        {

            var pageResult = await GetPageResultQueryable(request);

            var data = (await pageResult.Queryable.Include(o=>o.Organization).Include(o=>o.Specialities).ToListAsync())
                .Select(o => {
                    var roles = UserManager.GetRolesAsync(o).Result;
                    var specialities = UserManager.GetSpecialitisAsync(o).Result;
                    return new {o.Id, o.Name, o.UserName, o.IsActive, RoleName = string.Join(",", roles.Select(r => r.DisplayName)),OrganizationName=o.Organization?.BriefName,Specialities=specialities.Select(s=>s.Name) };
                });
                

            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }

        /// <summary>
        /// 针对不同调用页面重写不同的搜索条件
        /// </summary>
        /// <param name="searchKeys"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override Task<IQueryable<User>> BuildSearchQueryAsync(IDictionary<string, string> searchKeys, IQueryable<User> query)
        {
            if (searchKeys.ContainsKey("searchType"))
            {
                var searchType = searchKeys["searchType"];
                switch (searchType)
                {
                    case "BindExpertSelect":
                        return BuildExpertSelectQuery(searchKeys, query);
                    case "BindChargerSelect":
                        return BuildChargerSelectQuery(searchKeys, query);
                }
            }
            if (searchKeys.ContainsKey("specialityId"))
            {
                var specialityId = int.Parse(searchKeys["specialityId"]);
                query = from user in query
                        join userSpeciality in _userSpecialityRepository.GetAll() on user.Id equals userSpeciality.UserId
                        where userSpeciality.SpecialityId == specialityId
                        select user;
            }
            if (searchKeys.ContainsKey("roleId"))
            {
                var roleId = int.Parse(searchKeys["roleId"]);
                query = query.Where(o => o.Roles.Count(r => r.RoleId == roleId) > 0);
            }
            return base.BuildSearchQueryAsync(searchKeys, query);
        }
        /// <summary>
        /// 专家绑定专业选择时的查询条件构建
        /// </summary>
        /// <param name="searchKeys"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private Task<IQueryable<User>> BuildExpertSelectQuery(IDictionary<string, string> searchKeys, IQueryable<User> query)
        {
            //只显示专家并且并未被绑定
            var majorId = Convert.ToInt32(searchKeys["majorId"]);

            var newQuery = from user in query
                           join userrole in _userRoleRepository.GetAll() on user.Id equals userrole.UserId
                           join role in _roleRepository.GetAll() on userrole.RoleId equals role.Id
                           where role.Name == "Expert"
                           where !(from majorexpert in _majorExpertRepository.GetAll()
                                  where majorexpert.MajorId == majorId
                                  select majorexpert.UserId).Contains(user.Id)
                           select user;

            return Task.FromResult(newQuery);
        }
        /// <summary>
        /// 绑定大专业负责人选择时的查询条件构建
        /// </summary>
        /// <param name="searchKeys"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private Task<IQueryable<User>> BuildChargerSelectQuery(IDictionary<string, string> searchKeys, IQueryable<User> query)
        {
            //只显示专家并且并未被绑定
            var majorId = Convert.ToInt32(searchKeys["majorId"]);
            var major =  _majorRepository.Get(majorId);
            var newQuery = from user in query
                           join userrole in _userRoleRepository.GetAll() on user.Id equals userrole.UserId
                           join role in _roleRepository.GetAll() on userrole.RoleId equals role.Id
                           where role.Name == "MajorManager"
                           where !(from majorcharger in _majorChargerRepository.GetAll()
                                   where majorcharger.Major.MatchId==major.MatchId
                                   select majorcharger.UserId).Contains(user.Id)
                           select user;

            return Task.FromResult(newQuery);
        }

        /// <summary>
        /// 将用户列表转换为供评选活动选择专家使用的Dto
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public virtual async Task<List<ReviewExpertDto>> UserToReviewExpertDtos(List<User> users,int matchId)
        {
            Logger.Error(  "Start ExpertTransform1"+ DateTime.Now.ToLongTimeString());
            var specialities = await _specialityRepository.GetAllListAsync();
            //var result= new List<ReviewExpertDto>();
            Logger.Error("Start ExpertTransform2" + DateTime.Now.ToLongTimeString());
            var result = users.Select(o => new ReviewExpertDto
            {
                Id = o.Id,
                Name = o.Name,
                OrganizationDisplayName = o.Organization?.DisplayName,
                Specialities = o.Specialities.Select(s => specialities.Single(sp => sp.Id == s.SpecialityId).Name).ToList()
            }).ToList();
            //Logger.Info(DateTime.Now.ToLongTimeString() + "After Transform");
            return result;
            var expertDtos = new List<ReviewExpertDto>();
            foreach(var user in users)
            {
                var expertDto = new ReviewExpertDto()
                {
                    Id=user.Id,
                    Name=user.Name,
                    OrganizationDisplayName=user.Organization?.DisplayName,
                    Specialities = user.Specialities.Select(o => o.Speciality.Name).ToList()
                };
                //await Repository.EnsurePropertyLoadedAsync(user, o => o.Organization);
                //var expertDto = user.MapTo<ReviewExpertDto>();

                //var query = from major in _majorRepository.GetAll() where major.MatchId==matchId
                //            join majorexpert in _majorExpertRepository.GetAll() on major.Id equals majorexpert.MajorId
                //            where majorexpert.UserId==user.Id
                //            select new MajorExpertDto() { MajorName=major.DisplayName,Rank=majorexpert.MajorExpertRank};

                //expertDto.MajorExperts = await query.ToListAsync();

                //expertDto.Specialities = user.Specialities.Select(o => o.Speciality.Name).ToList();

                expertDtos.Add(expertDto);
            }

            return expertDtos;
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="oriPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public virtual async Task ChangePassword(string oriPassword, string newPassword)
        {
            var user = await Repository.GetAsync(AbpSession.UserId.Value);
            var manager = Manager as UserManager;
            if (!await manager.CheckPasswordAsync(user, oriPassword))
            {
                throw new UserFriendlyException("原密码输入不正确");
            }

            await manager.SetPassword(user, newPassword);
        }
        /// <summary>
        /// 评选活动选择专家数据接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [DontWrapResult]
        [Audited]
        public virtual async Task<object> GetExperts(int matchInstanceId,int? specialityId,string name,string exclude,string all,int? isexclude,int? isinclude,int? organizationId)
        {
            var expertRoleId = (await _roleRepository.SingleAsync(o => o.Name == "Expert")).Id;
            var expertRoleName = StaticRoleNames.Host.Expert.Split('|')[0];
            var matchInstance = await _matchInstanceRepository.GetAsync(matchInstanceId);
            //var userQuery = Repository.GetAll();
            var userQuery = Repository.GetAllIncluding(o=>o.Organization,o=>o.Specialities).Where(o => o.Roles.Count(r => r.RoleId == expertRoleId) > 0);
            //var userQuery= from userrole in _userRoleRepository.GetAll()
            //               join role in _roleRepository.GetAll() on userrole.RoleId equals role.Id
            //               join user in Repository.GetAll() on userrole.UserId equals user.Id
            //               where role.Name== expertRoleName
            //                select user;
            //var userQuery = Repository.GetAll().Where(o=>o.Roles.Count(r=>r.));
            if (!string.IsNullOrEmpty(name))
            {
                userQuery = userQuery.Where(o => o.Name.Contains(name));
            }
            if (organizationId != null)
            {
                userQuery = userQuery.Where(o => o.OrganizationId == organizationId.Value);
            }
            IEnumerable<long> excludeIds=new List<long>();
            if (!string.IsNullOrEmpty(exclude))
            {
                excludeIds = exclude.Split(',').Select(o => Convert.ToInt64(o));
            }
            if (!string.IsNullOrEmpty(all))
            {
                //有all参数的为回避专家选择，all参数表示可供选择的专家范围
                var allIds=all.Split(',').Select(o => Convert.ToInt64(o));
                userQuery = userQuery.Where(o => allIds.Contains(o.Id));
                if ((isexclude.HasValue || isinclude.HasValue) && (!(isexclude.HasValue && isinclude.HasValue)))
                {
                    if (isexclude.HasValue)
                    {
                        //已选择回避
                        userQuery = userQuery.Where(o => excludeIds.Contains(o.Id));
                    }
                    else
                    {
                        //未选择回避
                        userQuery = userQuery.Where(o => !excludeIds.Contains(o.Id));
                    }
                }
            }
            else if (!string.IsNullOrEmpty(exclude))
            {
                //没有all参数有exclude参数的为选择评选专家
                
                userQuery = userQuery.Where(o => !excludeIds.Contains(o.Id));
            }

            //var majorExpertQuery = _majorExpertRepository.GetAll();
            //if (rank != null)
            //{
            //    var majorExpertRank = (MajorExpertRank)rank.Value;
            //    majorExpertQuery = majorExpertQuery.Where(o => o.MajorExpertRank == majorExpertRank);
            //}
            //if (majorId.HasValue)
            //{
            //    var major = await _majorRepository.GetAsync(majorId.Value);
            //    if (subMajorId != null)
            //    {
            //        //如果指定专业小类，则限定此专业小类绑定的专家
            //        var subMajor = await _majorRepository.GetAsync(subMajorId.Value);
            //        var oriSubMajor = await _majorRepository.GetAll().Where(o => o.BriefCode == subMajor.BriefCode && o.MatchId == subMajor.MatchInstance.MatchId).FirstOrDefaultAsync();
            //        if (oriSubMajor != null)
            //        {
            //            majorExpertQuery = majorExpertQuery.Where(o => o.MajorId == oriSubMajor.Id);
            //        }
            //    }
            //    else
            //    {
            //        //未指定专业小类的，限定专业大类下所有专业小类绑定的专家

            //        var oriMajor = await _majorRepository.GetAll().Where(o => o.BriefCode == major.BriefCode && o.MatchId == major.MatchInstance.MatchId).FirstOrDefaultAsync();
            //        if (oriMajor != null)
            //        {
            //            var subMajorIds = (await MajorManager.FindChildrenAsync(oriMajor.MatchId, oriMajor.MatchInstanceId, oriMajor.Id)).Select(o => o.Id);
            //            majorExpertQuery = majorExpertQuery.Where(o => subMajorIds.Contains(o.MajorId));
            //        }
            //    }
            //}

            if (specialityId.HasValue)
            {
                userQuery = userQuery.Where(u => _userSpecialityRepository.GetAll().Where(o => o.SpecialityId==specialityId.Value).Select(o => o.UserId).Contains(u.Id));
            }


            //var query = from user in userQuery
            //            join majorExpert in majorExpertQuery on user.Id equals majorExpert.UserId
            //            select user;
            var users = await userQuery.ToListAsync();
            //var specialities = await _specialityRepository.GetAllListAsync();
            var reviewExpertDtos = users.Select(o =>
            {
                var specialities = UserManager.GetSpecialitisAsync(o).Result;
                return new ReviewExpertDto() { Id = o.Id, Name = o.Name, OrganizationDisplayName = o.Organization?.BriefName, Specialities = specialities.Select(s => s.Name).ToList() };
            });

            //var expertDtos =await UserToReviewExpertDtos(users, matchInstance.MatchId);

            var result = new ResultPageDto()
            {
                code = 0,
                count = users.Count(),
                data = reviewExpertDtos.OrderBy(o => o.Name)
            };
            //var result = new ResultPageDto (){ code = 0, count = users.Count, data = temp.OrderBy(o=>o.Name) };
            return result;
        }
    }
}

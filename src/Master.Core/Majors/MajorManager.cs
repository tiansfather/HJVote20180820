using Abp.Domain.Repositories;
using Abp.UI;
using Master.Authentication;
using Master.Domain;
using Master.Matches;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Majors
{
    public class MajorManager:DomainServiceBase<Major,int>    
    {
        private readonly IRepository<MajorExpert, int> _majorExpertRepository;
        private readonly IRepository<MajorCharger, int> _majorChargerRepository;
        private readonly IRepository<UserRole, int> _userRoleRepository;
        private readonly IRepository<Role, int> _roleRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<MatchInstance, int> _matchInstanceRepository;
        public MajorManager(
            IRepository<MajorExpert, int> majorExpertRepository,
            IRepository<MajorCharger, int> majorChargerRepository,
            IRepository<UserRole, int> userRoleRepository,
            IRepository<Role, int> roleRepository,
            IRepository<User, long> userRepository,
            IRepository<MatchInstance, int> matchInstanceRepository
            )
        {
            _majorExpertRepository = majorExpertRepository;
            _majorChargerRepository = majorChargerRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _matchInstanceRepository = matchInstanceRepository;
        }
        /// <summary>
        /// 专业对应的绑定专家
        /// </summary>
        /// <param name="majorId"></param>
        /// <returns></returns>
        public virtual async Task<List<User>> GetMajorExperts(int majorId)
        {
            var major = await GetByIdAsync(majorId);
            var subMajors = await FindChildrenAsync(major.MatchId, major.MatchInstanceId, major.Id);

            var query = from user in _userRepository.GetAll() 
                        join majorexpert in _majorExpertRepository.GetAll() on user.Id equals majorexpert.UserId
                        where  user.IsActive && majorexpert.MajorId==majorId
                        select user;

            var experts = await query.Include(o => o.Organization).ToListAsync();
            foreach(var subMajor in subMajors)
            {
                experts.AddRange(await GetMajorExperts(subMajor.Id));
            }

            return experts.Distinct().ToList();
        }
        /// <summary>
        /// 大专业对应的负责人
        /// </summary>
        /// <param name="majorId"></param>
        /// <returns></returns>
        public virtual async Task<List<User>> GetMajorChargers(int majorId)
        {
            var major = await GetByIdAsync(majorId);

            var query = from user in _userRepository.GetAll()
                        join majorcharger in _majorChargerRepository.GetAll() on user.Id equals majorcharger.UserId
                        where user.IsActive && majorcharger.MajorId == majorId
                        select user;

            var chargers = await query.Include(o => o.Organization).ToListAsync();

            return chargers.Distinct().ToList();
        }
        public virtual async Task CreateAsync(Major major)
        {
            major.Code = await GetNextChildCodeAsync(major.MatchId,major.MatchInstanceId, major.ParentId);
            await ValidateMajorAsync(major);
            await Repository.InsertAsync(major);
        }

        public override async Task UpdateAsync(Major Major)
        {
            await ValidateMajorAsync(Major);
            await Repository.UpdateAsync(Major);
        }

        public virtual async Task<string> GetNextChildCodeAsync(int?matchId,int? matchInstanceId,int? parentId)
        {
            var lastChild = await GetLastChildOrNullAsync(matchId, matchInstanceId,parentId);
            if (lastChild == null)
            {
                var parentCode = parentId != null ? await GetCodeAsync(parentId.Value) : null;
                return Major.AppendCode(parentCode, Major.CreateCode(1));
            }

            return Major.CalculateNextCode(lastChild.Code);
        }

        public virtual async Task<Major> GetLastChildOrNullAsync(int? matchId, int? matchInstanceId, int? parentId)
        {
            var children = await Repository.GetAllListAsync(o => o.ParentId == parentId && o.MatchId==matchId && o.MatchInstanceId==matchInstanceId);
            return children.OrderBy(c => c.Code).LastOrDefault();
        }

        public virtual async Task<string> GetCodeAsync(int id)
        {
            return (await Repository.GetAsync(id)).Code;
        }

        public override async Task DeleteAsync(IEnumerable<int> ids)
        {
            var ous = await GetListByIdsAsync(ids);
            foreach (var ou in ous)
            {
                await DeleteAsync(ou);
            }
        }

        public override async Task DeleteAsync(Major entity)
        {
            var children = await FindChildrenAsync(entity.MatchId,entity.MatchInstanceId, entity.Id, true);

            foreach (var child in children)
            {
                await Repository.DeleteAsync(child);
            }

            await Repository.DeleteAsync(entity.Id);
        }

        public virtual async Task MoveAsync(int id, int? parentId)
        {
            var major = await Repository.GetAsync(id);
            if (major.ParentId == parentId)
            {
                return;
            }

            //Should find children before Code change
            var children = await FindChildrenAsync(major.MatchId, major.MatchInstanceId,id, true);

            //Store old code of OU
            var oldCode = major.Code;

            //Move OU
            major.Code = await GetNextChildCodeAsync(major.MatchId,major.MatchInstanceId, parentId);
            major.ParentId = parentId;

            await ValidateMajorAsync(major);

            //Update Children Codes
            foreach (var child in children)
            {
                child.Code = Major.AppendCode(major.Code, Major.GetRelativeCode(child.Code, oldCode));
            }
        }

        public async Task<List<Major>> FindChildrenAsync(int? matchId,int? matchInstanceId, int? parentId, bool recursive = false)
        {
            if (!recursive)
            {
                return await Repository.GetAll().Where(ou => ou.ParentId == parentId && ou.MatchId==matchId && ou.MatchInstanceId==matchInstanceId).OrderBy(ou => ou.Sort).ToListAsync();
            }

            if (!parentId.HasValue)
            {
                return await Repository.GetAll().Where(ou => ou.MatchId == matchId && ou.MatchInstanceId == matchInstanceId).OrderBy(ou => ou.Sort).ToListAsync();
            }

            var code = await GetCodeAsync(parentId.Value);

            return await Repository.GetAll().Where(
                ou => ou.Code.StartsWith(code) && ou.Id != parentId.Value && ou.MatchId==matchId && ou.MatchInstanceId==matchInstanceId
            ).OrderBy(ou => ou.Sort).ToListAsync();
        }

        protected virtual async Task ValidateMajorAsync(Major major)
        {
            if(await Repository.CountAsync(o=>o.MatchId==major.MatchId && o.MatchInstanceId==o.MatchInstanceId && o.BriefCode==major.BriefCode && o.Id != major.Id) > 0)
            {
                throw new UserFriendlyException("相同数据编码已存在");
            }

            var siblings = (await FindChildrenAsync(major.MatchId,major.MatchInstanceId, major.ParentId))
                .Where(ou => ou.Id != major.Id)
                .ToList();

            if (siblings.Any(ou => ou.DisplayName == major.DisplayName || ou.BriefName == major.BriefName))
            {
                throw new UserFriendlyException("专业名称或简称重复");
            }
        }
        /// <summary>
        /// 获取某用户在某赛事下的负责大专业
        /// </summary>
        /// <param name="chargerId"></param>
        /// <param name="matchInstanceId"></param>
        /// <returns></returns>
        public async Task<List<Major>> GetChargerMajors(long chargerId,int matchInstanceId)
        {
            var matchInstance = await _matchInstanceRepository.GetAsync(matchInstanceId);
            //获取用户绑定的原始大专业编码
            var oriMajorQuery = from majorcharger in _majorChargerRepository.GetAll()
                        where majorcharger.UserId == chargerId && majorcharger.Major.MatchId==matchInstance.MatchId
                        select majorcharger.Major.BriefCode;

            var newMajorQuery = from major in Repository.GetAll()
                                where major.MatchInstanceId == matchInstanceId && oriMajorQuery.Contains(major.BriefCode)
                                select major;

            return await newMajorQuery.ToListAsync();

        }
    }
}

using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.UI;
using Abp.Web.Models;
using Master.Authentication;
using Master.Dto;
using Master.Matches;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Majors
{
    public class MajorAppService : MasterAppServiceBase<Major, int>
    {
        private readonly IRepository<MajorExpert, int> _majorExpertRepository;
        private readonly IRepository<MajorCharger, int> _majorChargerRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<MatchInstance, int> _matchInstanceRepository;
        public MajorAppService(IRepository<MajorExpert, int> majorExpertRepository,
            IRepository<MajorCharger, int> majorChargerRepository,IRepository<User, long> userRepository, IRepository<MatchInstance, int> matchInstanceRepository)
        {
            _majorExpertRepository = majorExpertRepository;
            _majorChargerRepository = majorChargerRepository;
            _userRepository = userRepository;
            _matchInstanceRepository = matchInstanceRepository;
        }
        public override async Task FormSubmit(FormSubmitRequestDto request)
        {
            switch (request.Action)
            {
                case "Submit":
                    await DoSubmit(request);
                    break;
            }
        }

        private async Task DoSubmit(FormSubmitRequestDto request)
        {
            var matchId = int.Parse(request.Datas["MatchId"]);
            //如果有申报中的赛事实例，不允许修改
            //modi20181008 允许修改
            //if(await _matchInstanceRepository.CountAsync(o=>o.MatchId==matchId && o.MatchInstanceStatus == MatchInstanceStatus.Applying) > 0)
            //{
            //    throw new UserFriendlyException("赛事已在申报中,无法修改");
            //}
            if (request.Datas["ParentId"] == "null")
            {
                request.Datas["ParentId"] = null;
            }
            var manager = Manager as MajorManager;
            Major major = null;
            if (request.Datas["Id"] == "0")
            {
                //添加
                if (request.Datas["IsActive"] == "")
                {
                    request.Datas["IsActive"] = "false";
                }
                major = await manager.DoAdd(request.Datas);
                major.SetData("ExtendData1", request.Datas["ExtendData1"]);
                major.SetData("ExtendData2", request.Datas["ExtendData2"]);
                await manager.CreateAsync(major);
            }
            else
            {
                //不允许设置父级为自身或自身子级
                var id = Convert.ToInt32(request.Datas["Id"]);
                var oriMajor = await Repository.GetAsync(id);//旧专业实体
                int? newParentId=null;
                if (!string.IsNullOrEmpty(request.Datas["ParentId"]))
                {
                    newParentId = int.Parse(request.Datas["ParentId"]);
                }
                //仅当父级变动
                if (oriMajor.ParentId != newParentId)
                {
                    var childIds = (await manager.FindChildrenAsync(oriMajor.MatchId, oriMajor.MatchInstanceId, oriMajor.Id, true)).Select(o => o.Id).ToList();
                    if (oriMajor.Id == newParentId)
                    {
                        throw new UserFriendlyException("不允许设置父级为自己");
                    }else if (newParentId!=null && childIds.Contains(newParentId.Value))
                    {
                        throw new UserFriendlyException("不允许设置父级为子级");
                    }
                    if (newParentId == null)
                    {
                        await manager.MoveAsync(id, null);
                    }
                    else
                    {
                        await manager.MoveAsync(id, newParentId.Value);
                    }
                }
                
                major = await manager.DoEdit(request.Datas, id);
                major.SetData("ExtendData1", request.Datas["ExtendData1"]);
                major.SetData("ExtendData2", request.Datas["ExtendData2"]);

                await manager.UpdateAsync(major);
            }

        }
        /// <summary>
        /// 获取某个赛事的树形专业结构
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="parentId"></param>
        /// <param name="maxLevel"></param>
        /// <returns></returns>
        public virtual async Task<object> GetTreeJson(int? matchId,int? matchInstanceId,int? parentId, int maxLevel = 0)
        {
            var manager = Manager as MajorManager;
            var majors = await manager.FindChildrenAsync(matchId, matchInstanceId, parentId, true);
            if (maxLevel > 0)
            {
                majors = majors.Where(o => o.Code.ToCharArray().Count(c => c == '.') < maxLevel).ToList();
            }
            return majors.Select(o =>
            {
                var dto = o.MapTo<MajorDto>();
                dto.Depth = o.Code.ToCharArray().Count(c => c == '.')+1;
                dto.ExtendData1 = o.GetData<string>("ExtendData1");
                dto.ExtendData2 = o.GetData<string>("ExtendData2");

                return dto;
            }
            );
        }
        /// <summary>
        /// 获取对应专业绑定的所有专家
        /// </summary>
        /// <param name="majorId"></param>
        /// <returns></returns>
        [DontWrapResult]
        public async virtual Task<ResultPageDto> GetExpert(int majorId)
        {
            var manager = Manager as MajorManager;
            var experts = await manager.GetMajorExperts(majorId);

            var result = new ResultPageDto()
            {
                code = 0,
                count = experts.Count,
                data = experts.Select(o => {
                    var majorExpert = _majorExpertRepository.Single(e => e.MajorId == majorId && e.UserId == o.Id);
                    return new { o.Id,o.Name,o.UserName,OrganizationName=o.Organization?.BriefName,majorExpert.MajorExpertRank,MajorExpertId=majorExpert.Id};
                })
            };

            return result;
        }
        /// <summary>
        /// 获取对应大专业绑定的专业负责人
        /// </summary>
        /// <param name="majorId"></param>
        /// <returns></returns>
        [DontWrapResult]
        public async virtual Task<ResultPageDto> GetCharger(int majorId)
        {
            var manager = Manager as MajorManager;
            var chargers = await manager.GetMajorChargers(majorId);

            var result = new ResultPageDto()
            {
                code = 0,
                count = chargers.Count,
                data = chargers.Select(o => {
                    return new { o.Id, o.Name, o.UserName, OrganizationName = o.Organization?.BriefName };
                })
            };

            return result;
        }
        /// <summary>
        /// 绑定专家到专业
        /// </summary>
        /// <param name="majorId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public async virtual Task BindExpertsToMajor(int majorId,int[] userIds)
        {
            foreach(var userId in userIds)
            {
                if(await _majorExpertRepository.CountAsync(o=>o.MajorId==majorId && o.UserId == userId) == 0)
                {
                    var majorExpert = new MajorExpert()
                    {
                        MajorId = majorId,
                        UserId = userId
                    };
                    await _majorExpertRepository.InsertAsync(majorExpert);
                }
            }
        }
        /// <summary>
        /// 从专业移除专家
        /// </summary>
        /// <param name="majorId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public async virtual Task MoveExpertsOutMajor(int majorId, long[] userIds)
        {
            await _majorExpertRepository.DeleteAsync(o => o.MajorId == majorId && userIds.Contains(o.UserId));
        }
        /// <summary>
        /// 设置绑定专家等级
        /// </summary>
        /// <param name="majorExpertId"></param>
        /// <param name="majorExpertRank"></param>
        /// <returns></returns>
        public async virtual Task SetMajorExpertRank(int majorExpertId,MajorExpertRank majorExpertRank)
        {
            var majorExpert = await _majorExpertRepository.GetAll().SingleOrDefaultAsync(o => o.Id == majorExpertId);
            if (majorExpert == null)
            {
                throw new UserFriendlyException("对应数据不存在");
            }
            majorExpert.MajorExpertRank = majorExpertRank;
        }

        /// <summary>
        /// 绑定用户到大专业
        /// </summary>
        /// <param name="majorId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public async virtual Task BindChargersToMajor(int majorId, int[] userIds)
        {
            var major = await Repository.GetAsync(majorId);
            foreach (var userId in userIds)
            {
                if (await _majorChargerRepository.CountAsync(o =>  o.UserId == userId && o.Major.MatchId==major.MatchId) == 0)
                {
                    var majorCharger = new MajorCharger()
                    {
                        MajorId = majorId,
                        UserId = userId
                    };
                    await _majorChargerRepository.InsertAsync(majorCharger);
                }
            }
        }
        /// <summary>
        /// 从大专业移除负责人
        /// </summary>
        /// <param name="majorId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public async virtual Task MoveChargersOutMajor(int majorId, long[] userIds)
        {
            await _majorChargerRepository.DeleteAsync(o => o.MajorId == majorId && userIds.Contains(o.UserId));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Abp.Web.Models;
using Master.Dto;
using Microsoft.EntityFrameworkCore;

namespace Master.Matches
{
    public class MatchAppService : MasterAppServiceBase<Match, int>
    {
        public IRepository<MatchInstance,int> MatchInstanceRepository { get; set; }
        public IRepository<MatchAward,int> MatchAwardRepository { get; set; }
        public virtual async Task Add(string text)
        {
            if (Manager.Repository.Count(o => o.Name == text) > 0)
            {
                throw new UserFriendlyException("相同赛事名称已存在");
            }
            var match = new Match()
            {
                Name = text
            };
            await Manager.InsertAsync(match);
        }
        public virtual async Task Update(int id,string text)
        {
            //是否重复判定
            if(await Repository.CountAsync(o=>o.Id!=id && o.Name == text) > 0)
            {
                throw new UserFriendlyException("相同赛事名称已存在");
            }
            var match = await Manager.GetByIdAsync(id);
            match.Name = text;
        }
        [DontWrapResult]
        public virtual async Task<ResultPageDto> GetAll(RequestPageDto request)
        {
            var matches = await Manager.Repository.GetAll().Select(o => new { o.Id, o.Name }).ToListAsync();

            var result = new ResultPageDto()
            {
                code = 0,
                count = matches.Count,
                data = matches
            };

            return result;
        }
        /// <summary>
        /// 是否赛事正在申报中
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        public virtual async Task<bool> GetIfMatchInApply(int matchId)
        {
            return await MatchInstanceRepository.CountAsync(o => o.MatchId == matchId && o.MatchInstanceStatus == MatchInstanceStatus.Applying) > 0;
        }

        /// <summary>
        /// 赛事及显示设置
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public virtual async Task<object> GetAllWithShowStatus(string keyword)
        {
            var matches = await Manager.GetAll().Include(o => o.MatchInstances)
                .Where(o=>o.MatchInstances.Count(m=>m.MatchInstanceStatus!=MatchInstanceStatus.Draft)>0)
                .WhereIf(!string.IsNullOrEmpty(keyword), o => o.Name.Contains(keyword)).ToListAsync();
            return matches
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.DisplayName,
                    o.IsDisplay,
                    Instances = o.MatchInstances.Select(m => new
                    {
                        m.Id,
                        m.Identifier,
                        m.DisplayScope
                    }).ToList()
                });
        }
        /// <summary>
        /// 赛事及其获奖
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public virtual async Task<object> GetAllAwards(string keyword)
        {
            var awards=await MatchAwardRepository.GetAll().Include(o=>o.Match)
                .WhereIf(!string.IsNullOrEmpty(keyword), o => o.Match.Name.Contains(keyword))
                .Select(o=>new { 
                    o.Id,
                    o.MatchId,
                    MatchName=o.Match.Name,
                    o.AwardName,
                    o.AwardRank
                }).ToListAsync();

            return awards.OrderBy(o=>o.MatchId).ThenBy(o=>o.AwardRank);
        }
        public virtual async Task SubmitAwardField(int awardId,string field,string value)
        {
            var award = await MatchAwardRepository.GetAll().Where(o => o.Id == awardId).FirstOrDefaultAsync();
            switch (field)
            {
                case "awardName":
                    award.AwardName = value;
                    break;
                case "awardRank":
                    if(!int.TryParse(value,out var awardRank))
                    {
                        throw new UserFriendlyException("获奖等级必须为数字");
                    }
                    else
                    {
                        award.AwardRank = awardRank;
                    }
                    break;
            }
        }
        public virtual async Task SubmitMatchShowStatus(dynamic obj)
        {
            foreach(var matchDto in obj)
            {
                var matchId = (int)matchDto.id;
                var match = await Manager.GetAll().Include(o => o.MatchInstances).Where(o => o.Id == matchId).FirstOrDefaultAsync();
                match.DisplayName = matchDto.displayName;
                match.IsDisplay = matchDto.isDisplay;
                foreach(var instanceDto in matchDto.instances)
                {
                    var instanceId = (int)instanceDto.id;
                    var instance = match.MatchInstances.First(o => o.Id == instanceId);
                    instance.DisplayScope = (MatchInstanceDisplayScope)instanceDto.displayScope;
                }
            }
        }

        public override async Task FormSubmit(FormSubmitRequestDto request)
        {
            if (request.Action == "AwardAdd")
            {
                var matchId = int.Parse(request.Datas["MatchId"]);
                var awardName = request.Datas["AwardName"];
                var awardRank = int.Parse(request.Datas["AwardRank"]);
                var award = await MatchAwardRepository.GetAll().Where(o => o.MatchId == matchId && (o.AwardName == awardName || o.AwardRank == awardRank)).FirstOrDefaultAsync();
                if (award != null)
                {
                    throw new UserFriendlyException("相同获奖名称或等级已存在");
                }
                award = new MatchAward()
                {
                    MatchId=matchId,
                    AwardName=awardName,
                    AwardRank=awardRank
                };
                await MatchAwardRepository.InsertAsync(award);
            }
            else
            {
                await base.FormSubmit(request);
            }
            
        }
    }
}

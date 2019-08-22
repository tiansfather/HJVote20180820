using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Abp.Web.Models;
using Master.Dto;
using Microsoft.EntityFrameworkCore;

namespace Master.Matches
{
    public class MatchAppService : MasterAppServiceBase<Match, int>
    {
        public IRepository<MatchInstance,int> MatchInstanceRepository { get; set; }

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

    }
}

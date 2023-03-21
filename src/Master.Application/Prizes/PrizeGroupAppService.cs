using Abp.AutoMapper;
using Abp.Web.Models;
using Master.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Prizes
{
    public class PrizeGroupAppService : MasterAppServiceBase<PrizeGroup, int>
    {
        public PrizeManager PrizeManager { get; set; }

        public virtual async Task<object> GetPrizeGroup(int id)
        {
            var prizeGroup = await Repository.GetAll().Include(o => o.Prizes).FirstOrDefaultAsync(o => o.Id == id);
            var prizeGroupDto = prizeGroup.MapTo<PrizeGroupSubmitDto>();
            prizeGroupDto.SubPrizes = prizeGroup.Prizes.Select(o => new PrizeGroupSubmitPrizeDto() { Id = o.Id, Name = o.PrizeName, Checked = true }).ToList();

            return prizeGroupDto;
        }

        public virtual async Task SubmitPrizeGroup(PrizeGroupSubmitDto prizeGroupSubmitDto)
        {
            PrizeGroup prizeGroup = null;
            if (prizeGroupSubmitDto.Id == 0)
            {
                prizeGroup = prizeGroupSubmitDto.MapTo<PrizeGroup>();
                await Repository.InsertAsync(prizeGroup);
                await CurrentUnitOfWork.SaveChangesAsync();
                foreach (var prizeDto in prizeGroupSubmitDto.SubPrizes.Where(o => o.Checked))
                {
                    var prize = await PrizeManager.GetByIdAsync(prizeDto.Id);
                    prize.PrizeGroupId = prizeGroup.Id;
                    await PrizeManager.UpdateAsync(prize);
                }
            }
            else
            {
                prizeGroup = await Manager.GetByIdAsync(prizeGroupSubmitDto.Id);
                prizeGroupSubmitDto.MapTo(prizeGroup);
                //重新设置赛事所有奖项
                var prizes = await PrizeManager.GetAll().Where(o => o.MatchId == prizeGroup.MatchId).ToListAsync();
                prizes.Where(o => o.PrizeGroupId == prizeGroup.Id).ToList().ForEach(o => o.PrizeGroupId = null);
                foreach (var prizeDto in prizeGroupSubmitDto.SubPrizes.Where(o => o.Checked))
                {
                    prizes.Where(o => o.Id == prizeDto.Id).First().PrizeGroupId = prizeGroup.Id;
                }
                foreach (var prize in prizes)
                {
                    await PrizeManager.UpdateAsync(prize);
                }
            }
        }

        [DontWrapResult]
        public override async Task<ResultPageDto> GetPageResult(RequestPageDto request)
        {
            var pageResult = await GetPageResultQueryable(request);
            pageResult.Queryable = pageResult.Queryable.Include(o => o.Match).Include(o => o.Prizes);
            //只查询针对赛事的奖项
            var data = (await pageResult.Queryable.ToListAsync())
                .Select(o =>
                {
                    return new { MatchName = o.Match.Name, o.Id, o.GroupName, PrizeNames = o.Prizes.Select(p => p.PrizeName), o.Remarks, o.IsActive };
                });

            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }
    }
}
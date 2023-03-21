using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Abp.Web.Models;
using Master.Dto;
using Master.Matches;
using Microsoft.EntityFrameworkCore;

namespace Master.Prizes
{
    public class PrizeAppService : MasterAppServiceBase<Prize, int>
    {
        private readonly IRepository<PrizeSubMajor, int> _prizeSubMajorRepository;
        private readonly IRepository<MatchInstance, int> _matchInstanceRepository;

        public PrizeAppService(
            IRepository<PrizeSubMajor, int> prizeSubMajorRepository, IRepository<MatchInstance, int> matchInstanceRepository)
        {
            _prizeSubMajorRepository = prizeSubMajorRepository;
            _matchInstanceRepository = matchInstanceRepository;
        }

        public virtual async Task<object> GetPrize(int id)
        {
            var prize = await Repository.GetAll().Include(o => o.PrizeSubMajors).FirstOrDefaultAsync(o => o.Id == id);
            var prizeDto = prize.MapTo<PrizeSubmitDto>();
            prizeDto.SubMajors = prize.PrizeSubMajors.Select(o => new PrizeSubmitSubMajorDto() { Id = o.MajorId, Percent = o.Percent, Checked = o.Checked, Name = o.Major.BriefName, Ratio = o.Ratio }).ToList();

            return prizeDto;
        }

        [DontWrapResult]
        public override async Task<ResultPageDto> GetPageResult(RequestPageDto request)
        {
            var pageResult = await GetPageResultQueryable(request);
            pageResult.Queryable = pageResult.Queryable.Include(o => o.Match).Include(o => o.Major);
            //只查询针对赛事的奖项
            var data = (await pageResult.Queryable.ToListAsync())
                .Select(o =>
                {
                    return new { MatchName = o.Match.Name, o.Id, o.PrizeName, MajorName = o.Major.BriefName, o.IsActive, o.PrizeType, o.Remarks };
                });

            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }

        public virtual async Task SubmitPrize(PrizeSubmitDto prizeSubmitDto)
        {
            //如果有申报中的赛事实例，不允许修改
            //modi20181008 允许修改
            //if (await _matchInstanceRepository.CountAsync(o => o.MatchId == prizeSubmitDto.MatchId && o.MatchInstanceStatus == MatchInstanceStatus.Applying) > 0)
            //{
            //    throw new UserFriendlyException("赛事已在申报中,无法修改");
            //}

            Prize prize = null;
            if (prizeSubmitDto.Id == 0)
            {
                prize = prizeSubmitDto.MapTo<Prize>();
                await Repository.InsertAsync(prize);
                await CurrentUnitOfWork.SaveChangesAsync();
                foreach (var subMajor in prizeSubmitDto.SubMajors)
                {
                    var prizeSubMajor = new PrizeSubMajor()
                    {
                        PrizeId = prize.Id,
                        MajorId = subMajor.Id,
                        Percent = subMajor.Percent,
                        Checked = subMajor.Checked,
                        Ratio = subMajor.Ratio
                    };
                    await _prizeSubMajorRepository.InsertAsync(prizeSubMajor);
                }
            }
            else
            {
                prize = await Manager.GetByIdAsync(prizeSubmitDto.Id);
                prizeSubmitDto.MapTo(prize);
                //先删除奖项专业子类信息
                await _prizeSubMajorRepository.DeleteAsync(o => o.PrizeId == prize.Id);
                foreach (var subMajor in prizeSubmitDto.SubMajors)
                {
                    var prizeSubMajor = new PrizeSubMajor()
                    {
                        PrizeId = prize.Id,
                        MajorId = subMajor.Id,
                        Percent = subMajor.Percent,
                        Checked = subMajor.Checked,
                        Ratio = subMajor.Ratio
                    };
                    await _prizeSubMajorRepository.InsertAsync(prizeSubMajor);
                }
            }
        }

        public virtual async Task<object> GetMatchInstancePrizes(int matchInstanceId)
        {
            var manager = Manager as PrizeManager;
            var prizes = await manager.GetAll().Include("PrizeSubMajors.Major").Where(o => o.MatchInstanceId == matchInstanceId).ToListAsync();
            return prizes.Select(o =>
            {
                return new
                {
                    o.Id,
                    o.PrizeName,
                    o.PrizeType,
                    PrizeSubMajors = o.PrizeSubMajors.ToList().Select(p => new
                    {
                        p.Id,
                        p.Major.BriefName
                    })
                };
            });
        }

        public virtual async Task<object> GetMatchPrizes(int matchId)
        {
            var manager = Manager as PrizeManager;
            var prizes = await manager.GetAll().Where(o => o.MatchId == matchId).ToListAsync();
            return prizes.Select(o =>
            {
                return new
                {
                    o.Id,
                    Name = o.PrizeName,
                    o.PrizeGroupId
                };
            });
        }
    }
}
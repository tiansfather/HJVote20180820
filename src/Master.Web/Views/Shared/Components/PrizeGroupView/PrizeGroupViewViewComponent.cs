using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Master.Matches;
using Master.Prizes;
using Master.Web.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master.Web.Views.Shared.Components.PrizeGroupView
{
    public class PrizeGroupViewViewComponent : MasterViewComponent
    {
        public IRepository<Prize> PrizeRepository { get; set; }

        [UnitOfWork]
        public virtual async Task<IViewComponentResult> InvokeAsync(List<PrizeGroup> prizeGroups, MatchInstance matchInstance)
        {
            ViewData["prizeMapDic"] = matchInstance.GetData<Dictionary<int, int>>("PrizeMapDic");
            var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id && o.IsActive).ToListAsync();
            ViewData["prizes"] = prizes;
            return View(prizeGroups);
        }
    }
}
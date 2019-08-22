using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.Web.Controllers
{
    public class ShowController : MasterControllerBase
    {
        /// <summary>
        /// 查看回避专家
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<ActionResult> ExcludeExperts(string ids)
        {
            var idsArr = ids.Split(',').Select(o => Convert.ToInt64(o));
            var experts =await UserRepository.GetAllIncluding(o => o.Organization).Where(o => idsArr.Contains(o.Id))
                .ToListAsync();
            return View(experts);
        }
    }
}
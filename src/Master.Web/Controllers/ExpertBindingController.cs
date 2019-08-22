using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Master.Controllers;
using Master.Matches;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.Web.Controllers
{
    [AbpMvcAuthorize("Menu.systemmanager.expertbinding")]
    public class ExpertBindingController : MasterControllerBase
    {
        private readonly MatchManager _matchManager;
        public ExpertBindingController(MatchManager matchManager)
        {
            _matchManager = matchManager;
        }
        public async Task<IActionResult> Index(int? matchId)
        {
            var allMatches = await _matchManager.GetAll().ToListAsync();
            //if (allMatches.Count == 0)
            //{
            //    return RedirectToAction("Message", "Error", new { msg = "请先添加赛事" });
            //}
            Match currentMatch = null;
            if (matchId.HasValue && allMatches.Count(o => o.Id == matchId.Value) == 1)
            {
                currentMatch = allMatches.Single(o => o.Id == matchId.Value);
            }
            else if(allMatches.Count>0)
            {
                currentMatch = allMatches[0];
            }

            ViewData["matches"] = allMatches;

            return View(currentMatch);
        }
    }
}
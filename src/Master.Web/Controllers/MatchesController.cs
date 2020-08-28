using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Domain.Repositories;
using Master.Controllers;
using Master.Matches;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.Web.Controllers
{
    [AbpMvcAuthorize("Menu.matchmanager.match")]
    public class MatchesController : MasterControllerBase
    {
        private readonly MatchManager _matchManager;
        private readonly MatchInstanceManager _matchInstanceManager;
        public MatchesController(
            MatchManager matchManager,
            MatchInstanceManager matchInstanceManager
            )
        {
            _matchManager = matchManager;
            _matchInstanceManager = matchInstanceManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MatchNames()
        {
            return View();
        }
        public IActionResult ViewSetting()
        {
            return View();
        }
        public async Task<IActionResult> Add()
        {
            var matches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = matches;
            return View();
        }

        public async Task<IActionResult> Edit(string data)
        {
            var matches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = matches;
            var matchInstance = await _matchInstanceManager.GetByIdAsync(Convert.ToInt32(data));
            await _matchInstanceManager.Repository.EnsurePropertyLoadedAsync(matchInstance, o => o.Match);

            return View(matchInstance);
        }
    }
}
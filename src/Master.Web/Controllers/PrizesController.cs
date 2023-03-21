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
    [AbpMvcAuthorize("Menu.matchmanager.prize")]
    public class PrizesController : MasterControllerBase
    {
        private readonly MatchManager _matchManager;

        public PrizesController(MatchManager matchManager)
        {
            _matchManager = matchManager;
        }

        public async Task<IActionResult> Index()
        {
            var allMatches = await _matchManager.GetAll().ToListAsync();

            ViewData["matches"] = allMatches;
            return View();
        }

        public async Task<IActionResult> Groups()
        {
            var allMatches = await _matchManager.GetAll().ToListAsync();

            ViewData["matches"] = allMatches;
            return View();
        }

        public async Task<IActionResult> Submit()
        {
            var allMatches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = allMatches;

            return View();
        }

        public async Task<IActionResult> SubmitGroup()
        {
            var allMatches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = allMatches;

            return View();
        }
    }
}
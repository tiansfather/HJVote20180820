using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Domain.Repositories;
using Master.Authentication;
using Master.Controllers;
using Master.Organizations;
using Master.Prizes;
using Master.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.Web.Controllers
{
    /// <summary>
    /// 分子公司科管页面
    /// </summary>
    [AbpMvcAuthorize]
    public class SubManagerController : MasterControllerBase
    {
        public OrganizationManager OrganizationManager { get; set; }
        public UserManager UserManager { get; set; }
        public PrizeManager PrizeManager { get; set; }
        public ProjectManager ProjectManager { get; set; }
        public async Task<IActionResult> Index()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }

            var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id && o.IsActive).ToListAsync();
            foreach (var prize in prizes)
            {
                foreach (var prizeSubMajor in prize.PrizeSubMajors)
                {
                    await PrizeSubMajorRepository.EnsurePropertyLoadedAsync(prizeSubMajor, o => o.Major);
                }

            }

            var user = await UserManager.GetByIdAsync(AbpSession.UserId.Value);
            ViewBag.OrganizationId = user.OrganizationId;


            ViewData["matchInstance"] = matchInstance;
            return View(prizes);
        }
        public async Task<IActionResult> My()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }

            var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id && o.IsActive).ToListAsync();
            foreach (var prize in prizes)
            {
                foreach (var prizeSubMajor in prize.PrizeSubMajors)
                {
                    await PrizeSubMajorRepository.EnsurePropertyLoadedAsync(prizeSubMajor, o => o.Major);
                }

            }

            var user = await UserManager.GetByIdAsync(AbpSession.UserId.Value);
            ViewBag.OrganizationId = user.OrganizationId;


            ViewData["matchInstance"] = matchInstance;
            return View(prizes);
        }

        public async Task<IActionResult> Verify(int projectId)
        {
            ViewBag.ProjectId = projectId;
            return View();
        }
        public async Task<IActionResult> View(int projectId)
        {

            ViewBag.ProjectId = projectId;
            return View();
        }
    }
}
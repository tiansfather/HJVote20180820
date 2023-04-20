using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Master.Controllers;
using Master.Prizes;
using Master.Projects;
using Master.Reviews;
using Master.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.Web.Controllers
{
    public class GroupManagerController : MasterControllerBase
    {
        public UserAppService UserAppService { get; set; }
        public PrizeManager PrizeManager { get; set; }
        public PrizeGroupManager PrizeGroupManager { get; set; }
        public ProjectManager ProjectManager { get; set; }

        public async Task<IActionResult> Index()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }

            //var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id  && o.IsActive).ToListAsync();
            //foreach (var prize in prizes)
            //{
            //    foreach (var prizeSubMajor in prize.PrizeSubMajors)
            //    {
            //        await PrizeSubMajorRepository.EnsurePropertyLoadedAsync(prizeSubMajor, o => o.Major);
            //    }

            //}
            var prizeGroups = await PrizeGroupManager.GetAll().Include(o => o.Prizes).Where(o => o.MatchId == matchInstance.MatchId && o.IsActive).ToListAsync();

            ViewData["matchInstance"] = matchInstance;
            return View(prizeGroups);
        }

        public async Task<IActionResult> VerifyMajor()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }

            var prizeGroups = await PrizeGroupManager.GetAll().Include(o => o.Prizes).Where(o => o.MatchId == matchInstance.MatchId && o.IsActive).ToListAsync();

            //var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id && o.IsActive).ToListAsync();
            //foreach (var prize in prizes)
            //{
            //    foreach (var prizeSubMajor in prize.PrizeSubMajors)
            //    {
            //        await PrizeSubMajorRepository.EnsurePropertyLoadedAsync(prizeSubMajor, o => o.Major);
            //    }
            //}

            ViewData["matchInstance"] = matchInstance;
            return View(prizeGroups);
        }

        /// <summary>
        /// 所有申报项目
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> My()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }
            var prizeGroups = await PrizeGroupManager.GetAll().Include(o => o.Prizes).Where(o => o.MatchId == matchInstance.MatchId && o.IsActive).ToListAsync();
            //var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id && o.IsActive).ToListAsync();
            //foreach (var prize in prizes)
            //{
            //    foreach (var prizeSubMajor in prize.PrizeSubMajors)
            //    {
            //        await PrizeSubMajorRepository.EnsurePropertyLoadedAsync(prizeSubMajor, o => o.Major);
            //    }
            //}

            ViewData["matchInstance"] = matchInstance;
            return View(prizeGroups);
        }

        /// <summary>
        /// 待评选项目管理
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Projects()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }
            var prizeGroups = await PrizeGroupManager.GetAll().Include(o => o.Prizes).Where(o => o.MatchId == matchInstance.MatchId && o.IsActive).ToListAsync();
            //var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id && o.IsActive).ToListAsync();
            //foreach (var prize in prizes)
            //{
            //    foreach (var prizeSubMajor in prize.PrizeSubMajors)
            //    {
            //        await PrizeSubMajorRepository.EnsurePropertyLoadedAsync(prizeSubMajor, o => o.Major);
            //    }
            //}

            ViewData["matchInstance"] = matchInstance;
            return View(prizeGroups);
        }

        /// <summary>
        /// 项目审批
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Verify(int projectId)
        {
            ViewBag.ProjectId = projectId;
            return View();
        }

        /// <summary>
        /// 项目查看
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public async Task<IActionResult> View(int projectId)
        {
            ViewBag.ProjectId = projectId;
            return View();
        }

        /// <summary>
        /// 导入错误信息查看
        /// </summary>
        public IActionResult ImportErrorView()
        {
            return View();
        }
    }
}
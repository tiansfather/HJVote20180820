using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Domain.Repositories;
using Master.Controllers;
using Master.Majors;
using Master.Organizations;
using Master.Prizes;
using Master.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.Web.Controllers
{
    /// <summary>
    /// 申报人页面
    /// </summary>
    [AbpMvcAuthorize]
    public class ProjectReportController : MasterControllerBase
    {
        public MajorManager MajorManager { get; set; }
        public OrganizationManager OrganizationManager { get; set; }
        public PrizeManager PrizeManager { get; set; }
        public ProjectManager ProjectManager { get; set; }
        /// <summary>
        /// 项目申报主页
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var matchInstance =await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }

            var prizes = await PrizeRepository.GetAll().Include(o=>o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id && o.IsActive).ToListAsync();
            foreach(var prize in prizes)
            {
                foreach(var prizeSubMajor in prize.PrizeSubMajors)
                {
                    await PrizeSubMajorRepository.EnsurePropertyLoadedAsync(prizeSubMajor, o => o.Major);
                }
                
            }
            ViewData["matchInstance"] = matchInstance;
            return View(prizes);
        }
        /// <summary>
        /// 项目申报页
        /// </summary>
        /// <param name="prizeId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public  async Task<IActionResult> Post(int prizeId,int? subMajorId,int? projectId)
        {
            var matchInstance = await GetCurrentMatchInstance();
            var prize = await PrizeManager.GetByIdAsync(prizeId);

            var matchResources = await MatchResourceRepository.GetAll().Where(o =>o.MajorId==prize.MajorId && o.MatchInstanceId == matchInstance.Id && o.MatchResourceStatus==Matches.MatchResourceStatus.Publish).ToListAsync();
            ViewData["matchResources"] = matchResources;
            ViewData["subMajorId"] = subMajorId==null?"":subMajorId.Value.ToString();
            ViewData["matchRemarks"] = matchInstance.Remarks;
            ViewData["prizeRemarks"] = prize.Remarks;

            ViewBag.ProjectId = projectId;
            //第三级专业
            List<string> ThirdLevelMajors = new List<string>();
            if (subMajorId != null)
            {
                var childMajors =await MajorManager.FindChildrenAsync(null, matchInstance.Id, subMajorId);
                ThirdLevelMajors = childMajors.OrderBy(o=>o.Sort).Select(o => o.BriefName).ToList();
            }
            ViewData["ThirdLevelMajors"] = ThirdLevelMajors;
            //所有单位
            var organizations = (await OrganizationManager.FindChildrenAsync(null, true));
            ViewData["organizations"] = organizations;

            return View(prize);
        }
        public async Task<IActionResult> View(int projectId)
        {
           
            ViewBag.ProjectId = projectId;
            return View();
        }
        /// <summary>
        /// 我的申报
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> My()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }

            var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id).ToListAsync();

            ViewData["matchInstance"] = matchInstance;
            return View(prizes);
        }
        /// <summary>
        /// 退回的申报
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Reject()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }

            var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id).ToListAsync();

            ViewData["matchInstance"] = matchInstance;
            return View(prizes);
        }
    }
}
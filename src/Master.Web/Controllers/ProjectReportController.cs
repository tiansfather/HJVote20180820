using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Master.Controllers;
using Master.Majors;
using Master.Organizations;
using Master.Prizes;
using Master.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
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
        public PrizeGroupManager PrizeGroupManager { get; set; }
        public ProjectManager ProjectManager { get; set; }
        public IRepository<ProjectMajorInfo, int> ProjectMajorInfoRepository { get; set; }

        /// <summary>
        /// 项目申报主页
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }
            var prizeGroups = await PrizeGroupManager.GetAll().Include(o => o.Prizes).Where(o => o.MatchId == matchInstance.MatchId && o.IsActive).ToListAsync();
            var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id && o.IsActive).ToListAsync();
            foreach (var prize in prizes)
            {
                foreach (var prizeSubMajor in prize.PrizeSubMajors)
                {
                    await PrizeSubMajorRepository.EnsurePropertyLoadedAsync(prizeSubMajor, o => o.Major);
                }
            }
            ViewData["matchInstance"] = matchInstance;
            ViewData["prizeGroups"] = prizeGroups;
            ViewData["prizeMapDic"] = matchInstance.GetData<Dictionary<int, int>>("PrizeMapDic");
            return View(prizes);
        }

        /// <summary>
        /// 项目申报页
        /// </summary>
        /// <param name="prizeId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Post(int prizeId, string subMajorId, int? projectId)
        {
            var matchInstance = await GetCurrentMatchInstance();
            var prize = await PrizeManager.GetByIdAsync(prizeId);

            var matchResources = await MatchResourceRepository.GetAll().Where(o => o.MajorId == prize.MajorId && o.MatchInstanceId == matchInstance.Id && o.MatchResourceStatus == Matches.MatchResourceStatus.Publish).ToListAsync();
            ViewData["matchResources"] = matchResources;

            ViewData["matchRemarks"] = matchInstance.Remarks;
            ViewData["prizeRemarks"] = prize.Remarks;

            ViewBag.ProjectId = projectId;
            List<int> subMajorIds = new List<int>();
            //第三级专业
            List<string> ThirdLevelMajors = new List<string>();
            if (subMajorId != null)
            {
                subMajorIds = subMajorId.Split(',').Select(int.Parse).ToList();
                var childMajors = new List<Major>();
                foreach (var singleMajorId in subMajorIds)
                {
                    childMajors.AddRange(await MajorManager.FindChildrenAsync(null, matchInstance.Id, singleMajorId));
                }
                ThirdLevelMajors = childMajors.OrderBy(o => o.Sort).Select(o => o.BriefName).ToList();
            }
            //如果有项目id,则查询subMajorId
            if (projectId.HasValue)
            {
                //获取子专业id;
                subMajorIds = await ProjectMajorInfoRepository.GetAll().Where(o => o.ProjectId == projectId && o.MajorId != null).Select(o => o.MajorId.Value).ToListAsync();
            }
            ViewData["ThirdLevelMajors"] = ThirdLevelMajors;
            //所有单位
            var organizations = (await OrganizationManager.FindChildrenAsync(null, true));
            ViewData["organizations"] = organizations;
            ViewData["matchInstance"] = matchInstance;
            ViewData["subMajorIds"] = subMajorIds;
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

            var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeGroup).Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id).ToListAsync();

            var prizeGroups = await PrizeGroupManager.GetAll().Include(o => o.Prizes).Where(o => o.MatchId == matchInstance.MatchId && o.IsActive).ToListAsync();

            ViewData["matchInstance"] = matchInstance;
            return View(prizeGroups);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Domain.Repositories;
using Master.Authentication;
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
    /// 大专业负责人管理页
    /// </summary>
    [AbpMvcAuthorize]
    public class MajorManagerController : MasterControllerBase
    {
        public OrganizationManager OrganizationManager { get; set; }
        public UserManager UserManager { get; set; }
        public PrizeManager PrizeManager { get; set; }
        public ProjectManager ProjectManager { get; set; }
        public MajorManager MajorManager { get; set; }
        public async Task<IActionResult> Index()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }
            //获取当前用户绑定的大专业

            var majors = await MajorManager.GetChargerMajors(AbpSession.UserId.Value, matchInstance.Id);

            ViewData["Majors"] = majors;

            var user = await UserManager.GetByIdAsync(AbpSession.UserId.Value);


            ViewData["matchInstance"] = matchInstance;
            return View();
        }
        public async Task<IActionResult> My()
        {
            var matchInstance = await GetCurrentMatchInstance();

            if (matchInstance == null)
            {
                return Error("请先选择具体赛事");
            }
            //获取当前用户绑定的大专业

            var majors = await MajorManager.GetChargerMajors(AbpSession.UserId.Value, matchInstance.Id);

            ViewData["Majors"] = majors;

            var user = await UserManager.GetByIdAsync(AbpSession.UserId.Value);


            ViewData["matchInstance"] = matchInstance;
            return View();
        }

        public async Task<IActionResult> Verify(int projectId)
        {
            ViewBag.ProjectId = projectId;
            return View();
        }
    }
}
﻿using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Master.Majors;
using Master.Matches;
using Master.Prizes;
using Master.Projects;
using Master.Web.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master.Web.Views.Shared.Components.ProjectView
{
    public class ProjectViewViewComponent : MasterViewComponent
    {
        public MajorManager MajorManager { get; set; }
        public PrizeManager PrizeManager { get; set; }
        public ProjectManager ProjectManager { get; set; }
        public MatchResourceManager MatchResourceManager { get; set; }
        [UnitOfWork]
        public virtual async Task<IViewComponentResult> InvokeAsync( int projectId)
        {
            var project = await ProjectManager.GetByIdAsync(projectId);
            if (project.ProjectSource == ProjectSource.CrossMatch)
            {
                project = project.CrossProject;
            }
            var prize = await PrizeManager.GetByIdAsync(project.PrizeId);
            ProjectManager.Repository.EnsurePropertyLoaded(project, o => o.PrizeSubMajor);

            var matchResources = await MatchResourceManager.Repository.GetAll().Where(o => o.MajorId == prize.MajorId && o.MatchInstanceId == project.MatchInstanceId && o.MatchResourceStatus == Matches.MatchResourceStatus.Publish).ToListAsync();
            ViewData["matchResources"] = matchResources;
            ViewData["subMajorId"] = project.PrizeSubMajor == null ? "" : project.PrizeSubMajor.MajorId.ToString();
            //第三级专业
            List<string> ThirdLevelMajors = new List<string>();
            if (project.PrizeSubMajor != null)
            {
                var childMajors = await MajorManager.FindChildrenAsync(null, project.MatchInstanceId, project.PrizeSubMajor.MajorId);
                ThirdLevelMajors = childMajors.OrderBy(o => o.Sort).Select(o => o.BriefName).ToList();
            }
            ViewData["ThirdLevelMajors"] = ThirdLevelMajors;
            ViewData["ProjectId"] = project.Id;
            return View(prize);
        }
    }
}

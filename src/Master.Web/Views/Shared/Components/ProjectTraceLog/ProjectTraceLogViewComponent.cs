using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Master.Projects;
using Master.Web.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master.Projects;
using Abp.Domain.Entities;

namespace Master.Web.Views.Shared.Components.ProjectTraceLog
{
    public class ProjectTraceLogViewComponent : MasterViewComponent
    {
        public ProjectManager ProjectManager { get; set; }
        public IRepository<Master.Projects.ProjectTraceLog,int> ProjectTraceLogRepository { get; set; }
        [UnitOfWork]
        public virtual async Task<IViewComponentResult> InvokeAsync(int projectId)
        {
            var project = await ProjectManager.Repository.GetAllIncluding(o => o.ProjectTraceLogs).Where(o => o.Id == projectId).SingleAsync();
            if (project.ProjectSource == ProjectSource.CrossMatch)
            {
                projectId = project.CrossProjectId.Value;
                project = await ProjectManager.Repository.GetAllIncluding(o => o.ProjectTraceLogs).Where(o => o.Id == projectId).SingleOrDefaultAsync();
            }
            foreach (var traceLog in project.ProjectTraceLogs)
            {
                await ProjectTraceLogRepository.EnsurePropertyLoadedAsync(traceLog, o => o.CreatorUser);
            }
            return View(project.ProjectTraceLogs.ToList());
        }
    }
}

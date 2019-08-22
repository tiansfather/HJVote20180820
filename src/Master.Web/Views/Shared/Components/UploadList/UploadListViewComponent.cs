using Abp.Domain.Entities;
using Master.Majors;
using Master.Matches;
using Master.Web.Components;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master.Web.Views.Shared.Components.UploadList
{
    public class UploadListViewComponent : MasterViewComponent
    {
        public MajorManager MajorManager { get; set; }
        public async Task<IViewComponentResult> InvokeAsync(MatchResource matchResource, string subMajorId,bool viewMode)
        {
            List<MatchResourceUploadList> uploadList = new List<MatchResourceUploadList>();
            try
            {
                uploadList = matchResource.GetData<List<MatchResourceUploadList>>("Datas");
            }
            catch
            {

            }
            ViewData["subMajorId"] = subMajorId;
            var subMajorName = "基本信息";
            if (!string.IsNullOrEmpty(subMajorId))
            {
                subMajorName = "专业"+(await MajorManager.GetByIdAsync(int.Parse(subMajorId))).BriefName;
            }
            ViewData["subMajorName"] = subMajorName;

            var viewName = "Default";
            if (viewMode)
            {
                viewName = "View";
            }
            return View(viewName,uploadList);
        }
    }
}

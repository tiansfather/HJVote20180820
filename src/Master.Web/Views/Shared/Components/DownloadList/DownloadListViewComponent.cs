using Abp.Domain.Entities;
using Master.Matches;
using Master.Web.Components;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master.Web.Views.Shared.Components.DownloadList
{
    public class DownloadListViewComponent : MasterViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(MatchResource matchResource,int location)
        {
            List<MatchResourceDownloadList> downloadList = new List<MatchResourceDownloadList>();
            try
            {
                downloadList = matchResource.GetData<List<MatchResourceDownloadList>>("Datas")
                .Where(o => o.FileLocation == location)
                .OrderBy(o => o.Sort)
                .ToList();
            }
            catch
            {

            }

            return View(downloadList);
        }
    }
}

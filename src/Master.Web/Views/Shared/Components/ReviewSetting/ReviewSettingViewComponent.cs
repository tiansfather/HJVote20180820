using Master.Web.Components;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master.Web.Views.Shared.Components.ReviewSetting
{
    public class ReviewSettingViewComponent : MasterViewComponent
    {
        public virtual async Task<IViewComponentResult> InvokeAsync(int reviewRoundId)
        {
            ViewBag.ReviewRoundId = reviewRoundId;
            return View();
        }
    }
}

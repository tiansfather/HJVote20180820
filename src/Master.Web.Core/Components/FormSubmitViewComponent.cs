using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Master.Web.Components
{
    public class FormSubmitViewComponent : MasterViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(FormSubmitViewParam param)
        {

            return Task.FromResult(View(param) as IViewComponentResult);
        }
    }

    public class FormSubmitViewParam
    {
        public string ModuleKey { get; set; }
        public string ButtonKey { get; set; }
        public string Callback { get; set; }
    }
}

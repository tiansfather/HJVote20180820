using Abp.AutoMapper;
using Master.Module;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Master.Web.Components
{
    public class FormItemViewComponent : MasterViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(FormItemViewParam param)
        {


            //获取对应的视图
            var viewname = param.FormItem.FormItemType.ToString();

            return View(viewname, param);
        }

    }

    
    public class FormItemViewParam
    {
        public FormItem FormItem { get; set; }
        public object Value { get; set; }
    }
}

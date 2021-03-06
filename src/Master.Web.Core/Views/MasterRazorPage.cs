﻿using Abp.AspNetCore.Mvc.Views;
using Abp.Dependency;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Master.Web.Views
{
    public abstract class MasterRazorPage<TModel> : AbpRazorPage<TModel>
    {
        [RazorInject]
        public IAbpSession AbpSession { get; set; }
        [RazorInject]
        public IIocManager IocManager { get; set; }
        protected MasterRazorPage()
        {
            LocalizationSourceName = MasterConsts.LocalizationSourceName;
        }
    }
}

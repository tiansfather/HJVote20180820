using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Domain.Repositories;
using Master.Controllers;
using Master.Notices;
using Microsoft.AspNetCore.Mvc;

namespace Master.Web.Controllers
{
    public class NoticesController : MasterControllerBase
    {
        public IRepository<Notice, int> NoticeRepository { get; set; }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Post()
        {
            return View();
        }

        /// <summary>
        /// 查看公告
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> View(int id)
        {
            var notice = await NoticeRepository.GetAsync(id);
            return View(notice);
        }
    }
}
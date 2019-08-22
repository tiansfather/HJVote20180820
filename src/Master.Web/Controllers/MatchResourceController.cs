using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Master.Controllers;
using Master.Matches;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.Web.Controllers
{
    public class MatchResourceController : MasterControllerBase
    {
        private readonly MatchManager _matchManager;
        public MatchResourceController(
            MatchManager matchManager
            )
        {
            _matchManager = matchManager;
        }
        #region 表单设计
        [AbpMvcAuthorize("Menu.matchmanager.formdesign")]
        public async Task<IActionResult> FormDesign()
        {
            var matches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = matches;

            return View();
        }
        [AbpMvcAuthorize("Menu.matchmanager.formdesign")]
        public async Task<IActionResult> SubmitFormDesign()
        {
            var matches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = matches;

            return View();
        }
        #endregion

        #region 上传清单
        [AbpMvcAuthorize("Menu.matchmanager.uploadlist")]
        public async Task<IActionResult> UploadList()
        {
            var matches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = matches;

            return View();
        }
        [AbpMvcAuthorize("Menu.matchmanager.uploadlist")]
        public async Task<IActionResult> SubmitUploadList()
        {
            var matches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = matches;

            return View();
        }
        #endregion

        #region 下载列表
        [AbpMvcAuthorize("Menu.matchmanager.downloadlist")]
        public async Task<IActionResult> DownLoadList()
        {
            var matches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = matches;

            return View();
        }
        [AbpMvcAuthorize("Menu.matchmanager.downloadlist")]
        public async Task<IActionResult> SubmitDownloadList()
        {
            var matches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = matches;

            return View();
        }
        #endregion

        #region 评分表
        [AbpMvcAuthorize("Menu.matchmanager.ratingtable")]
        public async Task<IActionResult> RateTable()
        {
            var matches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = matches;

            return View();
        }
        [AbpMvcAuthorize("Menu.matchmanager.ratingtable")]
        public async Task<IActionResult> SubmitRateTable()
        {
            var matches = await _matchManager.GetAll().ToListAsync();
            ViewData["matches"] = matches;

            return View();
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master.Controllers;
using Master.Majors;
using Master.Web.Models.Select;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.Web.Controllers
{
    public class SelectController : MasterControllerBase
    {
        public SpecialityManager SpecialityManager { get; set; }
        public MajorManager MajorManager { get; set; }
        public IActionResult SelUser(SelectFormViewModel model)
        {
            return View(model);
        }
        /// <summary>
        /// 评选活动选入专家
        /// </summary>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        public async Task<IActionResult> SelExpert(int reviewId)
        {
            var review =await ReviewRepository.GetAllIncluding(o => o.Major).Where(o => o.Id == reviewId).SingleAsync();
            //List<Major> subMajors=new List<Major>();
            //if (review.SubMajorId == null)
            //{
            //    //如果未指定专业小类,需要列出所有专业小类供查询
            //    subMajors = await MajorManager.FindChildrenAsync(null, review.MatchInstanceId, review.MajorId);
            //}
            //else
            //{
            //    ViewBag.SubMajor = await MajorManager.GetByIdAsync(review.SubMajorId.Value);
            //}
            //ViewBag.SubMajors = subMajors;
            var specialities = await SpecialityManager.GetAll().OrderBy(o => o.Name).ToListAsync();
            ViewBag.Specialities = specialities;
            ViewBag.Exclude = Request.Cookies["excludeExperts"];
            return View(review);
        }
        public async Task<IActionResult> SelExcludeExpert(int reviewId)
        {
            var review = await ReviewRepository.GetAllIncluding(o => o.Major).Where(o => o.Id == reviewId).SingleAsync();
            List<Major> subMajors = new List<Major>();
            if (review.SubMajorId == null)
            {
                //如果未指定专业小类,需要列出所有专业小类供查询
                subMajors = await MajorManager.FindChildrenAsync(null, review.MatchInstanceId, review.MajorId);
            }
            else
            {
                ViewBag.SubMajor = await MajorManager.GetByIdAsync(review.SubMajorId.Value);
            }
            ViewBag.SubMajors = subMajors;
            ViewBag.Exclude = Request.Cookies["excludeExperts"];
            ViewBag.AllExpertIds= Request.Cookies["allExpertIds"];
            var specialities = await SpecialityManager.GetAll().OrderBy(o => o.Name).ToListAsync();
            ViewBag.Specialities = specialities;
            return View(review);
        }
        public async Task<IActionResult> SelProject(int reviewId)
        {
            var review = await ReviewRepository.GetAllIncluding(o => o.Major).Where(o => o.Id == reviewId).SingleAsync();
            List<Major> subMajors = new List<Major>();
            if (review.SubMajorId == null)
            {
                //如果未指定专业小类,需要列出所有专业小类供查询
                subMajors = await MajorManager.FindChildrenAsync(null, review.MatchInstanceId, review.MajorId);
            }
            else
            {
                ViewBag.SubMajor = await MajorManager.GetByIdAsync(review.SubMajorId.Value);
            }
            ViewBag.SubMajors = subMajors;
            ViewBag.Exclude = Request.Cookies["excludeProjects"];
            return View(review);
        }
    }
}
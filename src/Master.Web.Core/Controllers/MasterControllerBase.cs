using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using Abp.Web.Models;
using Abp.Web.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AutoMapper;
using Master.Matches;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Master.Prizes;
using Master.Reviews;
using Master.Authentication;
using Master.Projects;
using Master.Organizations;

namespace Master.Controllers
{
    public abstract class MasterControllerBase : AbpController
    {
        public  IRepository<MatchInstance, int> MatchInstanceRepository { get; set; }
        public IRepository<MatchResource,int> MatchResourceRepository { get; set; }
        public IRepository<Prize,int> PrizeRepository { get; set; }
        public IRepository<PrizeSubMajor,int> PrizeSubMajorRepository { get; set; }
        public IRepository<Review,int> ReviewRepository { get; set; }
        public IRepository<User,long> UserRepository { get; set; }
        public IRepository<Project,int> ProjectRepository { get; set; }
        public IRepository<ReviewRound,int> ReviewRoundRepository { get; set; }
        public IRepository<Organization,int> OrganizationRepository { get; set; }
        public IRepository<Role,int> RoleRepository { get; set; }
        protected MasterControllerBase()
        {
            LocalizationSourceName = MasterConsts.LocalizationSourceName;
        }


        [HttpGet]
        public virtual IActionResult Error(string msg)
        {
            var vm = new ErrorViewModel()
            {
                ErrorInfo = new ErrorInfo(msg)
            };
            ViewData["msg"] = msg;
            return View("Error",vm);
        }
        /// <summary>
        /// 获取当前操作中的赛事实例
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public virtual async Task<MatchInstance> GetCurrentMatchInstance()
        {
            MatchInstance matchInstance = null;
            var matchIntanceId = Request.Cookies["matchInstanceId"];
            if (!string.IsNullOrEmpty(matchIntanceId))
            {
                 matchInstance = await MatchInstanceRepository.GetAll().Include(o => o.Match).Where(o => o.Id == int.Parse(matchIntanceId)).SingleOrDefaultAsync();
            }
            return matchInstance;
        }
        /// <summary>
        /// 获取当前操作中的评审
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public virtual async Task<Review> GetCurrentReview()
        {
            Review review = null;
            var reviewId = Request.Cookies["reviewId"];
            if (!string.IsNullOrEmpty(reviewId))
            {
                review = await ReviewRepository.GetAll().Include(o => o.ReviewRounds).Where(o => o.Id == int.Parse(reviewId)).SingleOrDefaultAsync();
            }
            return review;
        }
    }

    
}

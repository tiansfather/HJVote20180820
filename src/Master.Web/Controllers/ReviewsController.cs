using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Master.Controllers;
using Master.Majors;
using Master.Prizes;
using Master.Projects;
using Master.Reviews;
using Master.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.Web.Controllers
{
    [AbpMvcAuthorize]
    public class ReviewsController : MasterControllerBase
    {
        public ProjectAppService ProjectAppService { get; set; }
        public ReviewAppService ReviewAppService { get; set; }
        public UserAppService UserAppService { get; set; }
        public PrizeManager PrizeManager { get; set; }
        public ProjectManager ProjectManager { get; set; }
        public ReviewManager ReviewManager { get; set; }
        public IRepository<Speciality, int> SpecialityRepository { get; set; }

        #region 评选活动
        /// <summary>
        /// 评选活动准备
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            var matchInstance = await GetCurrentMatchInstance();
            ViewData["matchInstance"] = matchInstance;
            return View();
        }
        /// <summary>
        /// 评选选择专业及初评终评
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ReviewPrizeChoose()
        {
            var matchInstance = await GetCurrentMatchInstance();
            ViewData["matchInstance"] = matchInstance;


            return View();
        }
        /// <summary>
        /// 评选活动详情
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<IActionResult> ReviewEdit(int data)
        {
            var matchInstance = await GetCurrentMatchInstance();
            var review = await ReviewRepository.GetAllIncluding(o => o.ReviewRounds).SingleOrDefaultAsync(o => o.Id == data);
            //获取对应的专家
            var expertIds = review.ReviewExperts.Select(o => o.Id);
            var expertUsers = await UserRepository.GetAll()
                .Include(o=>o.Organization)
                .Include(o=>o.Specialities)
                .Where(o => expertIds.Contains(o.Id)).ToListAsync();
            //Logger.Error("Before ExpertTransform:" + DateTime.Now.ToLongTimeString());
            var specialities = await SpecialityRepository.GetAllListAsync();
            var reviewExpertDtos = expertUsers.Select(o => new ReviewExpertDto
            {
                Id = o.Id,
                Name = o.Name,
                OrganizationDisplayName = o.Organization?.DisplayName,
                Specialities = o.Specialities.Select(s => specialities.Single(sp => sp.Id == s.SpecialityId).Name).ToList()
            });
            
            //var reviewExpertDtos = await UserAppService.UserToReviewExpertDtos(expertUsers,matchInstance.MatchId);
            //Logger.Error("After ExpertTransform:" + DateTime.Now.ToLongTimeString());
            ViewBag.Experts = reviewExpertDtos;
            //获取对应的项目
            var reviewProjects = review.ReviewProjects.OrderBy(o => o.Sort).ToList() ;
            //Logger.Error("Before ProjectTransform:" + DateTime.Now.ToLongTimeString());
            Logger.Error("0:" + DateTime.Now.ToString("HH:mm:ss:fff"));
            ViewBag.Projects =await ProjectAppService.ProjectToReviewProjectDtos(reviewProjects);
            //Logger.Error("After ProjectTransform:" + DateTime.Now.ToLongTimeString());
            ViewData["MatchInstance"] = matchInstance;
            return View(review);
        }
        #endregion

        #region 评审管理
        /// <summary>
        /// 评选活动管理
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Manager()
        {
            var matchInstance = await GetCurrentMatchInstance();
            ViewData["matchInstance"] = matchInstance;
            return View();
        }
        /// <summary>
        /// 提交评审及修改评审活动页
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ReviewRoundSubmit(int? data)
        {
            var matchInstance = await GetCurrentMatchInstance();
            ViewData["matchInstance"] = matchInstance;
            //获取当前赛事下的所有没有建立评审轮次的评选活动
            var reviews = await ReviewRepository.GetAll().Where(o => o.MatchInstanceId == matchInstance.Id && o.ReviewRounds.Count==0).OrderBy(o=>o.ReviewName).ToListAsync();
            ViewBag.Reviews = reviews;
            return View();
        }
        /// <summary>
        /// 建立下一轮或补充评审
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ReviewRoundNext(int reviewID, string projectIds, string reviewType)
        {
            var matchInstance = await GetCurrentMatchInstance();
            ViewData["matchInstance"] = matchInstance;

            var review = await ReviewRepository.GetAllIncluding(o => o.ReviewRounds).Where(o => o.Id == reviewID).SingleAsync();
            ViewData["reviewType"] = reviewType;
            ViewData["projectIds"] = projectIds;

            return View(review);
        }
        /// <summary>
        /// 评审中页面
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<IActionResult> ReviewRoundViewing(int data)
        {
            var matchInstance = await GetCurrentMatchInstance();
            ViewData["matchInstance"] = matchInstance;

            var reviewRound = await ReviewRoundRepository.GetAllIncluding(o => o.Review).Where(o => o.Id == data).SingleAsync();
            var expertIds = reviewRound.Review.ReviewExperts.Select(o => o.Id);
            var experts = await UserRepository.GetAll().Where(o => expertIds.Contains(o.Id)).OrderBy(o => o.Name).ToListAsync();
            ViewData["Experts"] = experts;
            return View(reviewRound);
        }
        /// <summary>
        /// 已结束评审查看
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<IActionResult> ReviewRoundViewFinish(int data)
        {
            var matchInstance = await GetCurrentMatchInstance();
            ViewData["matchInstance"] = matchInstance;

            var reviewRound = await ReviewRoundRepository.GetAll().Include(o => o.Review).ThenInclude(o=>o.ReviewRounds).Where(o => o.Id == data).SingleAsync();

            return View(reviewRound);
        }
        /// <summary>
        /// 评选总览页
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<IActionResult> Summary(int data)
        {
            var review = await ReviewRepository.GetAllIncluding(o => o.ReviewRounds).Where(o => o.Id == data).SingleAsync();
            return View(review);
        }
        /// <summary>
        /// 评审项目展示页
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Show(int projectId)
        {
            var review = await GetCurrentReview();

            ViewData["Review"] = review;

            ViewBag.Sort = review.ReviewProjects.Single(o => o.Id == projectId).Sort;

            ViewBag.ProjectId = projectId;
            ViewBag.ReviewMethod = review.CurrentReviewRound.ReviewMethod;
            return View();
        }
        /// <summary>
        /// 项目的专家打分明细
        /// </summary>
        /// <param name="reviewRoundId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProjectReviewDetail(int reviewRoundId,int projectId)
        {
            var reviewDetail = await ReviewRoundRepository.GetAsync(reviewRoundId);
            var expertIds = reviewDetail.Review.ReviewExperts.Select(o => o.Id);
            var experts = await UserRepository.GetAll().Include(o=>o.Organization).Where(o => expertIds.Contains(o.Id)).ToListAsync();
            ViewData["Experts"] = experts;

            
            var reviewProject = reviewDetail.Review.ReviewProjects.Single(o => o.Id == projectId);
            var projectDetails = reviewDetail.ExpertReviewDetails.SelectMany(o => o.ProjectReviewDetails).Where(o => o.ProjectId == projectId).ToList();
            //如果是第一轮则加上基础分
            if (reviewProject.BaseScore != null && reviewDetail.Round == 1 && reviewDetail.Turn == 1 && reviewDetail.ReviewMethod != ReviewMethod.Vote)
            {
                ViewData["basescore"] = reviewProject.BaseScore;
            }
            else
            {
                ViewData["basescore"] = "";
            }
            ViewData["reviewDetail"] = reviewDetail;
            ViewData["projectId"] = projectId;
            return View(projectDetails);
        }
        /// <summary>
        /// 专家评分表显示
        /// </summary>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        public async Task<IActionResult> RateTable(int reviewId)
        {
            var review = await ReviewRepository.GetAsync(reviewId);
            var rateTable = await ReviewManager.GetRateTable(review);

            return View(rateTable);
        }
        /// <summary>
        /// 评分表显示
        /// </summary>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        public async Task<IActionResult> RateTable2(int reviewId)
        {
            var review = await ReviewRepository.GetAsync(reviewId);
            ViewBag.MajorName = review.ReviewMajorName;
            var rateTable = await ReviewManager.GetRateTable(review);

            return View(rateTable);
        }
        public async Task<IActionResult> RateTableView(int reviewRoundId,int projectId,long expertId)
        {
            var reviewDetail = await ReviewRoundRepository.GetAsync(reviewRoundId);
            var review = reviewDetail.Review;

            var rateTable = await ReviewManager.GetRateTable(review);

            var projectReviewDetail=reviewDetail.ExpertReviewDetails.Single(o => o.ExpertID == expertId).ProjectReviewDetails.Single(o => o.ProjectId == projectId);
            //打分详情
            ViewData["projectReviewDetail"] = projectReviewDetail;

            return View(rateTable);
        }
        /// <summary>
        /// 全屏公示
        /// </summary>
        /// <param name="reviewRoundId"></param>
        /// <returns></returns>
        public async Task<IActionResult> FullView(int reviewRoundId)
        {
            var matchInstance = await GetCurrentMatchInstance();
            ViewData["matchInstance"] = matchInstance;
            var reviewDetail = await ReviewRoundRepository.GetAllIncluding(o => o.Review).Where(o => o.Id == reviewRoundId).SingleAsync();
            return View(reviewDetail);
        }
        #endregion

        #region 评选结果
        public  async Task<IActionResult> InitialReview()
        {
            var matchInstance = await GetCurrentMatchInstance();
            var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id && o.IsActive).ToListAsync();
            foreach (var prize in prizes)
            {
                foreach (var prizeSubMajor in prize.PrizeSubMajors)
                {
                    await PrizeSubMajorRepository.EnsurePropertyLoadedAsync(prizeSubMajor, o => o.Major);
                }

            }
            ViewData["matchInstance"] = matchInstance;
            return View(prizes);
        }
        public async Task<IActionResult> FinalReview()
        {
            var matchInstance = await GetCurrentMatchInstance();
            var prizes = await PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstance.Id && o.IsActive).ToListAsync();
            foreach (var prize in prizes)
            {
                foreach (var prizeSubMajor in prize.PrizeSubMajors)
                {
                    await PrizeSubMajorRepository.EnsurePropertyLoadedAsync(prizeSubMajor, o => o.Major);
                }

            }
            ViewData["matchInstance"] = matchInstance;
            return View(prizes);
        }
        /// <summary>
        /// 评选详情
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="reviewType"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProjectScoreView(int projectId,ReviewType reviewType)
        {
            var matchInstance = await GetCurrentMatchInstance();
            var project = await ProjectRepository.GetAsync(projectId);
            var finalScore = reviewType == ReviewType.Initial ? project.ScoreInitial : project.ScoreFinal;
            ViewBag.FinalScore = finalScore;
            //获取项目对应的所有评选数据
            var projectMajorScoreDtos = await ReviewAppService.GetProjectMajorScores(projectId, reviewType);
            return View("ProjectScoreViewSingle", projectMajorScoreDtos);
            if (project.Prize.PrizeType != PrizeType.Multiple)
            {
                //非综合类奖项
                
            }
            else
            {
                return View("ProjectScoreViewMultiple", projectMajorScoreDtos);
            }
        }
        #endregion
    }
}
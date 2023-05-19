using Abp.AspNetCore.Mvc.Authorization;
using Abp.Auditing;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using Abp.Extensions;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Master.Authentication;
using Master.Configuration;
using Master.Controllers;
using Master.EntityFrameworkCore;
using Master.Matches;
using Master.Projects;
using Master.Reviews;
using Master.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master.Web.Controllers
{
    public class HomeController : MasterControllerBase
    {
        private ISessionAppService _sessionAppService;
        private readonly UserManager _userManager;
        private readonly IRepository<Setting> _settingRepository;
        private readonly IHostingEnvironment _env;
        private readonly IRepository<MatchInstance, int> _matchInstanceRepository;

        public BackUpManager BackUpManager { get; set; }
        public ProjectAppService ProjectAppService { get; set; }
        public MatchManager MatchManager { get; set; }

        public HomeController(ISessionAppService sessionAppService, UserManager userManager, IRepository<Setting> settingRepository, IHostingEnvironment hostingEnvironment, IRepository<MatchInstance, int> matchInstanceRepository)
        {
            _userManager = userManager;
            _settingRepository = settingRepository;
            _sessionAppService = sessionAppService;
            _env = hostingEnvironment;
            _matchInstanceRepository = matchInstanceRepository;
        }

        [AbpMvcAuthorize]
        public async Task<ActionResult> Index(string view)
        {
            var user = AbpSession.ToUserIdentifier();
            Session.Dto.LoginInformationDto loginInfo;
            try
            {
                loginInfo = await _sessionAppService.GetCurrentLoginInformations();
                var userModel = await _userManager.GetByIdAsync(user.UserId);
                if (userModel.GetData<string>("currentToken") != Request.Cookies["token"])
                {
                    throw new Exception("已在别处登录");
                }
            }
            catch
            {
                Response.Cookies.Delete("token");
                return Redirect("/Account/Login");
            }

            var appConfiguration = _env.GetAppConfiguration();
            ViewData["softTitle"] = appConfiguration["System:SoftTitle"];

            //默认首页
            if (loginInfo.User.HomeUrl.IsNullOrEmpty())
            {
                loginInfo.User.HomeUrl = "/Home/Default";
            }

            //申报人、分公司科管、专业负责人、集团科管进入赛事实例页页面
            if (AbpSession.IsReporter() ||
                AbpSession.IsSubManager() ||
                AbpSession.IsGroupManager() ||
                AbpSession.IsMajorManager()
                )
            {
                if (view == "resultsearch")
                {
                    return View("ResultSearch", loginInfo);
                }
                return View("Index_MatchInstance", loginInfo);
            }
            if (AbpSession.IsExpert())
            {
                return View("Index_Review", loginInfo);
            }
            if (AbpSession.IsProjectViewer())
            {
                ViewData["MatchNames"] = await MatchManager.GetAll().Select(o => o.Name).ToListAsync();
                return View("ProjectView", loginInfo);
            }
            return View(loginInfo);
        }

        /// <summary>
        /// 基于赛事实例的操作，用于申报人、分公司科管、集团科管
        /// </summary>
        /// <returns></returns>
        [AbpMvcAuthorize]
        public async Task<ActionResult> MatchInstance()
        {
            var matchIntanceId = Request.Cookies["matchInstanceId"];
            if (string.IsNullOrEmpty(matchIntanceId))
            {
                return RedirectToAction("Message", "Error", new { msg = "参数错误" });
            }
            var matchInstance = await _matchInstanceRepository.GetAll().Include(o => o.Match).Where(o => o.Id == int.Parse(matchIntanceId)).SingleOrDefaultAsync();
            if (matchInstance == null)
            {
                return RedirectToAction("Message", "Error", new { msg = "参数错误" });
            }
            var appConfiguration = _env.GetAppConfiguration();
            var loginInfo = await _sessionAppService.GetCurrentLoginInformations();
            ViewData["softTitle"] = appConfiguration["System:SoftTitle"];
            ViewData["matchInstance"] = matchInstance;

            return View(loginInfo);
        }

        /// <summary>
        /// 基于评审实例的操作
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ReviewInstance()
        {
            var review = await GetCurrentReview();
            if (review == null || review.CurrentReviewRound == null || review.CurrentReviewRound.ReviewStatus != Reviews.ReviewStatus.Reviewing)
            {
                return RedirectToAction("Index");
            }

            var appConfiguration = _env.GetAppConfiguration();
            var loginInfo = await _sessionAppService.GetCurrentLoginInformations();
            ViewData["softTitle"] = appConfiguration["System:SoftTitle"];
            ViewData["reviewInstance"] = review;
            List<ReviewProject> reviewProjects = review.ReviewProjects;
            var sourceProjectIds = review.CurrentReviewRound.SourceProjectIDs.Split(',').Select(o => int.Parse(o));
            reviewProjects = reviewProjects.Where(o => sourceProjectIds.Contains(o.Id)).ToList();
            List<Project> projects;
            if (review.CurrentReviewRound.ReviewMethod != Reviews.ReviewMethod.Vote)
            {
                //非投票类型的评审需要移除当前专家回避的项目
                reviewProjects = reviewProjects.Where(o => !o.IsAvoidedByExpert(AbpSession.UserId.Value)).ToList();
            }
            //获取所有待评审项目
            projects = await ProjectRepository.GetAllIncluding(o => o.Prize, o => o.DesignOrganization).Include(o => o.PrizeSubMajor).ThenInclude(o => o.Major).Where(o => reviewProjects.Select(r => r.Id).Contains(o.Id)).ToListAsync();
            //暂存的数据
            List<ProjectReviewDetail> projectReviewDetails = new List<ProjectReviewDetail>();
            var expertReviewDetail = review.CurrentReviewRound.ExpertReviewDetails.SingleOrDefault(o => o.ExpertID == AbpSession.UserId.Value);
            if (expertReviewDetail != null)
            {
                projectReviewDetails = expertReviewDetail.ProjectReviewDetails;
            }

            ViewData["ReviewProjects"] = reviewProjects;
            ViewData["ReviewProjectDtos"] = await ProjectAppService.ProjectToReviewProjectDtos(reviewProjects);
            ViewData["Projects"] = projects;
            ViewData["ProjectReviewDetails"] = projectReviewDetails;

            return View(loginInfo);
        }

        public async Task<ActionResult> Init()
        {
            var reporterRole = await RoleRepository.GetAll().Where(o => o.Name == "ProjectReporter").SingleAsync();
            var subRole = await RoleRepository.GetAll().Where(o => o.Name == "SubManager").SingleAsync();
            //初始化用户数据
            //var arr = new string[] {"sjzx|10","kczx|20","shy|40","hdzy|40","dszy|30","jszx|30","hjy|30","ghy|30","lby|15","szy|20","sly|20","syyt|20","syzx|10","dxy|20","ldgs|10","zcb|10","xb|20","wh|20","xn|20","yn|20","dl|20","ah|20","js|20","nb|20","cs|20","sz|20","xm|20" };
            //var arr = new string[] { "xxzx|10", "hdy|60", "xdy|30", "jcss|40", "syyt|40", "hjsc|10" };
            var arr = new string[] { "hdy|80" };
            foreach (var item in arr)
            {
                var prefix = item.Split('|')[0];
                var maxCount = int.Parse(item.Split('|')[1]);
                var organization = await OrganizationRepository.GetAll().Where(o => o.BriefCode == prefix).FirstOrDefaultAsync();
                for (var i = 1; i <= maxCount; i++)
                {
                    var username = prefix + (i < 10 ? "0" + i : i.ToString());
                    if (await UserRepository.CountAsync(o => o.UserName == username) > 0)
                    {
                        continue;
                    }
                    var user = new User()
                    {
                        UserName = username,
                        Name = username,
                    };
                    if (organization != null)
                    {
                        user.OrganizationId = organization.Id;
                    }
                    await _userManager.InsertAsync(user);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    await _userManager.SetPassword(user, "87654321");

                    await _userManager.SetRoles(user, new int[] { reporterRole.Id });
                }
                //科管
                var subUserName = prefix + "jsb";
                if (await UserRepository.CountAsync(o => o.UserName == subUserName) == 0)
                {
                    var subUser = new User()
                    {
                        UserName = subUserName,
                        Name = subUserName
                    };
                    if (organization != null)
                    {
                        subUser.OrganizationId = organization.Id;
                    }
                    await _userManager.InsertAsync(subUser);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    await _userManager.SetPassword(subUser, "87654321");

                    await _userManager.SetRoles(subUser, new int[] { subRole.Id });
                }
            }
            return Content(AbpSession.UserId.ToString());
        }

        public ActionResult Encrpt(string msg)
        {
            return Content(SimpleStringCipher.Instance.Encrypt(msg));
        }

        public ActionResult Decypt(string msg)
        {
            return Content(SimpleStringCipher.Instance.Decrypt("xPSdWiKf+3JmG7TCYOd6sg=="));
        }

        [UnitOfWork(false)]
        public async Task<ActionResult> DoBackUp()
        {
            await BackUpManager.BackUp();
            return Content("备份成功");
        }
    }
}
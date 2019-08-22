using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Abp.UI;
using Abp.Runtime.Caching;
using Master.Domain;
using Master.Majors;
using Master.Prizes;
using Master.Reviews;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Projects
{
    public class ProjectManager : DomainServiceBase<Project, int>
    {
        private object lockObj = new object();
        public IRepository<Prize,int> PrizeRepository { get; set; }
        public IRepository<Major,int> MajorRepository { get; set; }
        public IRepository<PrizeSubMajor,int> PrizeSubMajorRepository { get; set; }
        public IRepository<ProjectTraceLog,int> ProjectTraceLogRepository { get; set; }
        public IRepository<Review,int> ReviewRepository { get; set; }
        public virtual async Task<List<Project>> GetReviewProjectsByCache(List<ReviewProject> reviewProjects)
        {
            if (reviewProjects.Count == 0)
            {
                return new List<Project>();
            }
            var key = string.Join(',', reviewProjects.OrderBy(o=>o.Id).Select(o => o.Id));
            return await CacheManager.GetCache<string, List<Project>>("ProjectCache")
                .GetAsync(key, async (ids) => {
                    var idsArr = ids.Split(',').ToList().ConvertAll(o => int.Parse(o));
                    return await Repository
                    .GetAllIncluding(o => o.DesignOrganization)
                    .Include(o => o.PrizeSubMajor).ThenInclude(o => o.Major)
                    .Include(o => o.Prize).ThenInclude(o => o.Major)
                    .Where(o=> idsArr.Contains(o.Id))
                    .ToListAsync(); });
        }
        /// <summary>
        /// 重写添加方法，生成申请编号
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override Task InsertAsync(Project entity)
        {
            //todo:如何提升性能
            lock (lockObj)
            {                
                entity.ReportSN = GenerateReportSN(entity);
                //不允许当前奖项中出现重名项目
                if (Repository.Count(o => o.PrizeId == entity.PrizeId && o.ProjectName == entity.ProjectName) > 0)
                {
                    throw new UserFriendlyException("奖项下相同项目名称已存在");
                }
                base.InsertAsync(entity).GetAwaiter().GetResult();
            }
            return Task.CompletedTask;
        }
        public override async Task UpdateAsync(Project entity)
        {
            
            //不允许当前奖项中出现重名项目
            if (await Repository.CountAsync(o =>o.Id!=entity.Id && o.PrizeId == entity.PrizeId && (o.ProjectName == entity.ProjectName || o.ReportSN==entity.ProjectSN)) > 0)
            {
                throw new UserFriendlyException("奖项下相同项目名称已存在");
            }
            await base.UpdateAsync(entity);
        }
        /// <summary>
        /// 记录项目状态变更流转
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="actionName"></param>
        /// <param name="targetStatus"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public virtual async Task TraceLog(int projectId,string actionName,ProjectStatus targetStatus,string remarks="")
        {
            var projectTraceLog = new ProjectTraceLog()
            {
                ProjectId = projectId,
                TargetStatus = targetStatus,
                Remarks = remarks,
                ActionName=actionName
            };
            await ProjectTraceLogRepository.InsertAsync(projectTraceLog);
        }
        /// <summary>
        /// 变更项目状态
        /// </summary>
        /// <param name="project"></param>
        /// <param name="fromStatus"></param>
        /// <param name="targetStatus"></param>
        /// <returns></returns>
        public virtual async Task ChangeProjectStatus(int projectId,ProjectStatus fromStatus,ProjectStatus targetStatus,string remarks="")
        {
            if (fromStatus == targetStatus)
            {
                return ;
            }
            
        }

        private string GenerateReportSN(Project project)
        {
            
            var prize = PrizeRepository.GetAll().Include(o=>o.PrizeSubMajors).Include(o=>o.Major).Where(o=>o.Id==project.PrizeId).Single();
            var mainMajorSN = prize.Major.BriefCode;
            var subMajorSN = "";
            if (project.PrizeSubMajorId.HasValue)
            {
                var prizeSubMajor = prize.PrizeSubMajors.Where(o => o.Id == project.PrizeSubMajorId.Value).First();
                PrizeSubMajorRepository.EnsurePropertyLoaded(prizeSubMajor, o => o.Major);
                subMajorSN = prizeSubMajor.Major.BriefCode;
            }
            var prefix = $"{DateTime.Now.ToString("yy")}-{mainMajorSN}{subMajorSN}-";
            var existCount = Repository.Count(o => o.ReportSN.StartsWith(prefix));
            return prefix + PadLeft((existCount + 1).ToString(),3);
            
        }

        public virtual (int allCount,int rankedCount) GetProjectExpertCount(Project project,ReviewType reviewType)
        {
            //获取包含项目的所有评审
            var reviews = ReviewRepository.GetAllIncluding(o => o.ReviewRounds)
                .Where(o => o.MatchInstanceId == project.MatchInstanceId && o.ReviewType == reviewType)
                .Where(o => o.ReviewProjects.Count(p => p.Id == project.Id) > 0)
                .ToList();
            //分发专家(去除回避)
            var expertCount = reviews.Sum(o =>o.ExpertCount- o.GetProjectExcludeExpertCount(project));
            var rankedCount = reviews.Sum(o => {
                //最后包含此项目的轮次
                var lastround = o.ReviewRounds.Where(r=>r.SourceProjectIDs.Split(',').Contains(project.Id.ToString())).LastOrDefault();
                if (lastround == null)
                {
                    return 0;
                }
                //去除回避项目的专家
                return lastround.ExpertReviewDetails.SelectMany(a => a.ProjectReviewDetails).Count(p => p.ProjectId == project.Id && !p.IsAvoid);

            });

            return (expertCount, rankedCount);
        }
        private string PadLeft(string str,int length)
        {
            if (str.Length >= length)
            {
                return str;
            }
            else
            {
                while(str.Length<length)
                {
                    str = "0" + str;
                }
                return str;
            }
        }
    }
}

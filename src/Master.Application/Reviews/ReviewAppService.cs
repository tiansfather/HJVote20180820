using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Abp.Web.Models;
using Master.Dto;
using Master.Majors;
using Master.Projects;
using Microsoft.EntityFrameworkCore;
using Abp.Runtime.Caching;
namespace Master.Reviews
{
    public class ReviewAppService : MasterAppServiceBase<Review, int>
    {
        public ProjectManager ProjectManager { get; set; }
        private object _lockObj = new object();
        public IRepository<ReviewRound,int> ReviewRoundRepository { get; set; }
        public IRepository<ProjectTraceLog,int> ProjectTraceLogRepository { get; set; }
        public IRepository<Major,int> MajorRepository { get; set; }
        public IRepository<Review,int> ReviewRepository { get; set; }
        [DontWrapResult]

        #region 评选活动
        public override async Task<ResultPageDto> GetPageResult(RequestPageDto request)
        {
            var pageResult = await GetPageResultQueryable(request);
            var data = (await pageResult.Queryable.Include(o=>o.ReviewRounds).ToListAsync())
                .Select(o => new {
                    o.Id,
                    o.ReviewMajorName,
                    o.ReviewName,
                    o.ReviewType,
                    o.Remarks,
                    o.ProjectCount,
                    o.ExpertCount,
                    o.ReviewStatus,
                    StartTime=o.StartTime==null?"":o.StartTime.Value.ToString("yyyy-MM-dd HH:mm"),
                    CurrentRound=Common.Fun.NumberToChinese( o.CurrentRound),
                    o.CurrentTurn
                }).ToList();

            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }

        /// <summary>
        /// 添加新的评审活动
        /// </summary>
        /// <param name="reviewPrizeChooseDto"></param>
        /// <returns></returns>
        public virtual async Task AddReview(ReviewPrizeChooseDto reviewPrizeChooseDto)
        {
            var manager = Manager as ReviewManager;
            var review = reviewPrizeChooseDto.MapTo<Review>();

            await manager.InsertAsync(review);
        }
        /// <summary>
        /// 调整评选活动的专业
        /// </summary>
        /// <param name="reviewPrizeChooseDto"></param>
        /// <returns></returns>
        public virtual async Task ChangeReview(ReviewPrizeChooseDto reviewPrizeChooseDto)
        {
            var manager = Manager as ReviewManager;
            var review = await Repository.GetAsync(reviewPrizeChooseDto.ReviewId);
            reviewPrizeChooseDto.MapTo(review);

            if (review.ReviewRounds.Count > 0)
            {
                throw new UserFriendlyException("已有评审轮次建立，不能进行调整");
            }

            await manager.ChangeMajorAsync(review);
        }

        /// <summary>
        /// 更新评选活动
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="reviewExpertDtos"></param>
        /// <param name="reviewProjectDtos"></param>
        /// <returns></returns>
        public virtual async Task UpdateReview(ReviewUpdateDto reviewUpdateDto)
        {
            var manager = Manager as ReviewManager;
            var review = await Repository.GetAllIncluding(o => o.ReviewRounds).Where(o => o.Id == reviewUpdateDto.ReviewId).SingleAsync();
            if (!manager.CanReviewChange(review))
            {
                throw new UserFriendlyException("只有在第一轮次下的未发布评审才能提交编辑");
            }
            review.ReviewExperts = reviewUpdateDto.Experts.Select(o => new ReviewExpert() { Id = o.Id }).ToList();
            review.ReviewProjects = reviewUpdateDto.Projects.MapTo<List<ReviewProject>>();
            review.ReviewName = reviewUpdateDto.ReviewName;
            //更新评审中的来源项目值
            if (review.ReviewRounds.Count > 0)
            {
                review.ReviewRounds.First().SourceProjectIDs= string.Join(',', review.ReviewProjects.Select(o => o.Id));
            }
            //add 20191206 将排序保存至项目
            var projects = await ProjectManager.GetListByIdsAsync(review.ReviewProjects.Select(o => o.Id));
            foreach(var project in projects)
            {
                project.ReviewSort = review.ReviewProjects.Single(o => o.Id == project.Id).Sort;
            }
            await manager.UpdateAsync(review);
        }
        /// <summary>
        /// 删除评选活动
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public override async Task DeleteEntity(IEnumerable<int> ids)
        {
            if(await Repository.CountAsync(o=>ids.Contains(o.Id) && o.ReviewRounds.Count > 0) > 0)
            {
                throw new UserFriendlyException("已建立评审活动的评选不能删除");
            }
            await base.DeleteEntity(ids);
        }

        public virtual async Task UpdateReviewScore(int[] reviewIds)
        {
            foreach(var reviewId in reviewIds)
            {
                await (Manager as ReviewManager).UpdateReviewProjectScore(reviewId);
            }
            
        }
        #endregion

        #region 评审轮次
        public virtual async Task<bool> GetIfHasRateTable(int reviewId)
        {
            var manager = Manager as ReviewManager;
            var review = await manager.GetByIdAsync(reviewId);
            var rateTable = await manager.GetRateTable(review);

            return rateTable != null;
        }
        /// <summary>
        /// 提交评审活动
        /// </summary>
        /// <returns></returns>
        public virtual async Task SubmitReviewRound(ReviewRoundDto reviewRoundDto)
        {
            var manager = Manager as ReviewManager;
            var review = await Manager.GetByIdAsync(reviewRoundDto.ReviewId);
            ReviewRound reviewRound = null;
            await Repository.EnsureCollectionLoadedAsync(review, o => o.ReviewRounds);
            if (reviewRoundDto.Id == 0)
            {
                //新的评审
                if(review.ReviewRounds.Count(o=>o.Round==reviewRoundDto.Round && o.Turn == reviewRoundDto.Turn) > 0)
                {
                    throw new UserFriendlyException("相同轮次的评审已存在");
                }
                else
                {
                    reviewRound = reviewRoundDto.MapTo<ReviewRound>();
                    //设置源项目id
                    if (review.ReviewRounds.Count == 0)
                    {
                        //如果是第一次评审，则取评选活动的参选项目做为本次评选的来源项目
                        reviewRound.SourceProjectIDs = string.Join(',', review.ReviewProjects.Select(o => o.Id));
                    }
                    else
                    {
                        //非第一次评审,则取选择的项目作为来源项目
                        //reviewRound.SourceProjectIDs = review.ReviewRounds.Last().ResultProjectIDs;
                    }
                    review.ReviewRounds.Add(reviewRound);
                    //如果有专家回避了所有项目,则直接设置此专家为已提交状态
                    var reviewProjects = review.ReviewProjects.Where(o => reviewRound.SourceProjectIDs.Split(',').Contains(o.Id.ToString()));
                    var excludeExpertIdsAll = reviewProjects.SelectMany(o => {
                        var excludeExpertIdsStr = string.IsNullOrEmpty(o.ExcludeExpertIDs) ? "" : o.ExcludeExpertIDs;
                        return excludeExpertIdsStr.Split(',');
                    }).ToList();
                    foreach(var expert in review.ReviewExperts)
                    {
                        if (excludeExpertIdsAll.Count(o => o == expert.Id.ToString()) == reviewProjects.Count())
                        {
                            reviewRound.ExpertReviewDetails=new List<ExpertReviewDetail>(){
                                new ExpertReviewDetail()
                            {
                                ExpertID=expert.Id,
                                FinishTime=DateTime.Now,
                                ProjectReviewDetails=new List<ProjectReviewDetail>()
                            } };
                        }
                    }
                }
            }
            else
            {
                //修改
                reviewRound = await ReviewRoundRepository.GetAsync(reviewRoundDto.Id);
                reviewRoundDto.MapTo(reviewRound);
            }
            //如果使用评分表，则将评分表的总分作为评审活动的分制上限
            if (reviewRound.ReviewMethodSetting.RateType == RateType.RateTable)
            {
                var rateTables = await manager.GetRateTable(review);
                var reviewMethodSetting = reviewRound.ReviewMethodSetting;
                reviewMethodSetting.MaxScore = rateTables[0].TotalScore;
                reviewRound.ReviewMethodSetting = reviewMethodSetting;
            }
            //根据评审轮次的状态更新评选活动的状态
            review.ReviewStatus = reviewRoundDto.ReviewStatus;
            if (review.ReviewStatus==ReviewStatus.Reviewing)
            {
                review.StartTime = DateTime.Now;
            }
        }
        /// <summary>
        /// 获取评审轮次数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<ReviewRoundDto> GetReviewRound(int id)
        {
            var manager = Manager as ReviewManager;
            var reviewRound = await ReviewRoundRepository.GetAsync(id);
            var reviewRoundDto = reviewRound.MapTo<ReviewRoundDto>();
            reviewRoundDto.ReviewName = reviewRound.Review.ReviewName;
            //获取对应打分
            var rateTable =await manager.GetRateTable(reviewRound.Review);
            reviewRoundDto.HasRateTable = rateTable != null;
            reviewRoundDto.HasRateTable = false;
            return reviewRoundDto;
        }
        /// <summary>
        /// 撤回评审轮次
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task WithDraw(int id)
        {
            var manager = Manager as ReviewManager;
            var reviewdetail = await ReviewRoundRepository.GetAsync(id);
            if (reviewdetail.ReviewStatus == ReviewStatus.Reviewed)
            {
               throw new UserFriendlyException("此次评审已经全部结束,无法撤回");
            }
            //撤回
            await manager.WithDraw(reviewdetail);
        }
        #endregion

        #region 专家评审相关
        /// <summary>
        /// 获取专家可用评审
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<ReviewDto>> GetAvailableReview()
        {
            //先获取进行中评审
            var reviews = await Manager.GetAll().Include(o => o.ReviewRounds).Where(o => o.ReviewStatus == ReviewStatus.Reviewing).ToListAsync();

            var expertId = AbpSession.UserId.Value;
            var reviewDtos = new List<ReviewDto>();
            foreach(var review in reviews)
            {
                //已选入该专家的评审
                if (review.ReviewExperts.Count(o => o.Id==expertId) > 0)
                {
                    var reviewDto = review.MapTo<ReviewDto>();
                    reviewDto.CurrentRoundC = Common.Fun.NumberToChinese(reviewDto.CurrentRound);
                    reviewDtos.Add(reviewDto);
                }
            }

            return reviewDtos;
        }
        /// <summary>
        /// 评审活动是否当前专家可以操作
        /// </summary>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        public virtual async Task<bool> GetIfReviewAvailable(int reviewId)
        {
            var review = await Repository.GetAllIncluding(o => o.ReviewRounds).Where(o => o.Id == reviewId).SingleAsync();
            var currentReviewRound = review.CurrentReviewRound;

            if (currentReviewRound == null)
            {
                throw new UserFriendlyException("不存在对应进行中评审"); 
            }
            if (currentReviewRound.ExpertReviewDetails.Exists(o => o.ExpertID == AbpSession.UserId.Value && o.FinishTime != null))
            {
                throw new UserFriendlyException("您已提交过评审");
            }

            return true;
        }

        public virtual async Task SubmitExpertReview(int reviewId, List<ProjectReviewDetail> projectReviewDetails, bool isPublish)
        {
            
            lock (_lockObj)
            {
                var review = Repository.Get(reviewId);
                var currentRound = review.CurrentReviewRound;//当前进行轮次评审

                //foreach (var projectReviewDetailDto in projectReviewDetailDtos)
                //{
                //    if (string.IsNullOrEmpty(projectReviewDetailDto.Score))
                //    {
                //        projectReviewDetailDto.Score = "0";
                //    }
                //}
                //var projectReviewDetails = projectReviewDetailDtos.MapTo<List<ProjectReviewDetail>>();

                //var projectReviewDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProjectReviewDetail>>(projectDetailsStr);

                foreach (var projectReviewDetail in projectReviewDetails)
                {
                    projectReviewDetail.ExpertId = AbpSession.UserId.Value;
                }

                var expertReviewDetails = currentRound.ExpertReviewDetails;
                //移除旧评分
                expertReviewDetails.RemoveAll(o => o.ExpertID == AbpSession.UserId.Value);
                //加入新评分
                var expertReviewDetail = new ExpertReviewDetail()
                {
                    ExpertID = AbpSession.UserId.Value,
                    ProjectReviewDetails = projectReviewDetails
                };
                if (isPublish) { expertReviewDetail.FinishTime = DateTime.Now; }
                expertReviewDetails.Add(expertReviewDetail);

                currentRound.ExpertReviewDetails = expertReviewDetails;

                //如果专家都已投票，则设置评审活动状态为已结束
                if (review.ExpertCount == expertReviewDetails.Count(o => o.FinishTime != null))
                {
                    currentRound.ReviewStatus = ReviewStatus.Reviewed;
                    review.ReviewStatus = ReviewStatus.Reviewed;
                }
            }

            //不重新计算排名
            //var manager = Manager as ReviewManager;
            //if (isPublish)
            //{
            //    //更新评选项目的分数
            //    await manager.UpdateReviewProjectScore(reviewId);
            //}
            
        }
        #endregion

        #region 评审数据获取
        /// <summary>
        /// 获取评审轮次数据
        /// </summary>
        /// <param name="reviewRoundId"></param>
        /// <returns></returns>
        [DontWrapResult]
        public virtual async Task<object> GetReviewRoundDetail(int reviewRoundId)
        {
            var manager = Manager as ReviewManager;
            var reviewdetail = await ReviewRoundRepository.GetAsync(reviewRoundId);
            var sourceprojectids = reviewdetail.SourceProjectIDs;
            var data =(await manager.GetProjectRanks(reviewdetail)).Where(o => reviewdetail.SourceProjectIDs.Split(',').ToList().Contains(o.Id.ToString())).ToList();
            //进行同分判定
            var linescore = data[reviewdetail.TargetNumber - 1].TotalScore;//界限分

            for (var i = 0; i < data.Count; i++)
            {
                var obj = data[i];
                obj.Rank = i + 1;
            }
            if (data.Count(o => o.TotalScore == linescore) > 1 && data.Count(o => o.TotalScore >= linescore) > reviewdetail.TargetNumber)
            {
                foreach(var obj in data.Where(o => o.TotalScore == linescore))
                {
                    obj.NeedConfirm = true;
                }
            }

            var result = new ResultPageDto()
            {
                code = 0,
                count = data.Count,
                data = data
            };

            return result;
        }
        /// <summary>
        /// 获取项目的每轮次打分明细
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public virtual async Task<List<ProjectMajorScoreDto>> GetProjectMajorScores(int projectId,ReviewType reviewType)
        {
            Logger.Error("0:" + DateTime.Now.ToString("HH:mm:ss:fff"));
            var manager = Manager as ReviewManager;
            var result = new List<ProjectMajorScoreDto>();

            var project = await ProjectRepository.GetAsync(projectId);
            var reviewQuery = Repository.GetAll().Where(o => o.MatchInstanceId == project.MatchInstanceId && o.ReviewType == reviewType );
            if (reviewType != ReviewType.Champion)
            {
                reviewQuery = reviewQuery.Where(o => o.MajorId == project.Prize.MajorId);
                //不同的奖项类型获取对应的评选条件不同
                if (project.Prize.PrizeType == Prizes.PrizeType.Major)
                {
                    reviewQuery = reviewQuery.Where(o => o.SubMajorId == project.PrizeSubMajor.MajorId);
                }
                else if (project.Prize.PrizeType == Prizes.PrizeType.Base || project.Prize.PrizeType == Prizes.PrizeType.Mixed || (project.Prize.PrizeType == Prizes.PrizeType.Multiple && reviewType == ReviewType.Finish))
                {
                    //混排类按基本类处理
                    //终评下的综合类也按基本类处理
                    reviewQuery = reviewQuery.Where(o => o.SubMajorId == null);
                }
                else
                {
                    var projectSubMajorIds = project.Prize.PrizeSubMajors.Select(o => o.MajorId);
                    reviewQuery = reviewQuery.Where(o => o.SubMajorId != null && projectSubMajorIds.Contains(o.SubMajorId.Value));
                }
            }
            
            //
            Logger.Error("1:" + DateTime.Now.ToString("HH:mm:ss:fff"));
            foreach (var review in await reviewQuery.ToListAsync())
            {
                decimal? tempScore;
                var subMajorName = "";
                string tip = "";
                decimal subMajorScore = 0;
                //混排类也只显示专业大类
                if (review.SubMajorId != null && project.Prize.PrizeType!=Prizes.PrizeType.Mixed)
                {
                    subMajorName = (await MajorRepository.GetAsync(review.SubMajorId.Value)).BriefName;
                }
                else
                {
                    subMajorName = review.Major?.BriefName;
                }
                if (project.Prize.PrizeType == Prizes.PrizeType.Multiple && reviewType==ReviewType.Initial)
                {
                    var projectMajor = project.ProjectMajorInfos.Where(o => o.MajorId == review.SubMajorId.Value).SingleOrDefault();
                    var prizeSubMajor = project.Prize.PrizeSubMajors.Where(o => o.MajorId == review.SubMajorId.Value).SingleOrDefault();
                    if (projectMajor != null && prizeSubMajor!=null)
                    {
                        //计算综合奖项每个专业分经过权重加成的分数
                        tempScore = (reviewType == ReviewType.Initial ? projectMajor.ScoreInitial : projectMajor.ScoreFinal);
                        tip = $"权重{prizeSubMajor.Percent}%";
                        if (project.IsOriginal) {
                            tip += $",加成系数{(prizeSubMajor.Ratio == null ? 1 : prizeSubMajor.Ratio.Value)}";
                        }
                        if (tempScore.HasValue)
                        {
                            subMajorScore = tempScore.Value;
                            subMajorScore *= prizeSubMajor.Percent.Value * ((prizeSubMajor.Ratio == null||!project.IsOriginal) ? 1 : prizeSubMajor.Ratio.Value)/100;
                        }
                        
                    }
                }
                else
                {//非综合类直接取项目分数
                    //tempScore = reviewType == ReviewType.Initial ? project.ScoreInitial : project.ScoreFinal;
                    tempScore = project.GetScoreForReviewType(reviewType);
                    if (tempScore.HasValue)
                    {
                        subMajorScore= tempScore.Value;
                    }
                }
                var projectMajorScoreDto = new ProjectMajorScoreDto()
                {
                    SubMajorName = subMajorName,
                    SubMajorScore =Math.Round( subMajorScore,2),
                    Tip = tip
                };
                projectMajorScoreDto.ProjectMajorRoundScoreDtos = new List<ProjectMajorRoundScoreDto>();
                Logger.Error("2:" + DateTime.Now.ToString("HH:mm:ss:fff"));
                var projectreviewSummaries = await manager.GetAllProjectRanks(review);
                Logger.Error("3:" + DateTime.Now.ToString("HH:mm:ss:fff"));
                foreach (var reviewRound in review.ReviewRounds.Where(o=>o.Turn==1))
                {
                    //是否存在某轮的打分
                    var roundScores = projectreviewSummaries.Where(o => o.Id == project.Id && o.Round == reviewRound.Round);                    
                    
                    if (roundScores.Count()>0)
                    {
                        var voteTurns = review.ReviewRounds.Where(o => o.Round == reviewRound.Round && o.Turn > 1);//补充评审
                        ////需要加入补充评审
                        //var subReviewDetails= voteTurns
                        //    .SelectMany(o => o.ExpertReviewDetails)
                        //    .SelectMany(o => o.ProjectReviewDetails)
                        //    .Where(o=>o.ProjectId==project.Id)
                        //    .ToList();
                        ////
                        var roundScore = roundScores.First();
                        var projectMajorRoundScoreDto = new ProjectMajorRoundScoreDto()
                        {
                            ReviewRoundId=reviewRound.Id,
                            Round = reviewRound.Round,
                            Score = roundScore.Score,
                            HasRateTable=reviewRound.ReviewMethodSetting.RateType==RateType.RateTable,
                            IsVote= reviewRound.ReviewMethod==ReviewMethod.Vote
                        };
                        //专家打分明细
                        projectMajorRoundScoreDto.ProjectMajorRoundExpertScoreDtos = reviewRound.ExpertReviewDetails.Where(o=>o.FinishTime!=null).SelectMany(o => o.ProjectReviewDetails)
                            .Where(o => o.ProjectId == project.Id )
                            .Select(o => new ProjectMajorRoundExpertScoreDto() {
                                ExpertId = o.ExpertId,
                                VoteFlag=o.VoteFlag,
                                Score =  o.Score.HasValue?o.Score.Value:0,
                                IsAvoid = o.IsAvoid,
                                ExpertName = UserManager.GetByIdAsync(o.ExpertId).Result.Name,
                                SubVotes = voteTurns.Select(v => {
                                    bool? voteResult;
                                    var details = v.ExpertReviewDetails.Where(d=>d.FinishTime!=null).SelectMany(a => a.ProjectReviewDetails)
                                        .Where(a => a.ProjectId == project.Id && a.ExpertId == o.ExpertId);
                                    if (details.Count() == 0)
                                    {
                                        voteResult = null;
                                    }
                                    else
                                    {
                                        voteResult = details.Count(a => a.VoteFlag == true) > 0;
                                    }
                                    return voteResult;
                                    //return v.ExpertReviewDetails.SelectMany(a => a.ProjectReviewDetails)
                                    //    .Where(a => a.ProjectId == project.Id)
                                    //    .Count(a => a.ExpertId == o.ExpertId && a.VoteFlag == true) > 0;
                                        }
                                ).ToList() 
                            })
                            .ToList();

                        projectMajorScoreDto.ProjectMajorRoundScoreDtos.Add(projectMajorRoundScoreDto);
                    }
                    
                }
                Logger.Error("4:" + DateTime.Now.ToString("HH:mm:ss:fff"));
                result.Add(projectMajorScoreDto);
            }
            Logger.Error("5:" + DateTime.Now.ToString("HH:mm:ss:fff"));
            return result;
        }
        #endregion

        #region 移入移出终评
        /// <summary>
        /// 进入终评
        /// </summary>
        /// <param name="moduleKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task BringInFinalReview(int[] projectIds)
        {
            if(await ProjectRepository.CountAsync(o=> projectIds.Contains(o.Id) && o.IsInFinalReview)>0){
                throw new UserFriendlyException("项目已经被选入终评");
            }
            var projects = await ProjectRepository.GetAll().Where(o => projectIds.Contains(o.Id)).ToListAsync();
            foreach(var project in projects)
            {
                project.IsInFinalReview = true;
                project.ProjectStatus = ProjectStatus.FinalReviewing;
                await CacheManager.GetCache<int, object>("ProjectResultCache").RemoveAsync(project.Id);
            }
        }

        public virtual async Task BringOutFinalReview(int[] projectIds)
        {
            if (await ProjectRepository.CountAsync(o => projectIds.Contains(o.Id) && !o.IsInFinalReview) > 0)
            {
                throw new UserFriendlyException("项目已经被移出终评");
            }
            //所有终评中的项目
            var reviews = await ReviewRepository.GetAll().Where(o => o.ReviewType == ReviewType.Finish)
                .ToListAsync();
            var finalReviewProjects = reviews.SelectMany(o => o.ReviewProjects).Select(o => o.Id).Distinct();
            if (projectIds.Count(o => finalReviewProjects.Contains(o)) > 0)
            {
                throw new UserFriendlyException("无法将终评中的项目移出终评");
            }
            //已经是终评中的项目无法取消终评
            //if(await ProjectRepository.CountAsync(o=>projectIds.Contains(o.Id) && o.ProjectStatus == ProjectStatus.FinalReviewing) > 0)
            //{
            //    throw new UserFriendlyException("无法将终评中的项目移出终评");
            //}
            var projects = await ProjectRepository.GetAll().Where(o => projectIds.Contains(o.Id)).ToListAsync();
            foreach (var project in projects)
            {
                project.IsInFinalReview = false;
                //获取项目之前的项目状态
                var traceLogs = await ProjectTraceLogRepository.GetAll().Where(o => o.ProjectId == project.Id).ToListAsync();
                if (traceLogs.Count > 2)
                {
                    //最后一项记录保存的是项目的当前状态,故再往上取一个记录
                    var lastTraceLog = traceLogs[traceLogs.Count - 2];
                    project.ProjectStatus = lastTraceLog.TargetStatus;
                    //更改项目状态
                    await ProjectManager.TraceLog(project.Id, "移出终评", project.ProjectStatus);
                }
                
                await CacheManager.GetCache<int, object>("ProjectResultCache").RemoveAsync(project.Id);
            }
        }
        #endregion

        #region 移入移出决赛
        /// <summary>
        /// 进入终评
        /// </summary>
        /// <param name="moduleKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task BringInChampionReview(int[] projectIds)
        {
            if (await ProjectRepository.CountAsync(o => projectIds.Contains(o.Id) && o.IsInChampionReview) > 0)
            {
                throw new UserFriendlyException("项目已经被选入决赛");
            }
            var projects = await ProjectRepository.GetAll().Where(o => projectIds.Contains(o.Id)).ToListAsync();
            foreach (var project in projects)
            {
                project.IsInChampionReview = true;
                project.ProjectStatus = ProjectStatus.ChampionReviewing;
                await CacheManager.GetCache<int, object>("ProjectResultCache").RemoveAsync(project.Id);
            }
        }

        public virtual async Task BringOutChampionReview(int[] projectIds)
        {
            if (await ProjectRepository.CountAsync(o => projectIds.Contains(o.Id) && !o.IsInChampionReview) > 0)
            {
                throw new UserFriendlyException("项目已经被移出决赛");
            }
            //所有决赛中的项目
            var reviews = await ReviewRepository.GetAll().Where(o => o.ReviewType == ReviewType.Champion)
                .ToListAsync();
            var championReviewProjects = reviews.SelectMany(o => o.ReviewProjects).Select(o => o.Id).Distinct();
            if (projectIds.Count(o => championReviewProjects.Contains(o)) > 0)
            {
                throw new UserFriendlyException("无法将决赛中的项目移出决赛");
            }
            //已经是终评中的项目无法取消终评
            //if(await ProjectRepository.CountAsync(o=>projectIds.Contains(o.Id) && o.ProjectStatus == ProjectStatus.FinalReviewing) > 0)
            //{
            //    throw new UserFriendlyException("无法将终评中的项目移出终评");
            //}
            var projects = await ProjectRepository.GetAll().Where(o => projectIds.Contains(o.Id)).ToListAsync();
            foreach (var project in projects)
            {
                project.IsInChampionReview = false;
                //获取项目之前的项目状态
                var traceLogs = await ProjectTraceLogRepository.GetAll().Where(o => o.ProjectId == project.Id).ToListAsync();
                //最后一项记录保存的是项目的当前状态,故再往上取一个记录
                var lastTraceLog = traceLogs[traceLogs.Count - 2];
                project.ProjectStatus = lastTraceLog.TargetStatus;
                //更改项目状态
                await ProjectManager.TraceLog(project.Id, "移出决赛", project.ProjectStatus);
                await CacheManager.GetCache<int, object>("ProjectResultCache").RemoveAsync(project.Id);
            }
        }
        #endregion

        #region 导出
        /// <summary>
        /// 导出评审
        /// </summary>
        /// <param name="matchInstanceId"></param>
        /// <returns></returns>
        public virtual async Task ExportReview(int reviewId)
        {
            var review = await Manager.GetAll().Include(o => o.MatchInstance).Include(o => o.ReviewRounds)
                .Where(o => o.Id == reviewId)
                .SingleOrDefaultAsync();
            var reviewFolder = Common.PathHelper.VirtualPathToAbsolutePath($"/MatchInstance/{review.MatchInstance.Name}/评审数据/{review.ReviewName}");
            System.IO.Directory.CreateDirectory(reviewFolder);
            foreach (var reviewRound in review.ReviewRounds)
            {
                //单次评审导出数据
                var reviewExportData = await ExportReviewRound(reviewRound.Id);
                var sourceFile = Common.PathHelper.VirtualPathToAbsolutePath(reviewExportData.filePath);
                var fileName = System.IO.Path.GetFileName(sourceFile);
                System.IO.File.Copy(sourceFile, System.IO.Path.Combine(reviewFolder, fileName));
            }
        }
        /// <summary>
        /// 导出评审详情
        /// </summary>
        /// <param name="reviewRoundId"></param>
        /// <returns></returns>
        public virtual async Task<dynamic> ExportReviewRound(int reviewRoundId)
        {
            var manager = Manager as ReviewManager;
            var reviewdetail = await ReviewRoundRepository.GetAsync(reviewRoundId);
            var review = reviewdetail.Review;
            var sourceprojectids = reviewdetail.SourceProjectIDs;
            //对应项目的打分情况
            var projectreviewsummarys = (await manager.GetProjectRanks(reviewdetail)).Where(o => reviewdetail.SourceProjectIDs.Split(',').ToList().Contains(o.Id.ToString())).ToList();
            for (var i = 0; i < projectreviewsummarys.Count; i++)
            {
                var obj = projectreviewsummarys[i];
                obj.Rank = i + 1;
            }
            var dt = await BuildProjectTable(reviewdetail);
            await BuildTableData(dt, reviewdetail, projectreviewsummarys);
            var now = DateTime.Now;
            var dic = $"{Directory.GetCurrentDirectory()}\\wwwroot\\files\\{now.ToString("yyyyMMdd")}";
            if (!System.IO.Directory.Exists(dic))
            {
                System.IO.Directory.CreateDirectory(dic);
            }
            var filename = $"{review.ReviewName}-{reviewdetail.Round}-{reviewdetail.Turn}-评审结果-{now.ToString("yyyyMMddHHmmss")}.xlsx";
            var filePath = System.IO.Path.Combine(dic, filename);
            Common.ExcelHelper.DataTableToExcel(dt, filePath, filename, true);
            return new { filePath = $"/files/{now.ToString("yyyyMMdd")}/{filename}", fileName = filename };
        }
        /// <summary>
        /// 构建表格结构
        /// </summary>
        /// <param name="review"></param>
        /// <param name="reviewdetail"></param>
        /// <param name="award"></param>
        /// <returns></returns>
        private async Task<DataTable> BuildProjectTable(ReviewRound reviewdetail)
        {
            var reviewExpertIds = reviewdetail.Review.ReviewExperts.Select(o => o.Id);
            var experts = await UserManager.Repository.GetAll().Where(o => reviewExpertIds.Contains(o.Id)).ToListAsync();
            var dt = new DataTable();
            var c = 53;//起始颜色
            dt.Columns.AddRange(new DataColumn[] {
                new DataColumn("排名"),
                new DataColumn("序号"),
                new DataColumn("项目名称"),
                new DataColumn("申报单位")
            });
            if (reviewdetail.ReviewMethod == ReviewMethod.Weighting)
            {
                //如果是与上轮加权的，需要增加一列
                dt.Columns.Add("加权得分").Prefix = "c" + c--.ToString();
            }
            dt.Columns.Add("本轮得分").Prefix = "c" + c--.ToString();
            if (reviewdetail.Round == 1 && reviewdetail.Turn == 1 && reviewdetail.ReviewMethod != ReviewMethod.Vote)
            {
                dt.Columns.Add("基础分").Prefix = "c" + c.ToString();
            }
            //补投列
            //显示的最大轮数
            var maxshowturn = reviewdetail.Turn == 1 ? reviewdetail.Review.ReviewRounds.Where(o => o.ReviewStatus == ReviewStatus.Reviewed && o.Round == reviewdetail.Round).Max(o => o.Turn) : reviewdetail.Turn;
            for (var i = 2; i <= maxshowturn; i++)
            {
                dt.Columns.Add("补投" + (i - 1)).Prefix = "c" + c--.ToString();
            }
            //专家打分列
            for (var i = 1; i <= maxshowturn; i++)
            {
                experts.ForEach(o => {
                    dt.Columns.Add(o.Name + i).Prefix = "c" + (c + i - 1).ToString();
                });
            }
            return dt;
        }

        private async Task BuildTableData(DataTable dt, ReviewRound reviewdetail, List<ProjectReviewSummary> projectreviewsummarys)
        {
            var reviewExpertIds = reviewdetail.Review.ReviewExperts.Select(o => o.Id);
            var reviewProjects = reviewdetail.Review.ReviewProjects;
            var experts = await UserManager.Repository.GetAll().Where(o => reviewExpertIds.Contains(o.Id)).ToListAsync();
            //显示的最大轮数
            var maxshowturn = reviewdetail.Turn == 1 ? reviewdetail.Review.ReviewRounds.Where(o => o.ReviewStatus == ReviewStatus.Reviewed && o.Round == reviewdetail.Round).Max(o => o.Turn) : reviewdetail.Turn;
            var allreviewdetails = reviewdetail.Review.ReviewRounds.Where(o => o.Round == reviewdetail.Round).ToList();
            foreach (var p in projectreviewsummarys)
            {
                var reviewProject = reviewProjects.Where(a => a.Id == p.Id).SingleOrDefault();
                var row = dt.NewRow();
                row["排名"] = p.Rank;
                row["序号"] = p.Sort;
                row["项目名称"] = p.ProjectName;
                row["申报单位"] = p.DesignOrganizationName;
                if (reviewdetail.ReviewMethod == ReviewMethod.Weighting)
                {
                    row["加权得分"] = p.Score;
                }
                row["本轮得分"] = p.OriScore;
                if (reviewdetail.Round == 1 && reviewdetail.Turn == 1 && reviewdetail.ReviewMethod != ReviewMethod.Vote)
                {
                    row["基础分"] = reviewProject?.BaseScore;
                }
                //补投列

                for (var i = 2; i <= maxshowturn; i++)
                {
                    row["补投" + (i - 1)] = p.SubScores[i - 2];
                }
                //专家打分列
                for (var i = 1; i <= maxshowturn; i++)
                {
                    var expertprojectreviewdetail = allreviewdetails[i - 1].ExpertReviewDetails;
                    experts.ForEach(o =>
                    {
                        var projectdetail = expertprojectreviewdetail.Single(e => e.ExpertID == o.Id).ProjectReviewDetails.SingleOrDefault(e => e.ProjectId == p.Id);
                        if (projectdetail != null)
                        {
                            row[o.Name + i] = projectdetail.IsAvoid ? "" : (allreviewdetails[i - 1].ReviewMethod == ReviewMethod.Vote ? (projectdetail.VoteFlag ? "Y" : "") : projectdetail.Score.ToString());
                        }

                    });
                }
                dt.Rows.Add(row);
            }
        }
        #endregion
    }
}

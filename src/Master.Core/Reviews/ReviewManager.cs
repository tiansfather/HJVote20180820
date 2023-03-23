using Abp.Domain.Repositories;
using Abp.UI;
using Abp.Runtime.Caching;
using Master.Authentication;
using Master.Domain;
using Master.Majors;
using Master.Matches;
using Master.Projects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Reviews
{
    public class ReviewManager : DomainServiceBase<Review, int>
    {
        public IRepository<MatchResource, int> MatchResourceRepository { get; set; }
        public IRepository<MatchInstance, int> MatchInstanceRepository { get; set; }
        public IRepository<Major, int> MajorRepository { get; set; }
        public IRepository<Project, int> ProjectRepository { get; set; }
        public MajorManager MajorManager { get; set; }
        public ProjectManager ProjectManager { get; set; }

        #region 评选活动准备

        /// <summary>
        /// 添加评审
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override async Task InsertAsync(Review entity)
        {
            var matchInstance = await MatchInstanceRepository.GetAsync(entity.MatchInstanceId);
            //if (matchInstance.MatchInstanceStatus != MatchInstanceStatus.Reviewing)
            //{
            //    throw new UserFriendlyException("当前赛事尚未结束申报，无法建立评选活动");
            //}
            //验证数据是否有效

            //if (await Repository.CountAsync(o=>o.MatchInstanceId==entity.MatchInstanceId && o.MajorId==entity.MajorId && o.SubMajorId==entity.SubMajorId && o.ReviewType == entity.ReviewType) > 0)
            //{
            //
            //}
            //初始化评审数据
            await InitReview(entity);

            await base.InsertAsync(entity);
        }

        /// <summary>
        /// 当评选活动更新后，需要更新当前赛事下所有项目的状态
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override async Task UpdateAsync(Review entity)
        {
            //验证数据是否有效
            if (await Repository.CountAsync(o => o.MatchInstanceId == entity.MatchInstanceId && o.ReviewName == entity.ReviewName && o.Id != entity.Id) > 0)
            {
                throw new UserFriendlyException("当前赛事下相同名称的评选活动已存在");
            }
            //todo:更新评选对应的项目状态
            await base.UpdateAsync(entity);
        }

        /// <summary>
        /// 是否评选活动可以编辑
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        public virtual bool CanReviewChange(Review review)
        {
            //当未建立评审或者评审只有一第一轮次，且未发布状态才可以编辑
            return review.ReviewStatus == ReviewStatus.BeforePublish && review.CurrentRound <= 1 && review.CurrentTurn <= 1;
        }

        /// <summary>
        /// 调整专业
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        public virtual async Task ChangeMajorAsync(Review review)
        {
            //验证数据是否有效
            if (await Repository.CountAsync(o => o.Id != review.Id && o.MatchInstanceId == review.MatchInstanceId && o.MajorId == review.MajorId && o.SubMajorId == review.SubMajorId && o.ReviewType == review.ReviewType) > 0)
            {
                throw new UserFriendlyException("当前赛事下相同专业对应的评选活动已存在");
            }
            await InitReview(review);
            await UpdateAsync(review);
        }

        /// <summary>
        /// 初始化评审数据：包括自动生成评审名称、专业名称，自动加入对应的项目和专家
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        private async Task InitReview(Review review)
        {
            var matchInstance = await MatchInstanceRepository.GetAllIncluding(o => o.Match).Where(o => o.Id == review.MatchInstanceId).FirstOrDefaultAsync();
            //决赛评审初始化
            if (review.ReviewType == ReviewType.Champion)
            {
                review.ReviewName = matchInstance.Match.Name + "-决赛";
                review.ReviewExperts = new List<ReviewExpert>();
                review.ReviewProjects = new List<ReviewProject>();
                return;
            }

            var mainMajor = await MajorRepository.GetAsync(review.MajorId.Value);
            //评选的专业名称
            review.ReviewMajorName = mainMajor.BriefName;
            if (review.SubMajorId != null)
            {
                var subMajor = await MajorRepository.GetAsync(review.SubMajorId.Value);
                review.ReviewMajorName += "-" + subMajor.BriefName;
            }
            review.ReviewName = matchInstance.Match.Name + "-" + review.ReviewMajorName;
            review.ReviewName += "-" + GetReviewName(review.ReviewType);
            //验证数据
            if (await Repository.CountAsync(o => o.MatchInstanceId == review.MatchInstanceId && o.ReviewName == review.ReviewName) > 0)
            {
                throw new UserFriendlyException("当前赛事下相同名称的评选活动已存在");
            }
            if (review.SubMajorId == null)
            {
                //如果未指定子专业，则默认不选入项目和专家
                review.ReviewExperts = new List<ReviewExpert>();
                review.ReviewProjects = new List<ReviewProject>();
                return;
            }
            //将当前赛事对应的项目加入
            var projectQuery = ProjectRepository.GetAll().Where(o => o.MatchInstanceId == review.MatchInstanceId && o.Prize.MajorId == review.MajorId);
            //modi20181031去除混排类项目
            projectQuery = projectQuery.Where(o => o.Prize.PrizeType != Prizes.PrizeType.Mixed);
            //&& (o.ProjectStatus==ProjectStatus.UnderReview || (o.ProjectStatus==ProjectStatus.Reviewing && o.Prize.PrizeType==Prizes.PrizeType.Multiple))
            if (review.ReviewType == ReviewType.Pre)
            {
                //预审选择待评选的项目
                projectQuery = projectQuery.Where(o => o.ProjectStatus == ProjectStatus.UnderReview);
            }
            else if (review.ReviewType == ReviewType.Initial)
            {
                //初评选择有初评标识的项目
                projectQuery = projectQuery.Where(o => o.IsInInitialReview);
            }
            else
            {
                //终评选择有终评标识的项目
                projectQuery = projectQuery.Where(o => o.IsInFinalReview);
            }
            if (review.SubMajorId == null)
            {
                //针对专业大类的项目
                projectQuery = projectQuery.Where(o => o.ProjectMajorInfos.Count == 1);
            }
            else
            {
                //有具体专业小类的项目
                projectQuery = projectQuery.Where(o => o.ProjectMajorInfos.Count(p => p.MajorId == review.SubMajorId) > 0);
            }
            var projects = await projectQuery.ToListAsync();
            var reviewProjects = new List<ReviewProject>();
            for (var i = 0; i < projects.Count; i++)
            {
                var reviewProject = new ReviewProject()
                {
                    Id = projects[i].Id,
                    Sort = 0
                };
                //设置项目状态
                if (review.ReviewType == ReviewType.Pre && projects[i].ProjectStatus != ProjectStatus.Preing)
                {
                    projects[i].ProjectStatus = ProjectStatus.Preing;
                    await ProjectManager.TraceLog(projects[i].Id, "进入预审", ProjectStatus.Preing);
                }
                if (review.ReviewType == ReviewType.Initial && projects[i].ProjectStatus != ProjectStatus.Reviewing)
                {
                    projects[i].ProjectStatus = ProjectStatus.Reviewing;
                    await ProjectManager.TraceLog(projects[i].Id, "进入初评", ProjectStatus.Reviewing);
                }
                if (review.ReviewType == ReviewType.Finish && projects[i].ProjectStatus != ProjectStatus.FinalReviewing)
                {
                    projects[i].ProjectStatus = ProjectStatus.FinalReviewing;
                    //更改项目状态
                    await ProjectManager.TraceLog(projects[i].Id, "进入终评", ProjectStatus.FinalReviewing);
                }

                reviewProjects.Add(reviewProject);
            }
            review.ReviewProjects = reviewProjects;

            //加入对应专业的专家modi20181101 不自动选入专家
            var reviewExperts = new List<ReviewExpert>();
            //List<User> experts = new List<User>();
            //var expertMajorId = review.SubMajorId == null ? review.MajorId : review.SubMajorId.Value;
            ////专家专业绑定使用的是原始专业树，而不是发布后复制出来的专业树，故需要通过专业sn去找
            //var major = (await MajorManager.GetByIdAsync(expertMajorId));
            //var oriMajor =await MajorRepository.GetAll().Where(o => o.BriefCode == major.BriefCode  && o.MatchInstanceId==null && o.MatchId==major.MatchInstance.MatchId).FirstOrDefaultAsync();
            //if (oriMajor != null)
            //{
            //    experts = await MajorManager.GetMajorExperts(oriMajor.Id);
            //    reviewExperts.AddRange(experts.Select(o => new ReviewExpert() { Id = o.Id }));
            //}
            review.ReviewExperts = reviewExperts;
        }

        private string GetReviewName(ReviewType reviewType)
        {
            if (reviewType == ReviewType.Pre) return "预审";
            if (reviewType == ReviewType.Initial) return "初评";
            if (reviewType == ReviewType.Finish) return "终评";
            if (reviewType == ReviewType.Champion) return "决赛";

            return string.Empty;
        }

        /// <summary>
        /// 获取评选活动的打分表
        /// </summary>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        public virtual async Task<List<MatchResourceRateTable>> GetRateTable(Review review)
        {
            var matchResource = await MatchResourceRepository.GetAll().Where(o => o.MatchInstanceId == review.MatchInstanceId && o.MatchResourceStatus == MatchResourceStatus.Publish && o.MatchResourceType == MatchResourceType.RateTable && o.MajorId == review.MajorId && o.SubMajorId == review.SubMajorId).FirstOrDefaultAsync();

            List<MatchResourceRateTable> result = null;
            if (matchResource != null)
            {
                result = new MatchResource<MatchResourceRateTable>(matchResource).Datas;
            }

            return result;
        }

        #endregion 评选活动准备

        #region 评审活动

        /// <summary>
        /// 撤回评审
        /// </summary>
        /// <param name="reviewRound"></param>
        /// <returns></returns>
        public virtual async Task WithDraw(ReviewRound reviewRound)
        {
            reviewRound.ReviewStatus = ReviewStatus.BeforePublish;
            reviewRound.ExpertReviewDetails = new List<ExpertReviewDetail>();//清空打分明细
            reviewRound.Review.ReviewStatus = ReviewStatus.BeforePublish;
            reviewRound.Review.StartTime = null;
        }

        /// <summary>
        /// 更新评选活动的项目分数
        /// </summary>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        public virtual async Task UpdateReviewProjectScore(int reviewId)
        {
            var review = await GetByIdAsync(reviewId);
            if (review.ReviewStatus != ReviewStatus.Reviewed)
            {
                //每次专家打分均重新计算排名
                //return;
            }
            var maxRound = review.ReviewRounds.Max(o => o.Round);
            var lastRound = review.ReviewRounds.Where(o => o.Round == maxRound && o.Turn == 1).Single();
            //评选活动的所有项目
            var sourceProjectIds = review.ReviewRounds.First().SourceProjectIDs.Split(',').Select(o => int.Parse(o));
            var projects = await ProjectRepository.GetAll().Where(o => sourceProjectIds.Contains(o.Id)).ToListAsync();
            //清空项目评分缓存
            foreach (var project in projects)
            {
                await CacheManager.GetCache<int, object>("ProjectResultCache").RemoveAsync(project.Id);
            }
            await CacheManager.GetCache<int, List<ProjectReviewSummary>>("ProjectReviewSummary")
                .RemoveAsync(reviewId);
            //获取评选的所有项目数据
            var projectReviewSummaries = await GetAllProjectRanks(review);
            //待重排名的奖项
            List<Prizes.Prize> prizes = new List<Prizes.Prize>();
            //如果投票专家超过10个，则补投权重为0.01，否则为0.1
            var subRatial = review.ReviewExperts.Count > 10 ? 100 : 10;
            foreach (var project in projects)
            {
                if (!prizes.Exists(o => o.Id == project.Prize.Id))
                {
                    //将项目的奖项加入待重排名列表
                    prizes.Add(project.Prize);
                }
                //统一转换为100分制计算轮次加权总分
                //rate=100/o.MaxScore 如果要转换为100分制
                var rate = 1;
                var maxround = review.ReviewRounds.Max(o => o.Round);
                var score = projectReviewSummaries.Where(o => o.Id == project.Id).Sum(o =>
                {
                    //var subScore = (o.SubScores.Count > 0 ? Convert.ToDecimal(o.SubScores.Average()) : 0)/10;
                    var subScore = CaculateIntScoresByIndex(o.SubScores.ToArray(), subRatial);
                    //加上补充评审分数
                    return (o.Score + subScore) * rate * Convert.ToDecimal(Math.Pow(10, o.Round - 1));
                }) / Convert.ToDecimal(Math.Pow(10, maxround - 1));
                if (review.ReviewType == ReviewType.Champion)
                {
                    //决赛项目直接设置分数
                    project.ScoreChampion = score;
                }
                else if (project.Prize.PrizeType != Prizes.PrizeType.Multiple ||
                    (review.ReviewType == ReviewType.Finish && project.Prize.PrizeType == Prizes.PrizeType.Multiple))
                {
                    //非综合类项目或者终评的综合类奖项直接设置分数
                    if (review.ReviewType == ReviewType.Pre)
                    {
                        project.ScorePre = score;
                    }
                    else if (review.ReviewType == ReviewType.Initial)
                    {
                        project.ScoreInitial = score;
                    }
                    else
                    {
                        project.ScoreFinal = score;
                    }
                }
                else
                {
                    //综合类奖项需要将每个专业的分数保存
                    var projectMajorInfos = project.ProjectMajorInfos;
                    var currentProjectMajor = projectMajorInfos.Single(o => o.MajorId == review.SubMajorId);
                    if (review.ReviewType == ReviewType.Initial)
                    {
                        currentProjectMajor.ScoreInitial = score;
                    }
                    else
                    {
                        currentProjectMajor.ScoreFinal = score;
                    }
                    //计算综合类权重总分
                    var prizeSubMajors = project.Prize.PrizeSubMajors;
                    project.ScoreInitial = 0;
                    project.ScoreFinal = 0;
                    foreach (var projectMajorInfo in projectMajorInfos.Where(o => o.MajorId != null))
                    {
                        var prizeSubMajor = prizeSubMajors.SingleOrDefault(o => o.MajorId == projectMajorInfo.MajorId);
                        if (prizeSubMajor != null)
                        {
                            if (projectMajorInfo.ScoreInitial != null)
                            {
                                //原创项目需要乘加成系数
                                project.ScoreInitial += projectMajorInfo.ScoreInitial.Value * prizeSubMajor.Percent.Value / 100 * ((prizeSubMajor.Ratio == null || !project.IsOriginal) ? 1 : prizeSubMajor.Ratio.Value);
                            }
                            if (projectMajorInfo.ScoreFinal != null)
                            {
                                //原创项目需要乘加成系数
                                project.ScoreFinal += projectMajorInfo.ScoreFinal.Value * prizeSubMajor.Percent.Value / 100 * ((prizeSubMajor.Ratio == null || !project.IsOriginal) ? 1 : prizeSubMajor.Ratio.Value);
                            }
                        }
                    }
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            //对项目排名重新计算
            if (review.ReviewType == ReviewType.Champion)
            {
                var i = 1;
                foreach (var project in projects.OrderByDescending(o => o.ScoreChampion))
                {
                    project.RankChampion = i++;
                }
            }
            else
            {
                await ReRankProjects(prizes, review.ReviewType);
            }
        }

        /// <summary>
        /// 计算数组加权分数，如[3,3,3]=0.333
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private decimal CaculateIntScoresByIndex(int[] arr, int subRatial = 10)
        {
            var result = 0M;
            for (var i = 0; i < arr.Length; i++)
            {
                result += Convert.ToDecimal(arr[i] * Math.Pow(subRatial, -i - 1));
            }
            return result;
        }

        /// <summary>
        /// 对奖项的项目进行重排
        /// </summary>
        /// <param name="prizes"></param>
        /// <returns></returns>
        private async Task ReRankProjects(List<Prizes.Prize> prizes, ReviewType reviewType)
        {
            foreach (var prize in prizes)
            {
                //加入待评选项目
                var projects = await ProjectRepository.GetAll().Where(o => o.PrizeId == prize.Id && (o.ProjectStatus == ProjectStatus.UnderReview || o.ProjectStatus == ProjectStatus.Reviewing || o.ProjectStatus == ProjectStatus.FinalReviewing)).ToListAsync();
                if (reviewType == ReviewType.Initial)
                {
                    if (prize.PrizeType == Prizes.PrizeType.Major)
                    {
                        //专业类需要分专业排名
                        var projectGroups = projects.GroupBy(o => o.PrizeSubMajorId);
                        foreach (var projectGroup in projectGroups)
                        {
                            var i = 1;
                            foreach (var project in projectGroup.OrderByDescending(o => o.ScoreInitial))
                            {
                                project.RankInitial = i++;
                            }
                        }
                    }
                    else
                    {
                        projects = projects.OrderByDescending(o => o.ScoreInitial).ToList();
                        foreach (var project in projects)
                        {
                            project.RankInitial = projects.IndexOf(project) + 1;
                        }
                    }
                }
                if (reviewType == ReviewType.Finish)
                {
                    if (prize.PrizeType == Prizes.PrizeType.Major)
                    {
                        //专业类需要分专业排名
                        var projectGroups = projects.GroupBy(o => o.PrizeSubMajorId);
                        foreach (var projectGroup in projectGroups)
                        {
                            var i = 1;
                            foreach (var project in projectGroup.OrderByDescending(o => o.ScoreFinal))
                            {
                                project.RankFinal = i++;
                            }
                        }
                    }
                    else
                    {
                        projects = projects.OrderByDescending(o => o.ScoreFinal).ToList();
                        foreach (var project in projects)
                        {
                            project.RankFinal = projects.IndexOf(project) + 1;
                        }
                    }
                }
            }
        }

        #endregion 评审活动

        #region 评选数据

        /// <summary>
        /// 获取评选的所有轮次项目得分数据
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        public virtual async Task<List<ProjectReviewSummary>> GetAllProjectRanks(Review review)
        {
            return await CacheManager.GetCache<int, List<ProjectReviewSummary>>("ProjectReviewSummary")
                .GetAsync(review.Id, async (id) =>
                {
                    var result = new List<ProjectReviewSummary>();
                    //20181104只计算轮的分数
                    foreach (var reviewRound in review.ReviewRounds.Where(o => o.Turn == 1))
                    {
                        result.AddRange(await GetProjectRanks(reviewRound));
                    }
                    return result;
                });

            //var result = new List<ProjectReviewSummary>();
            ////20181104只计算轮的分数
            //foreach(var reviewRound in review.ReviewRounds.Where(o=>o.Turn==1))
            //{
            //    result.AddRange(await GetProjectRanks(reviewRound));
            //}
            //return result;
        }

        /// <summary>
        /// 获取项目得分详情列表
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<ProjectReviewSummary>> GetProjectRanks(ReviewRound reviewRound, int r = 0)
        {
            //本轮首次得分
            //var mainreviewdetail = Config.Helper.Single<ReviewDetail>("where reviewid=@0 and round=@1 and turn=1", ReviewID, r == 0 ? Round : r);
            var mainReviewDetail = reviewRound.Review.ReviewRounds.Single(o => o.Turn == 1 && o.Round == (r == 0 ? reviewRound.Round : r));
            //var otherreviewdetails = Config.Helper.CreateWhere<ReviewDetail>()
            //    .Where(p => p.ReviewID == ReviewID && p.Round == mainreviewdetail.Round && p.Turn > 1 && p.ReviewStatus == ReviewStatus.Reviewed).Select();
            //modi20181104未完成的评审活动的数据也需要
            var otherReviewDetails = reviewRound.Review.ReviewRounds.Where(o => o.Round == mainReviewDetail.Round && o.Turn > 1 /*&& o.ReviewStatus == ReviewStatus.Reviewed*/).OrderBy(o => o.Turn).ToList();
            //上轮得分
            List<ProjectReviewSummary> lastProjectRanks = null;
            //参数
            var methodsetting = mainReviewDetail.ReviewMethodSetting;

            if (mainReviewDetail.ReviewMethod == ReviewMethod.Weighting)
            {
                lastProjectRanks = await GetProjectRanks(reviewRound, mainReviewDetail.Round - 1);
            }
            //var sourceProjects = Config.Helper.CreateWhere<Project>()
            //    .AddWhereSql("id in(" + mainreviewdetail.SourceProjectIDs + ")")
            //    .Select();
            var sourceProjectIds = mainReviewDetail.SourceProjectIDs.Split(',').Select(s => int.Parse(s));
            var sourceProjects = await ProjectRepository.GetAll().Where(o => sourceProjectIds.Contains(o.Id)).ToListAsync();

            //var projectdetails = mainreviewdetail.VoteDetails.SelectMany(o => o.VoteProjectDetail);
            var projectDetails = mainReviewDetail.ExpertReviewDetails.Where(o => o.FinishTime != null).SelectMany(o => o.ProjectReviewDetails);
            var result = sourceProjects.Select(o =>
            {
                //var awardproject = Config.Helper.CreateWhere<AwardProject>()
                //.Where(a => a.AwardID == review.AwardID && a.ProjectID == o.ID)
                //.SingleOrDefault();
                var reviewProject = reviewRound.Review.ReviewProjects.Single(p => p.Id == o.Id);

                var obj = new ProjectReviewSummary();
                obj.Id = o.Id;
                obj.Round = reviewRound.Round;
                obj.MaxScore = reviewRound.ReviewMethodSetting.MaxScore;
                obj.ProjectName = o.ProjectName;
                obj.PrizeName = o.Prize.PrizeName;
                obj.SubMajorName = o.PrizeSubMajor?.Major.BriefName;
                obj.DesignOrganizationName = o.DesignOrganization.BriefName;
                obj.Sort = reviewProject.Sort;//排序
                obj.NeedConfirm = false;//同分标记
                //得分
                if (mainReviewDetail.ReviewMethod == ReviewMethod.Vote || mainReviewDetail.ReviewMethod == ReviewMethod.VetoSystem)
                {
                    obj.Score = projectDetails.Where(p => p.ProjectId == o.Id).Sum(p =>
                    {
                        if (p.IsAvoid) { return 0; }
                        return p.VoteFlag ? 1 : 0;
                    });
                }
                else
                {
                    var scores = projectDetails.Where(p => p.ProjectId == o.Id && !p.IsAvoid).Select(p => p.Score.Value).ToList();
                    //如果是第一轮第一次的平均分计算，且存在基础分，则将基础分加入
                    if (mainReviewDetail.Round == 1 && reviewProject.BaseScore != null)
                    {
                        scores.Add(Convert.ToDecimal(reviewProject.BaseScore));
                    }
                    obj.Score = Math.Round(CalculateScore(scores, mainReviewDetail.ReviewMethodSetting), 4);
                    //obj.Score =Math.Round( projectdetails.Where(p => p.ProjectID == o.ID && !p.IsAvoid).Average(p =>
                    //{
                    //    return p.Score;
                    //}),4);
                    obj.OriScore = obj.Score;
                    //与上轮加权的还需要再计算
                    if (mainReviewDetail.ReviewMethod == ReviewMethod.Weighting && mainReviewDetail.Round > 1)
                    {
                        var lastroundscore = lastProjectRanks.Single(p => p.Id == o.Id).Score;
                        obj.Score = Math.Round(lastroundscore * methodsetting.WeightLast / 100 + obj.Score * methodsetting.WeightNow / 100, 4);
                    }
                }
                obj.TotalScore = obj.Score * 1000;//TotalScore用来存本轮的总分用来排序
                                                  //需要将同轮下面几次的分数也列出

                for (var i = 0; i < otherReviewDetails.Count; i++)
                {
                    var p = otherReviewDetails[i];
                    var subscore = p.ExpertReviewDetails.Where(e => e.FinishTime != null).SelectMany(v => v.ProjectReviewDetails).Where(v => v.ProjectId == o.Id).Sum(d =>
                    {
                        if (d.IsAvoid) { return 0; }
                        return d.VoteFlag ? 1 : 0;
                    });
                    obj.SubScores.Add(subscore);
                    obj.TotalScore += Convert.ToDecimal(subscore * Math.Pow(10, -i));
                }

                #region old

                //otherReviewDetails.ForEach(p =>
                //{
                //    var subscore = p.ExpertReviewDetails.Where(e => e.FinishTime != null).SelectMany(v => v.ProjectReviewDetails).Where(v => v.ProjectId == o.Id).Sum(d =>
                //        {
                //            if (d.IsAvoid) { return 0; }
                //            return d.VoteFlag ? 1 : 0;
                //        });
                //    obj.SubScores.Add(subscore);
                //    obj.TotalScore += subscore;
                //});

                #endregion old

                return obj;
            }).OrderByDescending(o => o.TotalScore).ToList();

            return result;
        }

        private decimal CalculateScore(IEnumerable<decimal> scores, ReviewMethodSetting setting)
        {
            if (scores.Count() == 0)
            {
                return 0;
            }
            var score = scores.Average();
            //仅当去掉最高最低分且打分数大于2
            if (setting.CutOff && scores.Count() > 2)
            {
                var max = scores.Max();
                var min = scores.Min();
                score = (scores.Sum() - max - min) / (scores.Count() - 2);
            }
            return score;
        }

        #endregion 评选数据

        #region 评选结果

        /// <summary>
        /// 重新生成评选结果
        /// </summary>
        /// <param name="matchInstanceId"></param>
        /// <returns></returns>
        public virtual async Task RegenerateResult(int matchInstanceId)
        {
            var maxReviewType = GetAll().Where(o => o.MatchInstanceId == matchInstanceId).Max(o => o.ReviewType);
            var projectQuery = ProjectManager.GetAll().Where(o => o.MatchInstanceId == matchInstanceId && (int)o.ProjectStatus >= 4);
            if (maxReviewType == ReviewType.Finish)
            {
                projectQuery = projectQuery.Where(o => o.IsInFinalReview);
            }
            if (maxReviewType == ReviewType.Champion)
            {
                projectQuery = projectQuery.Where(o => o.IsInChampionReview);
            }
            var projects = await projectQuery.ToListAsync();
            projects.ForEach(o =>
            {
                o.MatchAwardId = null;
                if (o.MaxReviewType == ReviewType.Champion)
                {
                    o.ScoreManual = o.ScoreChampion;
                }
                else if (o.MaxReviewType == ReviewType.Finish)
                {
                    o.ScoreManual = o.ScoreFinal;
                }
                else
                {
                    o.ScoreManual = o.ScoreInitial;
                }
            });
            var orderdProjects = projects.OrderByDescending(o => o.ScoreManual);
            for (var i = 0; i < orderdProjects.Count(); i++)
            {
                orderdProjects.ElementAt(i).RankManual = i + 1;
            }
        }

        #endregion 评选结果
    }
}
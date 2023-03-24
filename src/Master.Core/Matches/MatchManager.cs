using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.UI;
using Master.Authentication;
using Master.Domain;
using Master.Majors;
using Master.Prizes;
using Master.Projects;
using Master.Reviews;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Matches
{
    public class MatchManager : DomainServiceBase<Match, int>
    {
    }

    public class MatchInstanceManager : DomainServiceBase<MatchInstance, int>
    {
        private readonly IRepository<Major, int> _majorRepository;
        private readonly IRepository<Prize, int> _prizeRepository;
        private readonly IRepository<PrizeSubMajor, int> _prizeSubMajorRepository;
        private readonly IRepository<MatchResource, int> _matchResourceRepository;
        private readonly IRepository<Project, int> _projectRepository;
        private readonly IRepository<Review, int> _reviewRepository;

        public MatchInstanceManager(
            IRepository<Major, int> majorRepository,
            IRepository<Prize, int> prizeRepository,
            IRepository<PrizeSubMajor, int> prizeSubMajorRepository,
            IRepository<MatchResource, int> matchResourceRepository,
            IRepository<Project, int> projectRepository,
            IRepository<Review, int> reviewRepository
            )
        {
            _majorRepository = majorRepository;
            _prizeRepository = prizeRepository;
            _matchResourceRepository = matchResourceRepository;
            _prizeSubMajorRepository = prizeSubMajorRepository;
            _projectRepository = projectRepository;
            _reviewRepository = reviewRepository;
        }

        /// <summary>
        /// 赛事实例的状态变化
        /// </summary>
        /// <param name="matchInstance"></param>
        /// <param name="oriStatus"></param>
        /// <returns></returns>
        public async Task ChangeMatchInstanceStatus(MatchInstance matchInstance, MatchInstanceStatus oriStatus)
        {
            if (matchInstance.MatchInstanceStatus == oriStatus)
            {
                return;
            }
            //草稿状态
            if (matchInstance.MatchInstanceStatus == MatchInstanceStatus.Draft)
            {
                //如果有项目申报过,则不允许切换回草稿状态
                if (await _projectRepository.CountAsync(o => o.MatchInstanceId == matchInstance.Id) > 0)
                {
                    throw new UserFriendlyException("赛事已发布，不可再修改");
                }
                //清空所有赛事数据
                await ResetMatchInstance(matchInstance);
            }
            //申报中
            if (matchInstance.MatchInstanceStatus == MatchInstanceStatus.Applying)
            {
                //1.从草稿变为申报中
                if (oriStatus == MatchInstanceStatus.Draft)
                {
                    //需要检测专业分类和奖项是否有录入
                    var majorCount = await _majorRepository.CountAsync(o => o.MatchId == matchInstance.MatchId && o.IsActive);
                    var prizeCount = await _prizeRepository.CountAsync(o => o.MatchId == matchInstance.MatchId && o.IsActive);
                    if (majorCount == 0 || prizeCount == 0)
                    {
                        throw new UserFriendlyException("赛事必须有有效的专业分类及奖项才能发布");
                    }
                    //同步所有赛事配置至赛事实例
                    await SyncMatchInstanceResource(matchInstance);
                }
                //2.从其它状态变为申报中
                else
                {
                    //如果有评选活动发布，则抛出异常
                    var reviewCount = await _reviewRepository.CountAsync(o => o.MatchInstanceId == matchInstance.Id);
                    if (reviewCount > 0)
                    {
                        throw new UserFriendlyException("赛事已经发布评选活动,不能重新申报");
                    }
                }
            }
            //评选中
            //if (matchInstance.MatchInstanceStatus == MatchInstanceStatus.Reviewing)
            //{
            //    //如果没有项目申报，则抛出异常
            //    if (await _projectRepository.CountAsync(o => o.MatchInstanceId == matchInstance.Id) == 0)
            //    {
            //        throw new UserFriendlyException("尚未有项目申报此赛事，无法进入评选");
            //    }
            //}
        }

        /// <summary>
        /// 重置赛事实例，清空所有赛事实例相关数据
        /// </summary>
        /// <param name="matchInstance"></param>
        /// <returns></returns>
        public virtual async Task ResetMatchInstance(MatchInstance matchInstance)
        {
            //清空所有赛事实例相关配置数据
            //奖项
            await _prizeRepository.DeleteAsync(o => o.MatchInstanceId == matchInstance.Id);
            await CurrentUnitOfWork.SaveChangesAsync();
            //评审信息
            await _reviewRepository.DeleteAsync(o => o.MatchInstanceId == matchInstance.Id);
            //专业分类
            await _majorRepository.DeleteAsync(o => o.MatchInstanceId == matchInstance.Id);
            //赛事资源
            await _matchResourceRepository.DeleteAsync(o => o.MatchInstanceId == matchInstance.Id);
            //清空赛事所有申报项目信息
            await _projectRepository.DeleteAsync(o => o.MatchInstanceId == matchInstance.Id);

            matchInstance.MatchInstanceStatus = MatchInstanceStatus.Draft;
        }

        /// <summary>
        /// 同步所有赛事配置至赛事实例
        /// </summary>
        /// <param name="matchInstance"></param>
        /// <returns></returns>
        private async Task SyncMatchInstanceResource(MatchInstance matchInstance)
        {
            //专业分类;
            var majorMapDic = new Dictionary<int, int>();//专业分类新老id配对
            var majors = await _majorRepository.GetAll().Where(o => o.MatchId == matchInstance.MatchId).ToListAsync();
            foreach (var major in majors.Where(o => o.ParentId == null))
            {
                var newMajor = new Major(major);
                newMajor.MatchId = null;
                newMajor.MatchInstanceId = matchInstance.Id;
                await _majorRepository.InsertAsync(newMajor);
                await CurrentUnitOfWork.SaveChangesAsync();
                majorMapDic[major.Id] = newMajor.Id;
                //子级
                foreach (var subMajor in majors.Where(o => o.ParentId == major.Id))
                {
                    var newSubMajor = new Major(subMajor);
                    newSubMajor.MatchId = null;
                    newSubMajor.MatchInstanceId = matchInstance.Id;
                    newSubMajor.ParentId = newMajor.Id;
                    await _majorRepository.InsertAsync(newSubMajor);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    majorMapDic[subMajor.Id] = newSubMajor.Id;
                    //三级
                    foreach (var thirdMajor in majors.Where(o => o.ParentId == subMajor.Id))
                    {
                        var newThirdMajor = new Major(thirdMajor);
                        newThirdMajor.MatchId = null;
                        newThirdMajor.MatchInstanceId = matchInstance.Id;
                        newThirdMajor.ParentId = newSubMajor.Id;
                        await _majorRepository.InsertAsync(newThirdMajor);
                        await CurrentUnitOfWork.SaveChangesAsync();
                        majorMapDic[thirdMajor.Id] = newThirdMajor.Id;
                    }
                }
            }
            //奖项
            var prizeMapDic = new Dictionary<int, int>();
            var prizes = await _prizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchId == matchInstance.MatchId).ToListAsync();
            foreach (var prize in prizes)
            {
                var newPrize = new Prize()
                {
                    PrizeName = prize.PrizeName,
                    PrizeType = prize.PrizeType,
                    ExtensionData = prize.ExtensionData,
                    Remarks = prize.Remarks,
                    IsActive = prize.IsActive,
                    MatchInstanceId = matchInstance.Id,
                    MajorId = majorMapDic[prize.MajorId]
                };
                await _prizeRepository.InsertAsync(newPrize);
                await CurrentUnitOfWork.SaveChangesAsync();
                prizeMapDic[prize.Id] = newPrize.Id;

                foreach (var prizeSubMajor in prize.PrizeSubMajors)
                {
                    var newPrizeSubMajor = new PrizeSubMajor()
                    {
                        PrizeId = newPrize.Id,
                        MajorId = majorMapDic[prizeSubMajor.MajorId],
                        Percent = prizeSubMajor.Percent,
                        Checked = prizeSubMajor.Checked,
                        Ratio = prizeSubMajor.Ratio
                    };
                    await _prizeSubMajorRepository.InsertAsync(newPrizeSubMajor);
                }
            }
            //上传清单
            //下载清单
            //评分表
            //表单设计
            var matchResources = await _matchResourceRepository.GetAll().Where(o => o.MatchId == matchInstance.MatchId).ToListAsync();
            foreach (var matchResource in matchResources)
            {
                var newMatchResource = new MatchResource()
                {
                    MatchInstanceId = matchInstance.Id,
                    MajorId = majorMapDic[matchResource.MajorId],
                    MatchResourceStatus = matchResource.MatchResourceStatus,
                    MatchResourceType = matchResource.MatchResourceType,
                    ExtensionData = matchResource.ExtensionData,
                };
                if (matchResource.SubMajorId != null)
                {
                    newMatchResource.SubMajorId = majorMapDic[matchResource.SubMajorId.Value];
                }
                await _matchResourceRepository.InsertAsync(newMatchResource);
            }

            //保存映射关系
            matchInstance.SetData("MajorMapDic", majorMapDic);
            matchInstance.SetData("PrizeMapDic", prizeMapDic);
            return;
        }

        /// <summary>
        /// 增量同步所有赛事配置至赛事实例
        /// </summary>
        /// <param name="matchInstance"></param>
        /// <returns></returns>
        public async Task ReSyncMatchInstanceResource(MatchInstance matchInstance)
        {
            //专业分类;
            var majorMapDic = matchInstance.GetData<Dictionary<int, int>>("MajorMapDic");//专业分类新老id配对
            var majors = await _majorRepository.GetAll().Where(o => o.MatchId == matchInstance.MatchId).ToListAsync();
            foreach (var major in majors.Where(o => o.ParentId == null))
            {
                Major newMajor = null;
                //是否已存在已同步的专业
                if (majorMapDic.ContainsKey(major.Id))
                {
                    //更新原有赛事实例专业
                    newMajor = await _majorRepository.GetAsync(majorMapDic[major.Id]);
                    newMajor.LoadFrom(major);
                    newMajor.MatchId = null;
                    newMajor.MatchInstanceId = matchInstance.Id;
                    await _majorRepository.UpdateAsync(newMajor);
                }
                else
                {
                    //新增赛事实例专业
                    newMajor = new Major(major);
                    newMajor.MatchId = null;
                    newMajor.MatchInstanceId = matchInstance.Id;
                    await _majorRepository.InsertAsync(newMajor);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                majorMapDic[major.Id] = newMajor.Id;
                //子级
                foreach (var subMajor in majors.Where(o => o.ParentId == major.Id))
                {
                    Major newSubMajor = null;
                    //是否已存在已同步的专业
                    if (majorMapDic.ContainsKey(subMajor.Id))
                    {
                        //更新原有赛事实例专业
                        newSubMajor = await _majorRepository.GetAsync(majorMapDic[subMajor.Id]);
                        newSubMajor.LoadFrom(subMajor);
                        newSubMajor.MatchId = null;
                        newSubMajor.MatchInstanceId = matchInstance.Id;
                        newSubMajor.ParentId = newMajor.Id;
                        await _majorRepository.UpdateAsync(newSubMajor);
                    }
                    else
                    {
                        //新增赛事实例专业
                        newSubMajor = new Major(subMajor);
                        newSubMajor.MatchId = null;
                        newSubMajor.MatchInstanceId = matchInstance.Id;
                        newSubMajor.ParentId = newMajor.Id;
                        await _majorRepository.InsertAsync(newSubMajor);
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                    majorMapDic[subMajor.Id] = newSubMajor.Id;
                    //三级
                    foreach (var thirdMajor in majors.Where(o => o.ParentId == subMajor.Id))
                    {
                        Major newThirdMajor = null;
                        //是否已存在已同步的专业
                        if (majorMapDic.ContainsKey(thirdMajor.Id))
                        {
                            //更新原有赛事实例专业
                            newThirdMajor = await _majorRepository.GetAsync(majorMapDic[thirdMajor.Id]);
                            newThirdMajor.LoadFrom(thirdMajor);
                            newThirdMajor.MatchId = null;
                            newThirdMajor.MatchInstanceId = matchInstance.Id;
                            newThirdMajor.ParentId = newSubMajor.Id;
                            await _majorRepository.UpdateAsync(newThirdMajor);
                        }
                        else
                        {
                            //新增赛事实例专业
                            newThirdMajor = new Major(thirdMajor);
                            newThirdMajor.MatchId = null;
                            newThirdMajor.MatchInstanceId = matchInstance.Id;
                            newThirdMajor.ParentId = newSubMajor.Id;
                            await _majorRepository.InsertAsync(newThirdMajor);
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                        majorMapDic[thirdMajor.Id] = newThirdMajor.Id;
                    }
                }
            }
            //奖项
            var prizeMapDic = matchInstance.GetData<Dictionary<int, int>>("PrizeMapDic");//奖项新老id配对
            var prizes = await _prizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchId == matchInstance.MatchId).ToListAsync();
            foreach (var prize in prizes)
            {
                Prize newPrize = null;
                //是否已存在已同步的奖项
                if (prizeMapDic.ContainsKey(prize.Id))
                {
                    newPrize = await _prizeRepository.GetAsync(prizeMapDic[prize.Id]);
                    newPrize.PrizeName = prize.PrizeName;
                    newPrize.PrizeType = prize.PrizeType;
                    newPrize.ExtensionData = prize.ExtensionData;
                    newPrize.Remarks = prize.Remarks;
                    newPrize.IsActive = prize.IsActive;
                    newPrize.MatchInstanceId = matchInstance.Id;
                    newPrize.MajorId = majorMapDic[prize.MajorId];
                }
                else
                {
                    newPrize = new Prize()
                    {
                        PrizeName = prize.PrizeName,
                        PrizeType = prize.PrizeType,
                        ExtensionData = prize.ExtensionData,
                        Remarks = prize.Remarks,
                        IsActive = prize.IsActive,
                        MatchInstanceId = matchInstance.Id,
                        MajorId = majorMapDic[prize.MajorId]
                    };
                    await _prizeRepository.InsertAsync(newPrize);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                prizeMapDic[prize.Id] = newPrize.Id;
                foreach (var prizeSubMajor in prize.PrizeSubMajors)
                {
                    var newPrizeSubMajor = await _prizeSubMajorRepository.GetAll()
                        .Where(o => o.PrizeId == newPrize.Id && o.MajorId == majorMapDic[prizeSubMajor.MajorId]).FirstOrDefaultAsync();
                    if (newPrizeSubMajor == null)
                    {
                        newPrizeSubMajor = new PrizeSubMajor()
                        {
                            PrizeId = newPrize.Id,
                            MajorId = majorMapDic[prizeSubMajor.MajorId],
                            Percent = prizeSubMajor.Percent,
                            Checked = prizeSubMajor.Checked,
                            Ratio = prizeSubMajor.Ratio
                        };
                        await _prizeSubMajorRepository.InsertAsync(newPrizeSubMajor);
                    }
                    else
                    {
                        newPrizeSubMajor.Percent = prizeSubMajor.Percent;
                        newPrizeSubMajor.Checked = prizeSubMajor.Checked;
                        newPrizeSubMajor.Ratio = prizeSubMajor.Ratio;
                        await _prizeSubMajorRepository.UpdateAsync(newPrizeSubMajor);
                    }
                }
            }
            //上传清单
            //下载清单
            //评分表
            //表单设计
            //先清空所有赛事资源
            await _matchResourceRepository.DeleteAsync(o => o.MatchId == null && o.MatchInstanceId == matchInstance.Id);

            var matchResources = await _matchResourceRepository.GetAll().Where(o => o.MatchId == matchInstance.MatchId).ToListAsync();
            foreach (var matchResource in matchResources)
            {
                var newMatchResource = new MatchResource()
                {
                    MatchInstanceId = matchInstance.Id,
                    MajorId = majorMapDic[matchResource.MajorId],
                    MatchResourceStatus = matchResource.MatchResourceStatus,
                    MatchResourceType = matchResource.MatchResourceType,
                    ExtensionData = matchResource.ExtensionData,
                };
                if (matchResource.SubMajorId != null)
                {
                    newMatchResource.SubMajorId = majorMapDic[matchResource.SubMajorId.Value];
                }
                await _matchResourceRepository.InsertAsync(newMatchResource);
            }
            //保存映射关系
            matchInstance.SetData("MajorMapDic", majorMapDic);
            matchInstance.SetData("PrizeMapDic", prizeMapDic);

            return;
        }

        /// <summary>
        /// 根据导出类型获取导出根相对路径
        /// </summary>
        /// <param name="matchInstanceName"></param>
        /// <param name="exportType"></param>
        /// <returns></returns>
        public async Task<string> GetExportRootVirtualPath(MatchInstance matchInstance, string exportType = "")
        {
            string result = $"/MatchInstance/{matchInstance.Name}/项目";
            switch (exportType)
            {
                case "org":
                    //子公司导出
                    var user = await IocManager.Instance.Resolve<UserManager>().GetAll().Include(o => o.Organization).Where(o => o.Id == AbpSession.UserId.Value).FirstAsync();
                    result = $"/MatchInstance/{matchInstance.Name}/{user.Organization.BriefName}";
                    break;

                case "major":
                    //大专业导出
                    var majors = await IocManager.Instance.Resolve<MajorManager>().GetChargerMajors(AbpSession.UserId.Value, matchInstance.Id);
                    result = $"/MatchInstance/{matchInstance.Name}/{majors.Select(o => o.BriefName).JoinAsString("-")}";
                    break;

                default:
                    break;
            }
            return result;
        }
    }
}
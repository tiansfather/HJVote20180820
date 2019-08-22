using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Web.Models;
using Master.Dto;
using Master.Projects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Notices
{
    public class NoticeAppService : MasterAppServiceBase<Notice, int>
    {
        
        [DontWrapResult]
        public override async Task<ResultPageDto> GetPageResult(RequestPageDto request)
        {
            var pageResult = await GetPageResultQueryable(request);
            var data = await pageResult.Queryable
                .Select(o => new {
                    o.Id,
                    o.Title,
                    CreationTime=o.CreationTime.ToString("yyyy-MM-dd HH:mm"),
                    LastModificationTime=o.LastModificationTime!=null? o.LastModificationTime.Value.ToString("yyyy-MM-dd HH:mm"):"",
                    PublishTime= o.PublishTime != null ? o.PublishTime.Value.ToString("yyyy-MM-dd HH:mm") : "",
                    PublishTimeShort = o.PublishTime != null ? o.PublishTime.Value.ToString("yyyy-MM-dd") : "",
                    o.NoticeStatus
                }).ToListAsync();

            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }
        public virtual async Task<NoticeDto> GetNotice(int noticeId)
        {
            var notice = await Repository.GetAsync(noticeId);
            var noticeDto = notice.MapTo<NoticeDto>();
            noticeDto.Datas = notice.GetData<List<ProjectFile>>("Datas");

            return noticeDto;
        }
        /// <summary>
        /// 提交公告
        /// </summary>
        /// <param name="noticeDto"></param>
        /// <returns></returns>
        public virtual async Task SubmitNotice(NoticeDto noticeDto)
        {
            Notice notice = null;
            if (noticeDto.Id == 0)
            {
                notice = noticeDto.MapTo<Notice>();
                notice.SetData("Datas", noticeDto.Datas);
                await Repository.InsertAsync(notice);
            }
            else
            {
                notice = await Repository.GetAsync(noticeDto.Id);
                noticeDto.MapTo(notice);
                notice.SetData("Datas", noticeDto.Datas);
                await Repository.UpdateAsync(notice);
            }
            //记录发布时间
            if(notice.NoticeStatus==NoticeStatus.Publish && notice.PublishTime == null)
            {
                notice.PublishTime = DateTime.Now;
            }
            else
            {
                notice.PublishTime = null;
            }
        }
    }
}

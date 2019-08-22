using Abp.UI;
using Abp.Web.Models;
using Master.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Majors
{
    public class SpecialityAppService : MasterAppServiceBase<Speciality, int>
    {
        public virtual async Task Add(string text)
        {
            if (Manager.Repository.Count(o => o.Name == text) > 0)
            {
                throw new UserFriendlyException("相同专业名称已存在");
            }
            var speciality = new Speciality()
            {
                Name = text
            };
            if (await Repository.CountAsync() == 0)
            {
                speciality.Sort = 1;
            }
            else
            {
                var maxOrder = await Manager.Repository.GetAll().MaxAsync(o => o.Sort);
                speciality.Sort = maxOrder + 1;
            }
            
            
            await Manager.InsertAsync(speciality);
        }
        public virtual async Task Update(int id, string text)
        {
            //是否重复判定
            if (await Repository.CountAsync(o => o.Id != id && o.Name == text) > 0)
            {
                throw new UserFriendlyException("相同专业名称已存在");
            }
            var speciality = await Manager.GetByIdAsync(id);
            speciality.Name = text;
        }
        public virtual async Task SetSort(int id,int sort)
        {
            var speciality = await Manager.GetByIdAsync(id);
            speciality.Sort = sort;
        }
        [DontWrapResult]
        public virtual async Task<ResultPageDto> GetAll(RequestPageDto request)
        {
            var specialities = (await Manager.Repository.GetAll().Select(o => new { o.Id, o.Name ,o.Sort}).ToListAsync()).OrderBy(o=>o.Sort);

            var result = new ResultPageDto()
            {
                code = 0,
                count = specialities.Count(),
                data = specialities
            };

            return result;
        }
    }
}

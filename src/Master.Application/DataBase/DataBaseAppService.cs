using Abp.Domain.Uow;
using Abp.Web.Models;
using Master.Dto;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.DataBase
{
    public class DataBaseAppService: MasterAppServiceBase
    {
        private IHostingEnvironment _hostingEnvironment;
        public BackUpManager BackUpManager { get; set; }
        public DataBaseAppService(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        [DontWrapResult]
        public virtual ResultPageDto GetPageResult(RequestPageDto requestPageDto)
        {
            var path = _hostingEnvironment.ContentRootPath + "\\App_Data\\BackUp\\";
            var files = System.IO.Directory.GetFiles(path).Select(o =>
            {
                var fileName = System.IO.Path.GetFileName(o);
                var creationTime = System.IO.File.GetCreationTime(o).ToString("yyyy-MM-dd HH:mm:ss");
                return new { fileName, creationTime };
            }).OrderByDescending(o=>o.creationTime);

            return new ResultPageDto()
            {
                code = 0,
                count = files.Count(),
                data = files.Skip((requestPageDto.Page-1)*requestPageDto.Limit).Take(requestPageDto.Limit)
            };
        }
        [UnitOfWork(false)]
        public virtual async Task BackUp()
        {
            await BackUpManager.BackUp();
        }

        public virtual async Task DoDo()
        {

        }

        public virtual void Delete(IEnumerable<string> fileNames)
        {
            var path = _hostingEnvironment.ContentRootPath + "\\App_Data\\BackUp\\";
            foreach(var fileName in fileNames)
            {
                var filePath = System.IO.Path.Combine(path, fileName);
                System.IO.File.Delete(filePath);
            }
            
        }
    }
}

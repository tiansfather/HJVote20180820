using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using Master.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Master
{
    public class BackUpManager:ITransientDependency
    {
        private readonly IRepository<User, long> _userRepository;
        private IHostingEnvironment _hostingEnvironment;

        public BackUpManager(IRepository<User, long> userRepository, IHostingEnvironment hostingEnvironment)
        {
            _userRepository = userRepository;
            _hostingEnvironment = hostingEnvironment;
        }
        [UnitOfWork(false)]
        public virtual async Task BackUp()
        {
            var dbContext = _userRepository.GetDbContext();
            var path = _hostingEnvironment.ContentRootPath + "\\App_Data\\BackUp\\";
            System.IO.Directory.CreateDirectory(path);
            //path = @"D:\";
            await dbContext.Database.ExecuteSqlCommandAsync("exec p_shrinkdb");
            await dbContext.Database.ExecuteSqlCommandAsync($"exec p_backupdb '',@p0", path);
        }

        public virtual bool NeedBackUp()
        {
            var needBackup = true;
            var path = _hostingEnvironment.ContentRootPath + "\\App_Data\\BackUp\\";
            foreach(var file in System.IO.Directory.GetFiles(path))
            {
                if ((DateTime.Now - System.IO.File.GetCreationTime(file)).Days <=0)
                {
                    needBackup = false;
                }
            }
            return needBackup;
        }
    }
}

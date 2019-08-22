using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Master.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.EntityFrameworkCore
{
    public class BackUpWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private BackUpManager _backUpManager;
        public BackUpWorker(AbpTimer timer, BackUpManager backUpManager)
            : base(timer)
        {
            _backUpManager = backUpManager;
            Timer.Period = 1000*60*60; //一小时执行一次
        }

        [UnitOfWork(false)]
        protected override void DoWork()
        {
            if (_backUpManager.NeedBackUp())
            {
                _backUpManager.BackUp().GetAwaiter().GetResult();
            }
            
        }
    }
}

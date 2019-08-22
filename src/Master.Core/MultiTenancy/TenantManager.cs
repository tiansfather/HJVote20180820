using Abp.UI;
using Master.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Master.MultiTenancy
{
    public class TenantManager: DomainServiceBase<Tenant,int>
    {
        /// <summary>
        /// 获取所有帐套
        /// </summary>
        /// <returns></returns>
        public async Task<List<Tenant>> All()
        {
            return await GetAll().ToListAsync();
        }

        public override async Task InsertAsync(Tenant tenant)
        {

            if (await Repository.FirstOrDefaultAsync(t => t.TenancyName == tenant.TenancyName) != null)
            {
                throw new UserFriendlyException(string.Format(L("TenancyNameIsAlreadyTaken"), tenant.TenancyName));
            }

            await base.InsertAsync(tenant);
        }

        public override async Task UpdateAsync(Tenant tenant)
        {
            if (await Repository.FirstOrDefaultAsync(t => t.TenancyName == tenant.TenancyName && t.Id != tenant.Id) != null)
            {
                throw new UserFriendlyException(string.Format(L("TenancyNameIsAlreadyTaken"), tenant.TenancyName));
            }

            await base.UpdateAsync(tenant);
        }
    }
}

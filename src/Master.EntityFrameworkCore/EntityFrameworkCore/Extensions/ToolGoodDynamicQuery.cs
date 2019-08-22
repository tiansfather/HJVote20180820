using Abp.Runtime.Session;
using Master.Domain;
using Master.MultiTenancy;
using System;
using System.Collections.Generic;
using System.Text;
using ToolGood.ReadyGo3;

namespace Master.EntityFrameworkCore.Extensions
{
    public class ToolGoodDynamicQuery : IDynamicQuery
    {
        private readonly IDbPerTenantConnectionStringResolver _connectionStringResolver;
        public IAbpSession AbpSession { get; set; }
        public ToolGoodDynamicQuery(IDbPerTenantConnectionStringResolver connectionStringResolver)
        {
            _connectionStringResolver = connectionStringResolver;
            AbpSession = NullAbpSession.Instance;
        }

        private string GetDbConnString()
        {
            return _connectionStringResolver.GetNameOrConnectionString(new DbPerTenantConnectionStringResolveArgs(AbpSession.TenantId));
        }

        public IEnumerable<dynamic> Select(string sql)
        {

            using (var helper = GetSqlHelper())
            {
                return helper.Select<dynamic>(sql);
            }
        }

        public T Single<T>(string sql)
        {
            using (var helper = GetSqlHelper())
            {
                return helper.Single<T>(sql);
            }
        }

        public SqlHelper GetSqlHelper()
        {
            var connStr = GetDbConnString();
            return SqlHelperFactory.OpenDatabase(connStr, "", ToolGood.ReadyGo3.SqlType.MySql);
        }
    }
}

using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Domain
{
    public interface IDynamicQuery : ITransientDependency
    {
        /// <summary>
        /// 动态查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        IEnumerable<dynamic> Select(string sql);

        T Single<T>(string sql);
    }
}

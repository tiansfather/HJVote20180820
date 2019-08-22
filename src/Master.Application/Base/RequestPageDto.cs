using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Dto
{
    public class RequestPageDto:RequestDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public string Where { get; set; }
        /// <summary>
        /// 通用高级检索
        /// </summary>
        public string SearchCondition { get; set; }
        /// <summary>
        /// 页面内置检索
        /// </summary>
        public string SearchKeys { get; set; }
        /// <summary>
        /// 排序字段
        /// </summary>
        public string OrderField { get; set; }
        /// <summary>
        /// 排序方式:asc,desc
        /// </summary>
        public string OrderType { get; set; }
    }
}

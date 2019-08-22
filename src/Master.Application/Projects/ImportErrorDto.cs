using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Projects
{
    /// <summary>
    /// 导入错误模型
    /// </summary>
    public class ImportErrorDto
    {
        public int? Row { get; set; }
        public int? Column { get; set; }
        public string Message { get; set; }
    }
}

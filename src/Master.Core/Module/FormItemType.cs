using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Master.Module
{
    public enum FormItemType
    {
        [Display(Name = "文本")]
        Text =1,
        [Display(Name = "下拉列表")]
        Select =2,
        [Display(Name = "单选")]
        Radio =3,
        [Display(Name = "多选")]
        MultiSelect =4,
        [Display(Name = "多行文本")]
        TextArea =5,
        [Display(Name = "数值")]
        Number =6,
        [Display(Name = "开关")]
        Switch =7,
        [Display(Name = "日期时间")]
        DateTime
    }
}

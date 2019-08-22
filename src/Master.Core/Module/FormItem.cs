using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Module
{
    public class FormItem
    {
        public virtual string ItemName { get; set; }
        public virtual string ItemKey { get; set; }
        public virtual string DefaultValue { get; set; }
        public virtual string VerifyRules { get; set; }
        public virtual string Tips { get; set; }
        public virtual bool IsInline { get; set; }
        public virtual FormItemType FormItemType { get; set; }
    }
}

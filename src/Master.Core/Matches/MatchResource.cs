using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Master.Majors;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Master.Matches
{
    public class MatchResource : CreationAuditedEntity<int>, IExtendableObject
    {
        public virtual int? MatchId { get; set; }
        public virtual Match Match { get; set; }
        public virtual int? MatchInstanceId { get; set; }
        public virtual MatchInstance MatchInstance { get; set; }
        public virtual int MajorId { get; set; }
        public virtual Major Major {get;set;}
        public virtual int? SubMajorId { get; set; }
        [ForeignKey("SubMajorId")]
        public virtual Major SubMajor { get; set; }
        public virtual string ExtensionData { get; set; }
        public virtual MatchResourceType MatchResourceType { get; set; }
        public virtual MatchResourceStatus MatchResourceStatus { get; set; }
    }

    public enum MatchResourceType
    {
        /// <summary>
        /// 表单设计
        /// </summary>
        DynamicForm=1,
        /// <summary>
        /// 上传表单
        /// </summary>
        UploadList=2,
        /// <summary>
        /// 样例下载
        /// </summary>
        DownloadList=3,
        /// <summary>
        /// 评分表
        /// </summary>
        RateTable=4
    }
    public enum MatchResourceStatus
    {
        Draft=1,
        Publish=2
    }
    /// <summary>
    /// 上传清单
    /// </summary>
    public class MatchResourceUploadList
    {
        public int Sort { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public bool FileMust { get; set; }
        public string Identifier { get; set; }
    }
    /// <summary>
    /// 下载样例
    /// </summary>
    public class MatchResourceDownloadList
    {
        public int Sort { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        /// <summary>
        /// 附件上传区域：2，数据填写区域：1
        /// </summary>
        public int FileLocation { get; set; }
    }
    /// <summary>
    /// 评分表
    /// </summary>
    public class MatchResourceRateTable
    {
        public int Sort { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Percent { get; set; }
        public int TotalScore { get; set; }
    }
    /// <summary>
    /// 动态表单
    /// </summary>
    public class MatchResourceFormDesignItem
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Padding { get; set; }
        public string SelectValues { get; set; }
        public string Span { get; set; }
        public string Color { get; set; }
        public string Background { get; set; }
        public string Align { get; set; }
        public string Name { get; set; }
        public string FormName { get; set; }
        public string Tips { get; set; }
        public bool Required { get; set; }
        public List<MatchResourceFormDesignItem> Children { get; set; }
    }
    //public class FormDesignControl
    //{
    //    public string Id { get; set; }
    //    public string LeftTip { get; set; }
    //    public string RightTip { get; set; }
    //    public bool required { get; set; }
    //    public string Value { get; set; }
    //    public string SelectValues { get; set; }
    //    public int ControlWidth { get; set; }
    //    public FormDesignControlType FormDesignControlType { get; set; }
    //}
    [AutoMap(typeof(MatchResource))]
    public class MatchResource<T> : MatchResource
    {
        public MatchResource(MatchResource matchResource)
        {
            matchResource.MapTo(this);
            this.Datas = this.GetData<List<T>>("Datas");
        }

        public List<T> Datas { get; set; }
        public void Init()
        {
            this.SetData("Datas", Datas);
        }
    }
}

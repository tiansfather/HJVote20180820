using Abp.AutoMapper;
using Abp.Domain.Entities;
using Master.Matches;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Projects
{
    [AutoMap(typeof(ProjectMajorInfo))]
    public class ProjectMajorInfoDto
    {
        public int? MajorId { get; set; }
        public bool FangAn { get; set; }
        public bool ChuShe { get; set; }
        public bool ShiGongTu { get; set; }
        public bool HasOther { get; set; }
        public string Other { get; set; }
        public string Coorperation { get; set; }
        public string Person1 { get; set; }
        public string Person2 { get; set; }
        public string Person3 { get; set; }
        public string Person4 { get; set; }
        public string Person5 { get; set; }
        public string Person6 { get; set; }
        public string Person7 { get; set; }
        public string Person8 { get; set; }
        public string Person9 { get; set; }
        public string Person10 { get; set; }
        public List<ProjectFile> Files { get; set; }
        public List<Matches.MatchResourceFormDesignItem> Layouts { get; set; }

        /// <summary>
        /// 数据复制
        /// </summary>
        /// <param name="majorInfo"></param>
        internal void SyncTo(ProjectMajorInfo majorInfo)
        {
            majorInfo.SetData("FangAn", this.FangAn);
            majorInfo.SetData("ChuShe", this.ChuShe);
            majorInfo.SetData("ShiGongTu", this.ShiGongTu);
            majorInfo.SetData("HasOther", this.HasOther);
            majorInfo.SetData("Other", this.Other);
            majorInfo.SetData("Coorperation", this.Coorperation);
            majorInfo.SetData("Person1", this.Person1);
            majorInfo.SetData("Person2", this.Person2);
            majorInfo.SetData("Person3", this.Person3);
            majorInfo.SetData("Person4", this.Person4);
            majorInfo.SetData("Person5", this.Person5);
            majorInfo.SetData("Person6", this.Person6);
            majorInfo.SetData("Person7", this.Person7);
            majorInfo.SetData("Person8", this.Person8);
            majorInfo.SetData("Person9", this.Person9);
            majorInfo.SetData("Person10", this.Person10);
            majorInfo.SetData("Files", Files);
            majorInfo.SetData("Layouts", Layouts);
        }

        internal void SyncFrom(ProjectMajorInfo majorInfo)
        {
            FangAn = majorInfo.GetData<bool>("FangAn");
            ChuShe = majorInfo.GetData<bool>("ChuShe");
            ShiGongTu = majorInfo.GetData<bool>("ShiGongTu");
            HasOther = majorInfo.GetData<bool>("HasOther");
            Other = majorInfo.GetData<string>("Other");
            Coorperation = majorInfo.GetData<string>("Coorperation");
            Person1 = majorInfo.GetData<string>("Person1");
            Person2 = majorInfo.GetData<string>("Person2");
            Person3 = majorInfo.GetData<string>("Person3");
            Person4 = majorInfo.GetData<string>("Person4");
            Person5 = majorInfo.GetData<string>("Person5");
            Person6 = majorInfo.GetData<string>("Person6");
            Person7 = majorInfo.GetData<string>("Person7");
            Person8 = majorInfo.GetData<string>("Person8");
            Person9 = majorInfo.GetData<string>("Person9");
            Person10 = majorInfo.GetData<string>("Person10");
            Files = majorInfo.GetData<List<ProjectFile>>("Files");
            Layouts = majorInfo.GetData<List<MatchResourceFormDesignItem>>("Layouts");
        }
    }
}

using Abp.Collections.Extensions;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Extensions;
using Master.Entity;
using Master.Matches;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Master.Majors
{
    public class Major : CreationAuditedEntity<int>, IExtendableObject, IHaveRemarks, IHaveSort, IPassivable
    { 
        /// <summary>
        /// Maximum depth of an UO hierarchy.
        /// </summary>
        public const int MaxDepth = 3;
        public const int CodeUnitLength = 5;

        public virtual int? MatchId { get; set; }
        public virtual Match Match { get; set; }
        public virtual int? MatchInstanceId { get; set; }
        public virtual MatchInstance MatchInstance { get; set; }
        public virtual string ExtensionData { get; set; }
        public virtual string Remarks { get; set; }
        [ForeignKey("ParentId")]
        public virtual Major Parent { get; set; }
        public virtual int? ParentId { get; set; }
        public virtual string Code { get; set; }

        public virtual string DisplayName { get; set; }
        /// <summary>
        /// 简称
        /// </summary>
        public virtual string BriefName { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public virtual string BriefCode { get; set; }
        /// <summary>
        /// Children of this OU.
        /// </summary>
        public virtual ICollection<Major> Children { get; set; }
        public int Sort { get; set; }
        public bool IsActive { get; set; }

        public Major()
        {

        }
        public Major (Major major)
        {
            LoadFrom(major);
        }
        public void LoadFrom(Major major)
        {
            MatchId = major.MatchId;
            MatchInstanceId = major.MatchInstanceId;
            Remarks = major.Remarks;
            ExtensionData = major.ExtensionData;
            Code = major.Code;
            BriefCode = major.BriefCode;
            BriefName = major.BriefName;
            ParentId = major.ParentId;
            DisplayName = major.DisplayName;
            IsActive = major.IsActive;
            Sort = major.Sort;
        }
        public Major(string displayName, int? parentId = null)
        {
            DisplayName = displayName;
            ParentId = parentId;
        }

        /// <summary>
        /// Creates code for given numbers.
        /// Example: if numbers are 4,2 then returns "00004.00002";
        /// </summary>
        /// <param name="numbers">Numbers</param>
        public static string CreateCode(params int[] numbers)
        {
            if (numbers.IsNullOrEmpty())
            {
                return null;
            }

            return numbers.Select(number => number.ToString(new string('0', CodeUnitLength))).JoinAsString(".");
        }

        /// <summary>
        /// Appends a child code to a parent code. 
        /// Example: if parentCode = "00001", childCode = "00042" then returns "00001.00042".
        /// </summary>
        /// <param name="parentCode">Parent code. Can be null or empty if parent is a root.</param>
        /// <param name="childCode">Child code.</param>
        public static string AppendCode(string parentCode, string childCode)
        {
            if (childCode.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(childCode), "childCode can not be null or empty.");
            }

            if (parentCode.IsNullOrEmpty())
            {
                return childCode;
            }

            return parentCode + "." + childCode;
        }

        /// <summary>
        /// Gets relative code to the parent.
        /// Example: if code = "00019.00055.00001" and parentCode = "00019" then returns "00055.00001".
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="parentCode">The parent code.</param>
        public static string GetRelativeCode(string code, string parentCode)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            if (parentCode.IsNullOrEmpty())
            {
                return code;
            }

            if (code.Length == parentCode.Length)
            {
                return null;
            }

            return code.Substring(parentCode.Length + 1);
        }

        /// <summary>
        /// Calculates next code for given code.
        /// Example: if code = "00019.00055.00001" returns "00019.00055.00002".
        /// </summary>
        /// <param name="code">The code.</param>
        public static string CalculateNextCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var parentCode = GetParentCode(code);
            var lastUnitCode = GetLastUnitCode(code);

            return AppendCode(parentCode, CreateCode(Convert.ToInt32(lastUnitCode) + 1));
        }

        /// <summary>
        /// Gets the last unit code.
        /// Example: if code = "00019.00055.00001" returns "00001".
        /// </summary>
        /// <param name="code">The code.</param>
        public static string GetLastUnitCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var splittedCode = code.Split('.');
            return splittedCode[splittedCode.Length - 1];
        }

        /// <summary>
        /// Gets parent code.
        /// Example: if code = "00019.00055.00001" returns "00019.00055".
        /// </summary>
        /// <param name="code">The code.</param>
        public static string GetParentCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var splittedCode = code.Split('.');
            if (splittedCode.Length == 1)
            {
                return null;
            }

            return splittedCode.Take(splittedCode.Length - 1).JoinAsString(".");
        }
    }
}

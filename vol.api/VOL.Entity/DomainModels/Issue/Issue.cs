/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果数据库字段发生变化，请在代码生器重新生成此Model
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VOL.Entity.SystemModels;

namespace VOL.Entity.DomainModels
{
    [Entity(TableCnName = "任务信息",TableName = "Issue")]
    public partial class Issue:BaseEntity
    {
        /// <summary>
       ///编号
       /// </summary>
       [Key]
       [Display(Name ="编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long Id { get; set; }

       /// <summary>
       ///项目编号
       /// </summary>
       [Display(Name ="项目编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long ProjectId { get; set; }

       /// <summary>
       ///描述信息
       /// </summary>
       [Display(Name ="描述信息")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Description { get; set; }

       /// <summary>
       ///最新状态
       /// </summary>
       [Display(Name ="最新状态")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? LastestStatus { get; set; }

       /// <summary>
       ///类型
       /// </summary>
       [Display(Name ="类型")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Category { get; set; }

       /// <summary>
       ///优先级
       /// </summary>
       [Display(Name ="优先级")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Priority { get; set; }

       /// <summary>
       ///完成进度
       /// </summary>
       [Display(Name ="完成进度")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Percent { get; set; }

       /// <summary>
       ///最新状态编号
       /// </summary>
       [Display(Name ="最新状态编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? LastestStatusId { get; set; }

       /// <summary>
       ///创建人编号
       /// </summary>
       [Display(Name ="创建人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int CreatorId { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Creator { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime CreationTime { get; set; }

       /// <summary>
       ///修改人编号
       /// </summary>
       [Display(Name ="修改人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? LastModifierId { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string LastModifier { get; set; }

       /// <summary>
       ///修改时间
       /// </summary>
       [Display(Name ="修改时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LastModificationTime { get; set; }

       /// <summary>
       ///删除人编号
       /// </summary>
       [Display(Name ="删除人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? DeleterId { get; set; }

       /// <summary>
       ///删除人
       /// </summary>
       [Display(Name ="删除人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Deleter { get; set; }

       /// <summary>
       ///删除时间
       /// </summary>
       [Display(Name ="删除时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? DeletionTime { get; set; }

       /// <summary>
       ///删除
       /// </summary>
       [Display(Name ="删除")]
       [Column(TypeName="bit")]
       [Editable(true)]
       public bool? IsDeleted { get; set; }

       /// <summary>
       ///事务数量
       /// </summary>
       [Display(Name ="事务数量")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? IssueCounter { get; set; }

       /// <summary>
       ///是否完成
       /// </summary>
       [Display(Name ="是否完成")]
       [Column(TypeName="bit")]
       [Editable(true)]
       public bool? IsFinished { get; set; }

       /// <summary>
       ///完成时间
       /// </summary>
       [Display(Name ="完成时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? FinishTime { get; set; }

       /// <summary>
       ///指定人员编号
       /// </summary>
       [Display(Name ="指定人员编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AssigneeId { get; set; }

       /// <summary>
       ///指定人员
       /// </summary>
       [Display(Name ="指定人员")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Assignee { get; set; }

       /// <summary>
       ///提交人编号
       /// </summary>
       [Display(Name ="提交人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ReporterId { get; set; }

       /// <summary>
       ///提交人员
       /// </summary>
       [Display(Name ="提交人员")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Reporter { get; set; }

       
    }
}
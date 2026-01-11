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
    [Entity(TableCnName = "项目信息",TableName = "Project")]
    public partial class Project:BaseEntity
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
       ///姓名
       /// </summary>
       [Display(Name ="姓名")]
       [MaxLength(32)]
       [Column(TypeName="nvarchar(32)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Name { get; set; }

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
       ///最新状态编号
       /// </summary>
       [Display(Name ="最新状态编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? LastestStatusId { get; set; }

       /// <summary>
       ///开始时间
       /// </summary>
       [Display(Name ="开始时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? StartTime { get; set; }

       /// <summary>
       ///结束时间
       /// </summary>
       [Display(Name ="结束时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? EndTime { get; set; }

       /// <summary>
       ///机构编号
       /// </summary>
       [Display(Name ="机构编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? OrganizationId { get; set; }

       /// <summary>
       ///部门编号
       /// </summary>
       [Display(Name ="部门编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long DepartmentId { get; set; }

       /// <summary>
       ///参与人员数量
       /// </summary>
       [Display(Name ="参与人员数量")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? StaffCounter { get; set; }

       /// <summary>
       ///费用
       /// </summary>
       [Display(Name ="费用")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Cost { get; set; }

       /// <summary>
       ///耗时
       /// </summary>
       [Display(Name ="耗时")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Duration { get; set; }

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

       
    }
}
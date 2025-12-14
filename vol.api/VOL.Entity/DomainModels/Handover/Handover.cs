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
    [Entity(TableCnName = "交接班",TableName = "Handover")]
    public partial class Handover:BaseEntity
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
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Creator { get; set; }

       /// <summary>
       ///审核状态
       /// </summary>
       [Display(Name ="审核状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditStatus { get; set; }

       /// <summary>
       ///结果状态
       /// </summary>
       [Display(Name ="结果状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ResultStatus { get; set; }

       /// <summary>
       ///异常数量
       /// </summary>
       [Display(Name ="异常数量")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AbnormalCounter { get; set; }

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
       ///审核人编号
       /// </summary>
       [Display(Name ="审核人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditUserId { get; set; }

       /// <summary>
       ///审核人
       /// </summary>
       [Display(Name ="审核人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string AuditUser { get; set; }

       /// <summary>
       ///审核时间
       /// </summary>
       [Display(Name ="审核时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? AuditTime { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///创建人编号
       /// </summary>
       [Display(Name ="创建人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreatorId { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreationTime { get; set; }

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
       ///类型
       /// </summary>
       [Display(Name ="类型")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public byte Category { get; set; }

       /// <summary>
       ///关联编号
       /// </summary>
       [Display(Name ="关联编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long RelatedId { get; set; }

       /// <summary>
       ///班次
       /// </summary>
       [Display(Name ="班次")]
       [MaxLength(8)]
       [Column(TypeName="nvarchar(8)")]
       [Editable(true)]
       public string ShiftName { get; set; }

       /// <summary>
       ///交班时间
       /// </summary>
       [Display(Name ="交班时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ShiftTime { get; set; }

       /// <summary>
       ///接班人编号
       /// </summary>
       [Display(Name ="接班人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ReceiverId { get; set; }

       /// <summary>
       ///接班人
       /// </summary>
       [Display(Name ="接班人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string ReceiverName { get; set; }

       /// <summary>
       ///检查总数
       /// </summary>
       [Display(Name ="检查总数")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CheckCounter { get; set; }

       /// <summary>
       ///项目总数
       /// </summary>
       [Display(Name ="项目总数")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ItemCounter { get; set; }

       
    }
}
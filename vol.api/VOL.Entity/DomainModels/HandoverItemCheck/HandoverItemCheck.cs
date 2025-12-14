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
    [Entity(TableCnName = "交接班物品检查",TableName = "HandoverItemCheck")]
    public partial class HandoverItemCheck:BaseEntity
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
       ///交接班编号
       /// </summary>
       [Display(Name ="交接班编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long HandoverId { get; set; }

       /// <summary>
       ///物品编号
       /// </summary>
       [Display(Name ="物品编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long ItemId { get; set; }

       /// <summary>
       ///物品批次编号
       /// </summary>
       [Display(Name ="物品批次编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long ItemDetailId { get; set; }

       /// <summary>
       ///物品名称
       /// </summary>
       [Display(Name ="物品名称")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ItemName { get; set; }

       /// <summary>
       ///系统数量
       /// </summary>
       [Display(Name ="系统数量")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int ItemCounter { get; set; }

       /// <summary>
       ///实际数量
       /// </summary>
       [Display(Name ="实际数量")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int ItemRealCounter { get; set; }

       /// <summary>
       ///过期日期
       /// </summary>
       [Display(Name ="过期日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ExpirationDate { get; set; }

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
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///修改人编号
       /// </summary>
       [Display(Name ="修改人编号")]
       [Column(TypeName="int")]
       public int? LastModifierId { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       public string LastModifier { get; set; }

       /// <summary>
       ///修改时间
       /// </summary>
       [Display(Name ="修改时间")]
       [Column(TypeName="datetime")]
       public DateTime? LastModificationTime { get; set; }

       
    }
}
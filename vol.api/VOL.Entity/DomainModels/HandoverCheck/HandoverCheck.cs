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
    [Entity(TableCnName = "交接班检查明细",TableName = "HandoverCheck")]
    public partial class HandoverCheck:BaseEntity
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
       ///检查编号
       /// </summary>
       [Display(Name ="检查编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int CheckId { get; set; }

       /// <summary>
       ///检查内容
       /// </summary>
       [Display(Name ="检查内容")]
       [MaxLength(1024)]
       [Column(TypeName="nvarchar(1024)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string CheckContent { get; set; }

       /// <summary>
       ///数值结果
       /// </summary>
       [Display(Name ="数值结果")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ValueResult { get; set; }

       /// <summary>
       ///文字结果
       /// </summary>
       [Display(Name ="文字结果")]
       [MaxLength(256)]
       [Column(TypeName="nvarchar(256)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string TextResult { get; set; }

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
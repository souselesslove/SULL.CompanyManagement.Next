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
    [Entity(TableCnName = "开关记录",TableName = "SealHistory")]
    public partial class SealHistory:BaseEntity
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
       ///仓库编号
       /// </summary>
       [Display(Name ="仓库编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long RelatedId { get; set; }

       /// <summary>
       ///类型
       /// </summary>
       [Display(Name ="类型")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public byte Category { get; set; }

       /// <summary>
       ///创建人编号
       /// </summary>
       [Display(Name ="创建人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreatorId { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Creator { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreationTime { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string LastModifier { get; set; }

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
       ///处理人编号
       /// </summary>
       [Display(Name ="处理人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AssignUserId { get; set; }

       /// <summary>
       ///处理人
       /// </summary>
       [Display(Name ="处理人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string AssignUser { get; set; }

       /// <summary>
       ///状态
       /// </summary>
       [Display(Name ="状态")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Status { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///编号
       /// </summary>
       [Display(Name ="编号")]
       [MaxLength(16)]
       [Column(TypeName="nvarchar(16)")]
       [Editable(true)]
       public string Code { get; set; }

       
    }
}
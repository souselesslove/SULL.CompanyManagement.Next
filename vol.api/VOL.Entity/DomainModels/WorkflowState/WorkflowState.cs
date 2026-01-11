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
    [Entity(TableCnName = "流程状态",TableName = "WorkflowState")]
    public partial class WorkflowState:BaseEntity
    {
        /// <summary>
       ///状态编号
       /// </summary>
       [Key]
       [Display(Name ="状态编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long Id { get; set; }

       /// <summary>
       ///任务编号
       /// </summary>
       [Display(Name ="任务编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long WorkTaskId { get; set; }

       /// <summary>
       ///当前状态
       /// </summary>
       [Display(Name ="当前状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Status { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(1024)]
       [Column(TypeName="nvarchar(1024)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///创建人编码
       /// </summary>
       [Display(Name ="创建人编码")]
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

       
    }
}
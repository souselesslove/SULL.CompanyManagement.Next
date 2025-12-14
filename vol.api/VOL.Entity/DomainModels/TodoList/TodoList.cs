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
    [Entity(TableCnName = "待办事项",TableName = "TodoList")]
    public partial class TodoList:BaseEntity
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
       ///关联编号
       /// </summary>
       [Display(Name ="关联编号")]
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
       ///物品名称
       /// </summary>
       [Display(Name ="物品名称")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Description { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Creator { get; set; }

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
       ///最近状态编号
       /// </summary>
       [Display(Name ="最近状态编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? LatestStatusId { get; set; }

       /// <summary>
       ///最近状态
       /// </summary>
       [Display(Name ="最近状态")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? LatestStatus { get; set; }

       /// <summary>
       ///最近处理时间
       /// </summary>
       [Display(Name ="最近处理时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LatestTime { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///处理人编号
       /// </summary>
       [Display(Name ="处理人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? LatestAssignUserId { get; set; }

       /// <summary>
       ///处理人
       /// </summary>
       [Display(Name ="处理人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string LatestAssignUser { get; set; }

       /// <summary>
       ///已结束
       /// </summary>
       [Display(Name ="已结束")]
       [Column(TypeName="bit")]
       [Editable(true)]
       public bool? IsFinished { get; set; }

       /// <summary>
       ///结束时间
       /// </summary>
       [Display(Name ="结束时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? FinishTime { get; set; }

       /// <summary>
       ///结果状态
       /// </summary>
       [Display(Name ="结果状态")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? FinalStatus { get; set; }

       /// <summary>
       ///处理结果
       /// </summary>
       [Display(Name ="处理结果")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Result { get; set; }

       /// <summary>
       ///最后期限
       /// </summary>
       [Display(Name ="最后期限")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? Deadline { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
        public DateTime? CreationTime { get; set; }

       /// <summary>
       ///创建人编号
       /// </summary>
       [Display(Name ="创建人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
        public int? CreatorId { get; set; }

       
    }
}
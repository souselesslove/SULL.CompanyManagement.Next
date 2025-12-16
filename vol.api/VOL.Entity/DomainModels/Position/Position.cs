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
    [Entity(TableCnName = "位置管理",TableName = "Position")]
    public partial class Position:BaseEntity
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
       ///仓库编号
       /// </summary>
       [Display(Name ="仓库编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? WarehouseId { get; set; }

       /// <summary>
       ///存储设备编号
       /// </summary>
       [Display(Name ="存储设备编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? ToolId { get; set; }

       /// <summary>
       ///层编号
       /// </summary>
       [Display(Name ="层编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FloorId { get; set; }

       /// <summary>
       ///序号
       /// </summary>
       [Display(Name ="序号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? SerialId { get; set; }

       /// <summary>
       ///类型
       /// </summary>
       [Display(Name ="类型")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public byte Category { get; set; }

       /// <summary>
       ///排序
       /// </summary>
       [Display(Name ="排序")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int OrderBy { get; set; }

       /// <summary>
       ///物品编号
       /// </summary>
       [Display(Name ="物品编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? ItemId { get; set; }

       /// <summary>
       ///物品名称
       /// </summary>
       [Display(Name ="物品名称")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string ItemName { get; set; }

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
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Remark { get; set; }

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
       ///修改时间
       /// </summary>
       [Display(Name ="修改时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LastModificationTime { get; set; }

       /// <summary>
       ///创建人编号
       /// </summary>
       [Display(Name ="创建人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreatorId { get; set; }

       /// <summary>
       ///责任人编号
       /// </summary>
       [Display(Name ="责任人编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? DutyUserId { get; set; }

       /// <summary>
       ///状态
       /// </summary>
       [Display(Name ="状态")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Status { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string LastModifier { get; set; }

       /// <summary>
       ///责任人
       /// </summary>
       [Display(Name ="责任人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string DutyUser { get; set; }

       /// <summary>
       ///显示名称
       /// </summary>
       [Display(Name ="显示名称")]
       [MaxLength(32)]
       [Column(TypeName="nvarchar(32)")]
       [Editable(true)]
       public string DisplayName { get; set; }

       /// <summary>
       ///状态编号
       /// </summary>
       [Display(Name ="状态编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? StatusId { get; set; }

       
    }
}
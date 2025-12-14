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
    [Entity(TableCnName = "设备信息",TableName = "Device")]
    public partial class Device:BaseEntity
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
       ///IP地址
       /// </summary>
       [Display(Name ="IP地址")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Ip { get; set; }

       /// <summary>
       ///详细信息
       /// </summary>
       [Display(Name ="详细信息")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Information { get; set; }

       /// <summary>
       ///申请时间
       /// </summary>
       [Display(Name ="申请时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime ApplyTime { get; set; }

       /// <summary>
       ///是否注册
       /// </summary>
       [Display(Name ="是否注册")]
       [Column(TypeName="bit")]
       [Editable(true)]
       public bool? IsRegistered { get; set; }

       /// <summary>
       ///审核人编码
       /// </summary>
       [Display(Name ="审核人编码")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? LastAuditUserId { get; set; }

       /// <summary>
       ///审核人
       /// </summary>
       [Display(Name ="审核人")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string LastAuditUser { get; set; }

       /// <summary>
       ///审核时间
       /// </summary>
       [Display(Name ="审核时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LastAuditTime { get; set; }

       /// <summary>
       ///显示名称
       /// </summary>
       [Display(Name ="显示名称")]
       [MaxLength(32)]
       [Column(TypeName="nvarchar(32)")]
       [Editable(true)]
       public string DisplayName { get; set; }

       /// <summary>
       ///类型
       /// </summary>
       [Display(Name ="类型")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Category { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       public string Remark { get; set; }

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
       ///注册码
       /// </summary>
       [Display(Name ="注册码")]
       [MaxLength(32)]
       [Column(TypeName="nvarchar(32)")]
       [Editable(true)]
       public string RegisterCode { get; set; }

       /// <summary>
       ///到期时间
       /// </summary>
       [Display(Name ="到期时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? EndTime { get; set; }

       /// <summary>
       ///上次注册编号
       /// </summary>
       [Display(Name ="上次注册编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? LastDeviceRegisterId { get; set; }

       
    }
}
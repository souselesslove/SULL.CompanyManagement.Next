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
    [Entity(TableCnName = "抢救记录",TableName = "Rescue")]
    public partial class Rescue:BaseEntity
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
       [MaxLength(128)]
       [Column(TypeName="nvarchar(128)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Name { get; set; }

       /// <summary>
       ///性别
       /// </summary>
       [Display(Name ="性别")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Gender { get; set; }

       /// <summary>
       ///年龄
       /// </summary>
       [Display(Name ="年龄")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Age { get; set; }

       /// <summary>
       ///科室名称
       /// </summary>
       [Display(Name ="科室名称")]
       [MaxLength(32)]
       [Column(TypeName="nvarchar(32)")]
       [Editable(true)]
       public string DepartmentName { get; set; }

       /// <summary>
       ///床号
       /// </summary>
       [Display(Name ="床号")]
       [MaxLength(8)]
       [Column(TypeName="nvarchar(8)")]
       [Editable(true)]
       public string BedNO { get; set; }

       /// <summary>
       ///住院号
       /// </summary>
       [Display(Name ="住院号")]
       [MaxLength(32)]
       [Column(TypeName="nvarchar(32)")]
       [Editable(true)]
       public string PatientId { get; set; }

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
       ///病情变化
       /// </summary>
       [Display(Name ="病情变化")]
       [MaxLength(1024)]
       [Column(TypeName="nvarchar(1024)")]
       [Editable(true)]
       public string Changes { get; set; }

       /// <summary>
       ///检查情况
       /// </summary>
       [Display(Name ="检查情况")]
       [MaxLength(512)]
       [Column(TypeName="nvarchar(512)")]
       [Editable(true)]
       public string Check { get; set; }

       /// <summary>
       ///诊断
       /// </summary>
       [Display(Name ="诊断")]
       [MaxLength(512)]
       [Column(TypeName="nvarchar(512)")]
       [Editable(true)]
       public string Diagnosis { get; set; }

       /// <summary>
       ///抢救过程
       /// </summary>
       [Display(Name ="抢救过程")]
       [MaxLength(512)]
       [Column(TypeName="nvarchar(512)")]
       [Editable(true)]
       public string Process { get; set; }

       /// <summary>
       ///抢救效果
       /// </summary>
       [Display(Name ="抢救效果")]
       [MaxLength(64)]
       [Column(TypeName="nvarchar(64)")]
       [Editable(true)]
       public string Effect { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(1024)]
       [Column(TypeName="nvarchar(1024)")]
       [Editable(true)]
       public string Remark { get; set; }

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
       ///状态
       /// </summary>
       [Display(Name ="状态")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Status { get; set; }

       /// <summary>
       ///参与人员数量
       /// </summary>
       [Display(Name ="参与人员数量")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? StaffCounter { get; set; }

       /// <summary>
       ///抢救耗时
       /// </summary>
       [Display(Name ="抢救耗时")]
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
       ///抢救车编号
       /// </summary>
       [Display(Name ="抢救车编号")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long CartId { get; set; }

       /// <summary>
       ///删除
       /// </summary>
       [Display(Name ="删除")]
       [Column(TypeName="bit")]
       [Editable(true)]
       public bool? IsDeleted { get; set; }

       /// <summary>
       ///抢救车
       /// </summary>
       [Display(Name ="抢救车")]
       [MaxLength(32)]
       [Column(TypeName="nvarchar(32)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string CartName { get; set; }

       
    }
}
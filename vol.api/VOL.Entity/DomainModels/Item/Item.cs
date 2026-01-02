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
    [Entity(TableCnName = "物品信息", TableName = "Item")]
    public partial class Item : BaseEntity
    {
        /// <summary>
        ///编号
        /// </summary>
        [Key]
        [Display(Name = "编号")]
        [Column(TypeName = "bigint")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public long Id { get; set; }

        /// <summary>
        ///物品名称
        /// </summary>
        [Display(Name = "物品名称")]
        [MaxLength(64)]
        [Column(TypeName = "nvarchar(64)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        /// <summary>
        ///完整名称
        /// </summary>
        [Display(Name = "完整名称")]
        [MaxLength(128)]
        [Column(TypeName = "nvarchar(128)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string FullName { get; set; }

        /// <summary>
        ///接口代码
        /// </summary>
        [Display(Name = "接口代码")]
        [MaxLength(32)]
        [Column(TypeName = "nvarchar(32)")]
        [Editable(true)]
        public string InterfaceCode { get; set; }

        /// <summary>
        ///类型
        /// </summary>
        [Display(Name = "类型")]
        [Column(TypeName = "tinyint")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public byte Category { get; set; }

        /// <summary>
        ///规格
        /// </summary>
        [Display(Name = "规格")]
        [MaxLength(32)]
        [Column(TypeName = "nvarchar(32)")]
        [Editable(true)]
        public string Specification { get; set; }

        /// <summary>
        ///单位
        /// </summary>
        [Display(Name = "单位")]
        [MaxLength(32)]
        [Column(TypeName = "nvarchar(32)")]
        [Editable(true)]
        public string Unit { get; set; }

        /// <summary>
        ///剂型
        /// </summary>
        [Display(Name = "剂型")]
        [MaxLength(32)]
        [Column(TypeName = "nvarchar(32)")]
        [Editable(true)]
        public string Form { get; set; }

        /// <summary>
        ///批准文号
        /// </summary>
        [Display(Name = "批准文号")]
        [MaxLength(32)]
        [Column(TypeName = "nvarchar(32)")]
        [Editable(true)]
        public string RegisterCode { get; set; }

        /// <summary>
        ///生产厂家
        /// </summary>
        [Display(Name = "生产厂家")]
        [MaxLength(128)]
        [Column(TypeName = "nvarchar(128)")]
        [Editable(true)]
        public string Factory { get; set; }

        /// <summary>
        ///基数
        /// </summary>
        [Display(Name = "基数")]
        [Column(TypeName = "int")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public int Cardinality { get; set; }

        /// <summary>
        ///条码号
        /// </summary>
        [Display(Name = "条码号")]
        [MaxLength(128)]
        [Column(TypeName = "nvarchar(128)")]
        [Editable(true)]
        public string Barcode { get; set; }

        /// <summary>
        ///排序
        /// </summary>
        [Display(Name = "排序")]
        [Column(TypeName = "int")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public int OrderBy { get; set; }

        /// <summary>
        ///库存
        /// </summary>
        [Display(Name = "库存")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? AvailableStock { get; set; }

        /// <summary>
        ///检索码
        /// </summary>
        [Display(Name = "检索码")]
        [MaxLength(16)]
        [Column(TypeName = "nvarchar(16)")]
        [Editable(true)]
        public string SearchCode { get; set; }

        /// <summary>
        ///批次数量
        /// </summary>
        [Display(Name = "批次数量")]
        [Column(TypeName = "int")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public int KindCounter { get; set; }

        /// <summary>
        ///创建人
        /// </summary>
        [Display(Name = "创建人")]
        [MaxLength(128)]
        [Column(TypeName = "nvarchar(128)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string Creator { get; set; }

        /// <summary>
        ///删除人编号
        /// </summary>
        [Display(Name = "删除人编号")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? DeleterId { get; set; }

        /// <summary>
        ///删除人
        /// </summary>
        [Display(Name = "删除人")]
        [MaxLength(128)]
        [Column(TypeName = "nvarchar(128)")]
        [Editable(true)]
        public string Deleter { get; set; }

        /// <summary>
        ///删除时间
        /// </summary>
        [Display(Name = "删除时间")]
        [Column(TypeName = "datetime")]
        [Editable(true)]
        public DateTime? DeletionTime { get; set; }

        /// <summary>
        ///删除
        /// </summary>
        [Display(Name = "删除")]
        [Column(TypeName = "bit")]
        [Editable(true)]
        public bool? IsDeleted { get; set; }

        /// <summary>
        ///状态
        /// </summary>
        [Display(Name = "状态")]
        [Column(TypeName = "tinyint")]
        [Editable(true)]
        public byte? Status { get; set; }

        /// <summary>
        ///备注
        /// </summary>
        [Display(Name = "备注")]
        [MaxLength(1024)]
        [Column(TypeName = "nvarchar(1024)")]
        [Editable(true)]
        public string Remark { get; set; }

        /// <summary>
        ///最近过期日期
        /// </summary>
        [Display(Name = "最近过期日期")]
        [Column(TypeName = "datetime")]
        [Editable(true)]
        public DateTime? LatestExpirationDate { get; set; }

        /// <summary>
        ///机构编号
        /// </summary>
        [Display(Name = "机构编号")]
        [Column(TypeName = "bigint")]
        [Editable(true)]
        public long? OrganizationId { get; set; }

        /// <summary>
        ///状态编号
        /// </summary>
        [Display(Name = "状态编号")]
        [Column(TypeName = "bigint")]
        [Editable(true)]
        public long? StatusId { get; set; }

        /// <summary>
        ///部门编号
        /// </summary>
        [Display(Name = "部门编号")]
        [Column(TypeName = "bigint")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public long DepartmentId { get; set; }

        /// <summary>
        ///价格
        /// </summary>
        [Display(Name = "价格")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? Price { get; set; }

        /// <summary>
        ///标记
        /// </summary>
        [Display(Name = "标记")]
        [MaxLength(128)]
        [Column(TypeName = "nvarchar(128)")]
        [Editable(true)]
        public string Tag { get; set; }

        /// <summary>
        ///状态备注
        /// </summary>
        [Display(Name = "状态备注")]
        [MaxLength(256)]
        [Column(TypeName = "nvarchar(256)")]
        [Editable(true)]
        public string StatusRemark { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        [Display(Name = "创建时间")]
        [Column(TypeName = "datetime")]
        [Required(AllowEmptyStrings = false)]
        public DateTime CreationTime { get; set; }

        /// <summary>
        ///修改人编号
        /// </summary>
        [Display(Name = "修改人编号")]
        [Column(TypeName = "int")]
        public int? LastModifierId { get; set; }

        /// <summary>
        ///修改人
        /// </summary>
        [Display(Name = "修改人")]
        [MaxLength(128)]
        [Column(TypeName = "nvarchar(128)")]
        public string LastModifier { get; set; }

        /// <summary>
        ///修改时间
        /// </summary>
        [Display(Name = "修改时间")]
        [Column(TypeName = "datetime")]
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        ///创建人编号
        /// </summary>
        [Display(Name = "创建人编号")]
        [Column(TypeName = "int")]
        [Required(AllowEmptyStrings = false)]
        public int CreatorId { get; set; }


    }
}
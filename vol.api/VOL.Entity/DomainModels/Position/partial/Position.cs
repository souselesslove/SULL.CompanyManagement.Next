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
    
    public partial class Position
    {
        //此处配置字段(字段配置见此model的另一个partial),如果表中没有此字段请加上 [NotMapped]属性，否则会异常
        // Category: 10=抢救车；20=仓库（当WarehouseId为空值时有意义，表示仓储类型）；100=层；110=序号
        // WarehouseId: 当Category=10时为空；当Category=100、110时；表示关联的WarehouseId
        // FloorId:  当Category=10时表示抢救车的总层数；当Category=100时；表示所在分层；当Category=110时；表示所在分层的编号
        // SerialId: 当Category=100时，代表当前分层的分隔数量；当Category=110时，代表当前分隔的序号
    }
}
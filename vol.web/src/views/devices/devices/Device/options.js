// *Author：jxx
// *Contact：283591387@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '设备信息',
        name: 'Device',
        url: "/Device/",
        sortName: "Id"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {};
    const editFormOptions = [];
    const searchFormFields = {};
    const searchFormOptions = [];
    const columns = [{field:'Id',title:'编号',type:'long',sort:true,width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'Ip',title:'IP地址',type:'string',link:true,width:100,require:true,align:'left'},
                       {field:'Information',title:'详细信息',type:'string',width:150,require:true,align:'left'},
                       {field:'ApplyTime',title:'申请时间',type:'datetime',sort:true,width:150,require:true,align:'left'},
                       {field:'IsRegistered',title:'是否注册',type:'bool',sort:true,width:100,align:'left'},
                       {field:'LastAuditUserId',title:'审核人编码',type:'int',width:110,hidden:true,align:'left'},
                       {field:'LastAuditUser',title:'审核人',type:'string',width:130,align:'left'},
                       {field:'LastAuditTime',title:'审核时间',type:'datetime',width:150,align:'left'},
                       {field:'DisplayName',title:'显示名称',type:'string',width:100,align:'left'},
                       {field:'Category',title:'类型',type:'byte',width:100,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:150,align:'left'},
                       {field:'DeleterId',title:'删除人编号',type:'int',width:110,hidden:true,align:'left'},
                       {field:'Deleter',title:'删除人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'DeletionTime',title:'删除时间',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'IsDeleted',title:'删除',type:'bool',width:110,hidden:true,align:'left'},
                       {field:'RegisterCode',title:'注册码',type:'string',width:100,align:'left'},
                       {field:'EndTime',title:'到期时间',type:'datetime',width:150,align:'left'},
                       {field:'LastDeviceRegisterId',title:'上次注册编号',type:'long',width:110,hidden:true,align:'left'}];
    const detail ={columns:[]};
    const details = [];

    return {
        table,
        key,
        tableName,
        tableCNName,
        newTabEdit,
        editFormFields,
        editFormOptions,
        searchFormFields,
        searchFormOptions,
        columns,
        detail,
        details
    };
}
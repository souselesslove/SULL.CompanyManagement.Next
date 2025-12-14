// *Author：jxx
// *Contact：283591387@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '流程记录',
        name: 'WorkFlow',
        url: "/WorkFlow/",
        sortName: "Id"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"Topic":"","AuditUser":"","Remark":""};
    const editFormOptions = [[{"dataKey":"流程主题","data":[],"title":"主题","required":true,"field":"Topic","type":"select"},
                               {"dataKey":"用户列表","data":[],"title":"审核人","field":"AuditUser","type":"select"}],
                              [{"title":"备注","field":"Remark","colSize":12,"type":"textarea"}]];
    const searchFormFields = {"Topic":""};
    const searchFormOptions = [[{"dataKey":"流程主题","data":[],"title":"主题","field":"Topic","type":"="}]];
    const columns = [{field:'Id',title:'编号',type:'guid',sort:true,width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'Topic',title:'主题',type:'string',bind:{ key:'流程主题',data:[]},width:120,require:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,require:true,align:'left'},
                       {field:'AuditStatus',title:'审核状态',type:'int',sort:true,width:100,align:'left'},
                       {field:'AuditUser',title:'审核人',type:'string',bind:{ key:'用户列表',data:[]},width:100,align:'left'},
                       {field:'AuditTime',title:'审核时间',type:'datetime',width:170,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:220,align:'left'},
                       {field:'DeleterId',title:'删除人编号',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Deleter',title:'删除人',type:'string',width:220,hidden:true,align:'left'},
                       {field:'DeletionTime',title:'删除时间',type:'datetime',width:180,hidden:true,align:'left'},
                       {field:'IsDeleted',title:'删除',type:'bool',width:110,hidden:true,align:'left'},
                       {field:'AuditUserId',title:'审核人编号',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateID',title:'创建人编号',type:'int',width:80,hidden:true,require:true,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:170,require:true,align:'left'},
                       {field:'ModifyID',title:'修改人编号',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:170,align:'left'}];
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
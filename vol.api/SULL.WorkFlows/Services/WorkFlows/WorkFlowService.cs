/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下WorkFlowService与IWorkFlowService中编写
 */
using SULL.WorkFlows.IRepositories;
using SULL.WorkFlows.IServices;
using VOL.Core.BaseProvider;
using VOL.Core.Extensions.AutofacManager;
using VOL.Entity.DomainModels;

namespace SULL.WorkFlows.Services
{
    public partial class WorkFlowService : ServiceBase<WorkFlow, IWorkFlowRepository>
    , IWorkFlowService, IDependency
    {
    public static IWorkFlowService Instance
    {
      get { return AutofacContainerModule.GetService<IWorkFlowService>(); } }
    }
 }

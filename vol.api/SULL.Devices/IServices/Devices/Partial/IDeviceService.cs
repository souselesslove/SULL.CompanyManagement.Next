/*
*所有关于Device类的业务代码接口应在此处编写
*/
using SULL.Devices.Services.Devices.Dtos;
using System.Linq.Expressions;
using VOL.Core.BaseProvider;
using VOL.Core.Utilities;
using VOL.Entity.DomainModels;
namespace SULL.Devices.IServices
{
    public partial interface IDeviceService
    {
        Task<WebResponseContent> GetRegisterAsync(DeviceRegisterInputDto input,CancellationToken cancellationToken = default);
    }
 }

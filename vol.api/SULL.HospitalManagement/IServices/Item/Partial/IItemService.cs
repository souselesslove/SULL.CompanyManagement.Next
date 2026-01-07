/*
*所有关于Item类的业务代码接口应在此处编写
*/
using VOL.Core.BaseProvider;
using VOL.Entity.DomainModels;
using VOL.Core.Utilities;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace SULL.HospitalManagement.IServices
{
    public partial interface IItemService
    {
        Task<WebResponseContent> CheckItemWarningsAsync(long id);
        Task<string> GetItemRemarkAsync(string combinedId);
    }
 }

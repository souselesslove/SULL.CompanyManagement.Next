/*
*所有关于ShiftType类的业务代码接口应在此处编写
*/
using VOL.Core.BaseProvider;
using VOL.Entity.DomainModels;
using VOL.Core.Utilities;
using System.Linq.Expressions;
namespace SULL.HospitalManagement.IServices
{
    public partial interface IShiftTypeService
    {
        WebResponseContent GenerateDefaultShifts(SaveModel saveModel);
    }
 }

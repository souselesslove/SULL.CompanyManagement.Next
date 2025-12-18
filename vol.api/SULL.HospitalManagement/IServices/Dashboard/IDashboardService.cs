using VOL.Core.BaseProvider;
using VOL.Core.Utilities;
using VOL.Entity.DomainModels;

namespace SULL.HospitalManagement.IServices
{
    public interface IDashboardService
    {
        object GetDashboardData(PageDataOptions options);
        void AddLog(long orgId, string type, string msg);
    }
}

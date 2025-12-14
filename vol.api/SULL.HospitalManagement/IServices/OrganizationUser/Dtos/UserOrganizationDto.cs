using VOL.Entity.DomainModels;

namespace SULL.HospitalManagement.IServices.Dtos;

public class UserOrganizationDto
{
    public OrganizationUser User { get; set; }
    public OrganizationDepartment Department { get; set; }
    public Organization Organization { get; set; }

}
namespace SULL.HospitalManagement.IServices.Dtos;

public class DepartmentDutyInputDto
{
    /// <summary>
    /// 部门编号
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 责任人编号
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 责任人
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}
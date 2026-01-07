namespace SULL.HospitalManagement.UniPush;

public class UniPushResult
{
    public bool Success { get; set; }
    public string Msg { get; set; } = string.Empty;
    public object Data { get; set; } = new object();
}
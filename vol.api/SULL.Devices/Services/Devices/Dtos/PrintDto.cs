namespace SULL.Devices.Services.Devices.Dtos;

public class PrintDto
{
    public string id { get; set; }
    public string category { get; set; }
    public int copies { get; set; }
    public int port { get; set; }
    public List<Param> _params { get; set; } = new List<Param>();
}

public class Param
{
    public string name { get; set; }
    public string value { get; set; }
}
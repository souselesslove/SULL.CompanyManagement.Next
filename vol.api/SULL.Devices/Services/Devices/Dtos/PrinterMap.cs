namespace SULL.Devices.Services.Devices.Dtos;

public class PrinterMap
{
    public Printer[] Printers { get; set; }

}

public class Printer
{
    public string DeviceCode { get; set; }
    public string ComputerIp { get; set; }
    public string Remark { get; set; }
}
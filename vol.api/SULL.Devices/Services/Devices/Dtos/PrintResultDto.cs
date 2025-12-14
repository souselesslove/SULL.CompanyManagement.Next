namespace SULL.Devices.Services.Devices.Dtos;

public class PrintResultDto
{
    public int code { get; set; }
    public string message { get; set; }

    public PrintResultDto()
    {
    }
    public PrintResultDto(int code, string message)
    {
        this.code = code;
        this.message = message;
    }

}
namespace SULL.Devices.Services.Devices.Dtos;

public class DeviceRegisterInputDto
{
    public string SDKVersion { get; set; }
    public string appId { get; set; }
    public string appLanguage { get; set; }
    public string appName { get; set; }
    public string appVersion { get; set; }
    public int? appVersionCode { get; set; }
    public string appWgtVersion { get; set; }
    public string brand { get; set; }
    public string browserName { get; set; }
    public string browserVersion { get; set; }
    public string deviceBrand { get; set; }
    public string deviceId { get; set; }
    public string deviceModel { get; set; }
    public string deviceOrientation { get; set; }
    public float? devicePixelRatio { get; set; }
    public string deviceType { get; set; }
    public string language { get; set; }
    public string model { get; set; }
    public string oaid { get; set; }
    public int? osAndroidAPILevel { get; set; }
    public string osLanguage { get; set; }
    public string osName { get; set; }
    public string osTheme { get; set; }
    public string osVersion { get; set; }
    public float? pixelRatio { get; set; }
    public string platform { get; set; }
    public string romName { get; set; }
    public string romVersion { get; set; }
    public Safearea safeArea { get; set; }
    public Safeareainsets safeAreaInsets { get; set; }
    public int? screenHeight { get; set; }
    public int? screenWidth { get; set; }
    public int? statusBarHeight { get; set; }
    public string system { get; set; }
    public string ua { get; set; }
    public string uniCompileVersion { get; set; }
    public string uniPlatform { get; set; }
    public string uniRuntimeVersion { get; set; }
    public string version { get; set; }
    public int? windowBottom { get; set; }
    public int? windowHeight { get; set; }
    public int? windowTop { get; set; }
    public int? windowWidth { get; set; }
}

public class Safearea
{
    public int? left { get; set; }
    public int? right { get; set; }
    public int? top { get; set; }
    public int? bottom { get; set; }
    public int? width { get; set; }
    public int? height { get; set; }
}

public class Safeareainsets
{
    public int? top { get; set; }
    public int? right { get; set; }
    public int? bottom { get; set; }
    public int? left { get; set; }
}

public class RegisterDto
{
    public string Code { get; set; }

    /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
    public RegisterDto()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
    public RegisterDto(string code)
    {
        Code = code;
    }
}

public class DeviceRegisterInfo
{
    public string orgId { get; set; }
    public string orgName { get; set; }
    public string sellerId { get; set; }
    public string sellerName { get; set; }
    public string category { get; set; }
    public string serialNumber { get; set; }
    public DateTime startTime { get; set; }
    public DateTime? endTime { get; set; }
    public DateTime? sellTime { get; set; }
    public DateTime? limitUseTime { get; set; }
}
namespace Api.Models.Api
{
    public class DeviceAuthenticationRequest
    {
        public string DeviceImei { get; set; }
        public string SimIccId { get; set; }
        public string SimImsi { get; set; }
        public string SimMsisdn { get; set; }
    }
}

namespace BcdrTestAppADX.Models
{
    public class ServicePrincipalSetting : IServicePrincipalSetting
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
    }

    public interface IServicePrincipalSetting
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string TenantId { get; set; }
    }
}
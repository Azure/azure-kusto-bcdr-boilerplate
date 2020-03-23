using System;

namespace BcdrTestAppADX.Models
{
    public class AuthenticationSetting : IAuthenticationSetting
    {
        public string ManagedIdentity { get; set; }
        public ServicePrincipalSetting ServicePrincipal { get; set; }
    }

    public interface IAuthenticationSetting
    {
        String ManagedIdentity { get; set; }

        ServicePrincipalSetting ServicePrincipal { get; set; }
    }
}

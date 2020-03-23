using System;
using System.Collections.Generic;

namespace BcdrTestAppADX.Models
{
    public class BcdrSetting : IBcdrSetting
    {
        public string PrimaryAdx { get; set; }
        public string SecondaryAdx { get; set; }
        public AuthenticationSetting Authentication { get; set; }
    }

    public interface IBcdrSetting
    {
        String PrimaryAdx { get; set; }
        String SecondaryAdx { get; set; }
        AuthenticationSetting Authentication { get; set; }
    }
}

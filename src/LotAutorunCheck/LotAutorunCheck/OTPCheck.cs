using System;
using System.Collections.Generic;
using System.Text;

namespace LotAutorunCheck
{
    public class OTPCheck
    {
        public String LotID { get; set; }

        public String OTP { get; set; }

        public bool[] Autorun = new bool[4];

        public bool AutorunResult { get; set; }

    }
}

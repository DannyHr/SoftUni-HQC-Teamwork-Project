namespace RestSharp.Tests.SampleClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Payment
    {
        public PaymentType Type { get; set; }

        public int Amount { get; set; }
    }
}

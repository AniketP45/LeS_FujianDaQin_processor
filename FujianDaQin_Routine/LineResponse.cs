using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FujianDaQin_Routine
{
    public class LineResponse
    {
        public Extend Extend { get; set; }
        public string Id { get; set; }
        public string Msg { get; set; }
        public bool Success { get; set; }
    }

    public class Extend
    {
        public double Fee { get; set; }
        public double CheckFee { get; set; }
        public double FeeCny { get; set; }
        public double FeeWithoutTax { get; set; }
    }
}

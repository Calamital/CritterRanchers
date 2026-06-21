using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterRanchers
{
    class Data
    {
        public double Money { get; set; }
		public int MaxCritters { get; set; }
		public double[]? CritterCosts { get; set; }
        public double[]? CritterMoney { get; set; }
        public List<int>? CritterList { get; set; }
        public Dictionary<string, double>? UpgradeCosts { get; set; }
        public double GlobalCritterProfit { get; set; }
        public double CritterProfitMultiplier { get; set; }
        public Dictionary<string, double[]>? UpgradeAmounts { get; set; }
        public int FenceSize { get; set; }
	}
}

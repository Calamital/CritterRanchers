using System;
using System.Collections.Generic;
using System.Numerics;

namespace CritterRanchers
{
	internal static class Stats
	{
		public static double Money = 1d;
		public static int MaxCritters = 0;
		public static int CritterCount = 0;
		public static double GlobalCritterProfit = 1d;
		public static double CritterProfitMultiplier = 1d;
		public static double[] CritterCosts =
		[
			1d,
			10d,
			150d,
			900d,
			2500d,
			10000d,
			40000d
		];
		public static double[] CritterMoney =
		[
			0.25d,
			1d,
			10d,
			45d,
			120d,
			800d,
			1800d
		];
		public static Dictionary<string, double> UpgradeCosts = new()
		{
			["FenceSize"] = 5000d,
			["CritterProfit"] = 5d,
			["CritterProfitMultiplier"] = 1000d
		};
		public static Dictionary<string, double[]> UpgradeAmounts = new()
		{
			["FenceSize"] = [0d, 8d],
			["CritterProfit"] = [0d, 500d],
			["CritterProfitMultiplier"] = [0d, 500d]
		};

		public static string Abbreviate(double n)
		{
			if (n < 1000)
			{
				return Math.Round(n, 2).ToString();
			}

			string abbreviations = "K,M,B,T,Qd,Qn,Sx,Sp,Oc,No,Dc,Ud,Dd,Td,QdD,QnD,SxD,SpD,OcD,NvD,Vt";
			int exponent = (int)Math.Floor(Math.Log10(n) / 3) - 1;
			return Math.Round(n / Math.Pow(1000, exponent + 1), 2) + abbreviations.Split(",")[exponent];
		}

		public static float EaseDistance(float t)
		{
			return (-2 * Math.Abs(t - 0.5f)) + 1f;
		}
	}
}

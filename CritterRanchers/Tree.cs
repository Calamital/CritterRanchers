using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Provider;

namespace CritterRanchers
{
	public abstract class Tree
	{
		public List<TreeUpgrade> Upgrades = [];
	}

	public class MechanicalAscensionTree : Tree
	{
		public MechanicalAscensionTree()
		{
			Upgrades.Add(new AscensionUpgrade()
			{
				TreeDirection = "start",
				UpgradeIcon = "ms-appx:///Assets/UpgradeTreeButtons/Blank.png",
				Title = "Generator 1",
				Description = "Builds the first generator; produces an extra 0.01x multiplier for every second of your current ascension.",
				Cost = 1
			});
			Upgrades.Add(new AscensionUpgrade()
			{
				TreeDirection = "up",
				UpgradeIcon = "ms-appx:///Assets/UpgradeTreeButtons/Blank.png",
				Title = "Generator 2",
				Description = "Builds the second generator; produces another Generator 1 every 10 seconds of your current ascensions.",
				Cost = 100
			});
			Upgrades[0].Position = new System.Numerics.Vector2(800, 500);
			Upgrades[1].PastUpgrade = Upgrades[0];
			foreach (TreeUpgrade upgrade in Upgrades)
			{
				upgrade.AddToCanvas();
			}
		}
	}
}

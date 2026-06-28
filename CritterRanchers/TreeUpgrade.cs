using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.WebUI;

namespace CritterRanchers
{
	public abstract class TreeUpgrade
	{
		public Vector2 Position;
		public TreeUpgrade? PastUpgrade;
		public required string TreeDirection { get; set; }
		public required string UpgradeIcon { get; set; }
		public required string Title { get; set; }
		public required string Description { get; set; }
		public required double Cost { get; set; }

		public abstract void PurchaseEffect();
		public void AddToCanvas()
		{
			if (PastUpgrade != null)
			{
				Position = PastUpgrade.Position;
				switch (TreeDirection)
				{
					case "up":
						Position -= new Vector2(0, 60);
						break;
					case "left":
						Position -= new Vector2(-60, 0);
						break;
					case "down":
						Position -= new Vector2(0, -60);
						break;
					case "right":
						Position -= new Vector2(60, 0);
						break;
				}
			}
			Button UpgradeButton = new()
			{
				Width = 50,
				Height = 50,
				Background = new SolidColorBrush(Color.FromArgb(255, 59, 43, 59))
			};
			BitmapImage bitmapImage = new() { UriSource = new Uri(UpgradeIcon) };
			Image Icon = new() { Source = bitmapImage };
			UpgradeButton.Content = Icon;
			App.Window?.AscensionBackground.Children.Add(UpgradeButton);
			Canvas.SetLeft(UpgradeButton, Position.X - 25);
			Canvas.SetTop(UpgradeButton, Position.Y - 25);
			Canvas.SetZIndex(UpgradeButton, 100);
			System.Diagnostics.Debug.WriteLine(Position);
		}
	}

	public class AscensionUpgrade : TreeUpgrade
	{
		public override void PurchaseEffect()
		{
			if (Stats.Stars >= Cost)
			{
				Stats.Stars -= Cost;
				Stats.AscensionUpgradeBuffs[this] = true;
			}
		}
	}
}

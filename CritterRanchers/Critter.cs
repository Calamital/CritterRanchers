using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CritterRanchers
{
	internal class Critter
	{
		public float Speed;
		public Vector2 Direction;
		public Vector2 Position;
		public float DeltaTime, LastBounceTime;
		public Image CritterIcon;
		public int ID;

		public Critter(string critterName, float speed, Vector2 direction, Vector2 position)
		{
			Speed = speed;
			Direction = direction;
			Position = position;
			DeltaTime = 0;
			ID = Critters.GetCritterID(critterName);

			BitmapImage critter = new()
			{
				UriSource = new Uri("ms-appx:///Assets/Critters/" + critterName + ".png")
			};

			CritterIcon = new Image()
			{
				Source = critter,
				Width = 60,
				Height = 60,
			};
			Canvas.SetLeft(CritterIcon, Position.X - 30);
			Canvas.SetTop(CritterIcon, Position.Y - 30);
			Canvas.SetZIndex(CritterIcon, 5);

			App.Window?.BackgroundCanvas.Children.Add(CritterIcon);
		}

		public Vector2 PositionOffset()
		{
			if (ID == 5)
			{
				return new Vector2(0, -45f * (float)Math.Abs(Math.Sin(DeltaTime * Math.PI * 6d)));
			}
			return new Vector2(0, 0);
		}

		public TextBlock CreateProfitText()
		{
			TextBlock textBlock = new()
			{
				Width = 200,
				Height = 20,
				FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Arial"),
				FontSize = 16,
				TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
				Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Critters.GetCritterColor(ID)),
				Text = "+$" + Stats.Abbreviate(Stats.CritterMoney[ID] * Stats.GlobalCritterProfit * Stats.CritterProfitMultiplier)
			};
			Canvas.SetLeft(textBlock, Position.X - 100 + PositionOffset().X);
			Canvas.SetTop(textBlock, Position.Y - 10 + PositionOffset().Y);
			Canvas.SetZIndex(textBlock, 50);

			App.Window?.BackgroundCanvas.Children.Add(textBlock);
			return textBlock;
		}

		public void TimeStep()
		{
			float xDistance = (Position.X + (30 * Direction.X) - Fence.XBound.X) / (40 * Fence.FenceSize);
			float yDistance = (Position.Y + (30 * Direction.Y) - Fence.YBound.Y) / (40 * Fence.FenceSize);
			float distanceToFence = Stats.EaseDistance(xDistance) * Stats.EaseDistance(yDistance);

			int rng = new Random().Next(101);

			DeltaTime += 1f / 60f;
			LastBounceTime += 1f / 60f;

			if (LastBounceTime >= 0.1f)
			{
				if ((Position.X <= Fence.XBound.X) || (Position.X >= Fence.XBound.Y))
				{
					Direction = new Vector2(-Direction.X, Direction.Y);
					LastBounceTime = 0f;
				}
				if ((Position.Y <= Fence.YBound.X) || (Position.Y >= Fence.YBound.Y))
				{
					Direction = new Vector2(Direction.X, -Direction.Y);
					LastBounceTime = 0f;
				}
			}

			Position -= (ID == 7 ? MathF.Pow(MathF.Cos(LastBounceTime * MathF.PI), 2) : 1) * (float)Math.Max(0.2, distanceToFence) * (float)Math.Min(1 + (LastBounceTime / 5), 1.2) * Speed * Direction / 2f;

			if (LastBounceTime >= 0.2f)
			{
				Direction += 1.25f * (1 - (distanceToFence * distanceToFence)) * new Vector2(Direction.Y, -Direction.X) * (0.5f + Math.Min(1, LastBounceTime)) * Speed / (MathF.Max(1, (Fence.FenceSize - 8) / 4) * (rng + 2000f));
				Direction /= Direction.Length();
			}

			try
			{
				Canvas.SetLeft(CritterIcon, Position.X - 30 + PositionOffset().X);
				Canvas.SetTop(CritterIcon, Position.Y - 30 + PositionOffset().Y);
			}
			catch { }

			if (DeltaTime >= 1 - Stats.CritterCooldownReduction)
			{
				DeltaTime = 0;
				Stats.Money += Stats.CritterMoney[ID] * Stats.GlobalCritterProfit * Stats.CritterProfitMultiplier;
				Critters.ProfitTexts.Add(CreateProfitText(), 0f);
			}
		}
	}
}

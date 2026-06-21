using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CritterRanchers
{
	internal static class Fence
	{
		public static int FenceSize = 8;
#pragma warning disable IDE0044
		private static List<Image> fenceTiles = [];
#pragma warning restore IDE0044
		public static Vector2 XBound = new(0, 0);
		public static Vector2 YBound = new(0, 0);

		public static void BuildFence()
		{
			int startX = 800 - (20 * FenceSize);
			int startY = 500 - (20 * FenceSize);

			XBound = new Vector2(startX + 60, startX - 40 + (FenceSize * 40));
			YBound = new Vector2(startY + 60, startY - 40 + (FenceSize * 40));

			Stats.MaxCritters = (int)Math.Floor(Math.Pow(FenceSize, 1.8d) / 3d);

			var mainWindow = App.Window;
			if (mainWindow == null) return;

			mainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
			{
				var canvas = mainWindow.BackgroundCanvas;
				if (canvas == null) return;

				foreach (Image fence in fenceTiles.ToList<Image>())
				{
					canvas.Children.Remove(fence);
					fenceTiles.Remove(fence);
				}

				for (int x = 0; x < FenceSize; x++)
				{
					for (int y = 0; y < FenceSize; y++)
					{
						string fenceTypeIndex = "";

						if (x == 0) fenceTypeIndex = "V1";
						if (x == FenceSize - 1) fenceTypeIndex = "V2";
						if (y == 0) fenceTypeIndex = "H1";
						if (y == FenceSize - 1) fenceTypeIndex = "H2";

						if ((x == 0) && (y == 0)) fenceTypeIndex = "Corner1";
						if ((x == FenceSize - 1) && (y == 0)) fenceTypeIndex = "Corner2";
						if ((x == 0) && (y == FenceSize - 1)) fenceTypeIndex = "Corner3";
						if ((x == FenceSize - 1) && (y == FenceSize - 1)) fenceTypeIndex = "Corner4";

						if (fenceTypeIndex != null)
						{
							BitmapImage fence = new()
							{
								UriSource = new System.Uri("ms-appx:///Assets/Fence" + fenceTypeIndex + ".png")
							};

							Image fencePiece = new()
							{
								Width = 40,
								Height = 40,
								Source = fence
							};

							Canvas.SetLeft(fencePiece, startX + (x * 40));
							Canvas.SetTop(fencePiece, startY + (y * 40));
							Canvas.SetZIndex(fencePiece, 1);
							canvas.Children.Add(fencePiece);
							fenceTiles.Add(fencePiece);
						}
					}
				}
			});
		}
	}
}

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Activation;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using Windows.UI;
using Windows.System.UserProfile;

namespace CritterRanchers
{
	public sealed partial class MainWindow : Window
	{
		public Canvas BackgroundCanvas => CanvasBackground;
		public string SelectedCritter = "";
		public CancellationTokenSource cancelToken = new();
		public bool GameLoaded = false;
		public MainWindow()
		{
			InitializeComponent();

			IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
			Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
			AppWindow window = AppWindow.GetFromWindowId(windowId);

			window.Resize(new Windows.Graphics.SizeInt32(1600, 1000));

			//if (AppWindow.Presenter is OverlappedPresenter presenter)
			//{
			//	presenter.IsResizable = false;
			//	presenter.IsMaximizable = false;
			//}

			CompositionTarget.Rendering += OnCompositionTargetRendering;

			Task.Run(async () =>
			{
				await Task.Delay(200);
				await InitializeGame();
			});
		}

		private async Task InitializeGame()
		{
			Data saveData = await Save.LoadUserData();

			DispatcherQueue.TryEnqueue(async () =>
			{
				if (!GameLoaded)
				{
					GameLoaded = true;

					Stats.Money = saveData.Money;
					Stats.MaxCritters = saveData.MaxCritters;
					Stats.CritterCosts = saveData.CritterCosts ?? Stats.CritterCosts;
					Stats.CritterMoney = saveData.CritterMoney ?? Stats.CritterMoney;
					Stats.UpgradeCosts = saveData.UpgradeCosts ?? Stats.UpgradeCosts;
					Stats.UpgradeAmounts = saveData.UpgradeAmounts ?? Stats.UpgradeAmounts;
					Fence.FenceSize = saveData.FenceSize;

					foreach (int critterID in saveData.CritterList ?? [])
					{
						Critters.SpawnCritter(Critters.GetCritterName(critterID));
					}

					BuildTerrain();
					Fence.BuildFence();
					SetPositions();

					await Task.Run(() => UpdateCritters(DispatcherQueue, cancelToken.Token));
				}
			});
		}

		private void SetPositions()
		{
			DispatcherQueue.TryEnqueue(() =>
			{
				Canvas.SetLeft(FenceSizeUpgradeBorder, Fence.XBound.X - 105);
				Canvas.SetTop(FenceSizeUpgradeBorder, Fence.YBound.Y);
				Canvas.SetLeft(MaxCritterUpgradeBorder, Fence.XBound.X - 105);
				Canvas.SetTop(MaxCritterUpgradeBorder, Fence.YBound.Y - 40);
				Canvas.SetLeft(CritterProfitUpgradeBorder, Fence.XBound.X - 60);
				Canvas.SetTop(CritterProfitUpgradeBorder, Fence.YBound.Y + 45);
				Canvas.SetLeft(CritterProfitMultiplierUpgradeBorder, Fence.XBound.X - 20);
				Canvas.SetTop(CritterProfitMultiplierUpgradeBorder, Fence.YBound.Y + 45);
				Canvas.SetLeft(CritterCooldownUpgradeBorder, Fence.XBound.Y);
				Canvas.SetTop(CritterCooldownUpgradeBorder, Fence.YBound.Y + 45);
			});
		}

		private static void BuildTerrain()
		{
			BitmapImage terrainImage = new()
			{
				UriSource = new Uri("ms-appx:///Assets/Grass0.png")
			};

			var mainWindow = App.Window;
			if (mainWindow == null) return;

			mainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, async () =>
			{
				var canvas = mainWindow.BackgroundCanvas;
				if (canvas == null) return;

				for (int x = 0; x < 16; x++)
				{
					for (int y = 0; y < 10; y++)
					{
						int random = new Random().Next(4);

						Image terrainTile = new()
						{
							Width = 101,
							Height = 101,
							Source = terrainImage,
							RenderTransform = new RotateTransform
							{
								Angle = random * 90,
								CenterX = 50.5,
								CenterY = 50.5
							}
						};

						Canvas.SetLeft(terrainTile, x * 100);
						Canvas.SetTop(terrainTile, y * 100);
						Canvas.SetZIndex(terrainTile, 0);
						canvas.Children.Add(terrainTile);
					}
				}
			});
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e)
		{
			cancelToken.Cancel();
		}

		private void OnCompositionTargetRendering(object? sender, object e)
		{
			MoneyText.Text = "$" + Stats.Abbreviate(Stats.Money);
			CritterLimitText.Text = "Critters: " + Stats.CritterCount + "/" + (Stats.MaxCritters + Stats.MaxCrittersIncrease);
		}

		private static async Task UpdateCritters(Microsoft.UI.Dispatching.DispatcherQueue dispatcher, CancellationToken cancel)
		{
			Stats.GlobalCritterProfit = 1 + (Stats.UpgradeAmounts["CritterProfit"][0] * 0.1d);
			Stats.CritterProfitMultiplier = 1 + (Stats.UpgradeAmounts["CritterProfitMultiplier"][0] * 0.25d);
			Stats.CritterCooldownReduction = Stats.UpgradeAmounts["CritterCooldown"][0] * 0.02d;
			Stats.MaxCrittersIncrease = (int)Stats.UpgradeAmounts["MaxCritter"][0];
			try
			{
				while (!cancel.IsCancellationRequested)
				{
					try
					{
						foreach (Critter critter in Critters.CritterList.ToList<Critter>())
						{
							dispatcher.TryEnqueue(() =>
							{
								critter.TimeStep();
							});
						}
					}
					catch (InvalidOperationException)
					{
						System.Diagnostics.Debug.WriteLine("Critter already deleted!");
					}
					try
					{
						foreach (KeyValuePair<TextBlock, float> profitText in Critters.ProfitTexts.ToList())
						{
							dispatcher.TryEnqueue(() =>
							{
								Canvas.SetTop(profitText.Key, Canvas.GetTop(profitText.Key) - (4f - profitText.Value));
								try
								{
									Critters.ProfitTexts[profitText.Key] += 1f / 15f;
								}
								catch (KeyNotFoundException)
								{
									System.Diagnostics.Debug.WriteLine("Profit text already gone!");
								}
								Color currentColor = ((SolidColorBrush)profitText.Key.Foreground).Color;
								profitText.Key.Foreground = new SolidColorBrush(Color.FromArgb((byte)(255f - (255f * profitText.Value / 4f)), currentColor.R, currentColor.G, currentColor.B));

								if (profitText.Value >= 4f)
								{
									App.Window?.BackgroundCanvas.Children.Remove(profitText.Key);
									Critters.ProfitTexts.Remove(profitText.Key);
								}
							});
						}
					}
					catch (ArgumentException)
					{
						System.Diagnostics.Debug.Write("Failed to loop profit texts! Retrying..");
					}

					await Save.SaveUserData();

					await Task.Delay(1000 / 60, cancel);
				}
			}
			catch { }
		}

		private void SelectCritter_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuFlyoutItem critterButton)
			{
				string critterName = critterButton.Text;
				BitmapImage critter = new()
				{
					UriSource = new Uri("ms-appx:///Assets/Critters/" + critterName + ".png")
				};
				int critterID = Critters.GetCritterID(critterName);

				CritterIcon.Source = critter;
				CritterName.Text = critterName;
				CritterCost.Text = "costs $" + Stats.Abbreviate(Stats.CritterCosts[critterID]);
				CritterProduction.Text = "makes +$" + Stats.Abbreviate(Stats.CritterMoney[critterID]) + "/s";
				SelectedCritter = critterName;
			}
		}

		private void BuyCritter_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedCritter == "")
			{
				return;
			}

			int critterID = Critters.GetCritterID(SelectedCritter);

			if ((Stats.Money >= Stats.CritterCosts[critterID]) && (Stats.MaxCritters + Stats.MaxCrittersIncrease > Stats.CritterCount))
			{
				Stats.Money -= Stats.CritterCosts[critterID];
				Stats.CritterCosts[critterID] *= 1.3d;

				Critters.SpawnCritter(SelectedCritter);
				MenuFlyoutItem fake = new()
				{
					Text = SelectedCritter
				};
				SelectCritter_Click(fake, new RoutedEventArgs());
			}
		}

		private void DeleteCritter_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedCritter == "")
			{
				return;
			}

			foreach (Critter critter in Critters.CritterList)
			{
				int critterID = Critters.GetCritterID(SelectedCritter);
				if (critter.ID == critterID)
				{
					BackgroundCanvas.Children.Remove(critter.CritterIcon);
					critter.Removed = true;
					Critters.CritterList.Remove(critter);
					Critters.CritterIDs.Remove(critter.ID);

					Stats.CritterCosts[critterID] /= 1.3d;
					Stats.Money += Stats.CritterCosts[critterID];

					MenuFlyoutItem fake = new()
					{
						Text = SelectedCritter
					};
					SelectCritter_Click(fake, new RoutedEventArgs());

					Stats.CritterCount--;
					return;
				}
			}
		}

		private void FenceSize_Click(object sender, RoutedEventArgs e)
		{
			if (Stats.Money >= Stats.UpgradeCosts["FenceSize"])
			{
				if (Stats.UpgradeAmounts["FenceSize"][0] < Stats.UpgradeAmounts["FenceSize"][1])
				{
					Stats.UpgradeAmounts["FenceSize"][0]++;

					Stats.Money -= Stats.UpgradeCosts["FenceSize"];
					Stats.UpgradeCosts["FenceSize"] *= 100;
					Fence.FenceSize++;
					Fence.BuildFence();
					SetPositions();

					Button_Highlighted(sender, new RoutedEventArgs());
				}
			}
		}

		private void CritterProfit_Click(object sender, RoutedEventArgs e)
		{
			if (Stats.Money >= Stats.UpgradeCosts["CritterProfit"])
			{
				if (Stats.UpgradeAmounts["CritterProfit"][0] < Stats.UpgradeAmounts["CritterProfit"][1])
				{
					Stats.UpgradeAmounts["CritterProfit"][0]++;

					Stats.Money -= Stats.UpgradeCosts["CritterProfit"];
					Stats.UpgradeCosts["CritterProfit"] *= 1.5;

					Button_Highlighted(sender, new RoutedEventArgs());
				}
			}
		}

		private void CritterProfitMultiplier_Click(object sender, RoutedEventArgs e)
		{
			if (Stats.Money >= Stats.UpgradeCosts["CritterProfitMultiplier"])
			{
				if (Stats.UpgradeAmounts["CritterProfitMultiplier"][0] < Stats.UpgradeAmounts["CritterProfitMultiplier"][1])
				{
					Stats.UpgradeAmounts["CritterProfitMultiplier"][0]++;

					Stats.Money -= Stats.UpgradeCosts["CritterProfitMultiplier"];
					Stats.UpgradeCosts["CritterProfitMultiplier"] *= 1.4;

					Button_Highlighted(sender, new RoutedEventArgs());
				}
			}
		}

		private void CritterCooldown_Click(object sender, RoutedEventArgs e)
		{
			if (Stats.Money >= Stats.UpgradeCosts["CritterCooldown"])
			{
				if (Stats.UpgradeAmounts["CritterCooldown"][0] < Stats.UpgradeAmounts["CritterCooldown"][1])
				{
					Stats.UpgradeAmounts["CritterCooldown"][0]++;

					Stats.Money -= Stats.UpgradeCosts["CritterCooldown"];
					Stats.UpgradeCosts["CritterCooldown"] *= 2.5;

					Button_Highlighted(sender, new RoutedEventArgs());
				}
			}
		}

		private void MaxCritter_Click(object sender, RoutedEventArgs e)
		{
			if (Stats.Money >= Stats.UpgradeCosts["MaxCritter"])
			{
				if (Stats.UpgradeAmounts["MaxCritter"][0] < Stats.UpgradeAmounts["MaxCritter"][1])
				{
					Stats.UpgradeAmounts["MaxCritter"][0]++;

					Stats.Money -= Stats.UpgradeCosts["MaxCritter"];
					Stats.MaxCrittersIncrease += 1;
					Stats.UpgradeCosts["MaxCritter"] *= 10;

					Button_Highlighted(sender, new RoutedEventArgs());
				}
			}
		}

		private void Button_Highlighted(object sender, RoutedEventArgs e)
		{
			string key = "";
			DetailsBorder.Visibility = Visibility.Visible;
			switch (((FrameworkElement)sender).Name)
			{
				case "FenceSizeUpgradeButton":
					UpgradeTitle.Text = "Fence Size Upgrade";
					key = "FenceSize";
					UpgradeDescriptionText.Text = $"Increases fence size from " +
						$"{Fence.FenceSize}" +
						$" to " +
						$"{Fence.FenceSize + 1}";
					break;
				case "CritterProfitUpgradeButton":
					UpgradeTitle.Text = "Critter Profit Upgrade";
					key = "CritterProfit";
					UpgradeDescriptionText.Text = $"Critters produce " +
						$"{Stats.Abbreviate(Stats.GlobalCritterProfit * Stats.CritterProfitMultiplier * 100)}" +
						$"% money (+" +
						$"{Stats.Abbreviate(Stats.CritterProfitMultiplier * 10)}" +
						$"% per level)";
					break;
				case "CritterProfitMultiplierUpgradeButton":
					UpgradeTitle.Text = "Critter Profit Upgrade Multiplier";
					key = "CritterProfitMultiplier";
					UpgradeDescriptionText.Text = $"Critter Profit Upgrade boost is multiplied by " +
						$"{Stats.Abbreviate(Stats.CritterProfitMultiplier)}" +
						$"x (+0.25x per level)";
					break;
				case "CritterCooldownUpgradeButton":
					UpgradeTitle.Text = "Critter Cooldown Reduction";
					key = "CritterCooldown";
					UpgradeDescriptionText.Text = $"Reduces the cooldown between critter profits by 0.02s per level " +
						$"(-{Stats.Abbreviate(Stats.CritterCooldownReduction)}s)";
					break;
				case "MaxCritterUpgradeButton":
					UpgradeTitle.Text = "Max Critters Increase";
					key = "MaxCritter";
					UpgradeDescriptionText.Text = $"Increases the amount of critters you can have in your fence by 1 " +
						$"(+{Stats.Abbreviate(Stats.MaxCrittersIncrease)} max critters)";
					break;
			}
			UpgradeLimitText.Text = $"owned {Stats.UpgradeAmounts[key][0]}/{Stats.UpgradeAmounts[key][1]}";
			UpgradeCostText.Text = $"costs ${Stats.Abbreviate(Stats.UpgradeCosts[key])}";
		}

		private void Button_Unhighlighted(object sender, RoutedEventArgs e)
		{
			DetailsBorder.Visibility = Visibility.Collapsed;
		}
	}
}

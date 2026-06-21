using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Devices.Core;
using Windows.UI;

namespace CritterRanchers
{
    class Critters
    {
		public static List<Critter> CritterList = [];
		public static List<int> CritterIDs = [];
		public static Dictionary<TextBlock, float> ProfitTexts = [];
		public static Color GetCritterColor(int ID)
		{
			return ID switch
			{
				0 => Color.FromArgb(255, 38, 125, 255),
				1 => Color.FromArgb(255, 72, 168, 77),
				2 => Color.FromArgb(255, 79, 39, 102),
				3 => Color.FromArgb(255, 138, 76, 102),
				4 => Color.FromArgb(255, 163, 113, 42),
				5 => Color.FromArgb(255, 209, 46, 196),
				6 => Color.FromArgb(255, 159, 129, 46),
				7 => Color.FromArgb(255, 142, 52, 128),
				_ => Color.FromArgb(255, 0, 0, 0)
			};
		}
        public static int GetCritterID(string critterName)
        {
			return critterName switch
			{
				"Scrant" => 0,
				"Frimbus" => 1,
				"Herbert" => 2,
				"Blingott" => 3,
				"Trummer" => 4,
				"Gerby" => 5,
				"Slemmy" => 6,
				"Schloop" => 7,
				_ => -1
			};
        }

		public static string GetCritterName(int ID)
		{
			return ID switch
			{
				0 => "Scrant",
				1 => "Frimbus",
				2 => "Herbert",
				3 => "Blingott",
				4 => "Trummer",
				5 => "Gerby",
				6 => "Slemmy",
				7 => "Schloop",
				_ => ""
			};
		}

		public static void SpawnCritter(string critter)
		{
			float speed = critter switch
			{
				"Scrant" => 60,
				"Frimbus" => 45,
				"Herbert" => 20,
				"Blingott" => 40,
				"Trummer" => 75,
				"Gerby" => 45,
				"Slemmy" => 50,
				"Schloop" => 55,
				_ => 0
			};
			double theta = Math.PI + (new Random().Next(360) * Math.PI / 180d);
			Vector2 direction = new((float)Math.Sin(theta), (float)Math.Cos(theta));
			Vector2 position = new(800, 500);

			Critter newCritter = new(critter, speed, direction, position);
			CritterList.Add(newCritter);
			CritterIDs.Add(newCritter.ID);
			Stats.CritterCount++;
		}
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace CritterRanchers
{
    class Save
    {
        public static async Task SaveUserData()
        {
            Data saveData = new()
            {
                Money = Stats.Money,
                MaxCritters = Stats.MaxCritters,
                CritterCosts = Stats.CritterCosts,
                CritterMoney = Stats.CritterMoney,
                CritterList = Critters.CritterIDs,
                UpgradeCosts = Stats.UpgradeCosts,
				UpgradeAmounts = Stats.UpgradeAmounts,
				FenceSize = Fence.FenceSize
            };

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string critterRanchersPath = Path.Combine(appDataPath, "CritterRanchers");

            if (!Directory.Exists(critterRanchersPath))
            {
                Directory.CreateDirectory(critterRanchersPath);
            }

            string storage = Path.Combine(critterRanchersPath, "stats.json");

            string JSON = JsonSerializer.Serialize(saveData);
            await File.WriteAllTextAsync(storage, JSON);
        }

        public static async Task<Data> LoadUserData()
        {
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string critterRanchersPath = Path.Combine(appDataPath, "CritterRanchers", "stats.json");

            Data saveData = new()
            {
                Money = 10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d,
                MaxCritters = 14,
                CritterCosts = Stats.CritterCosts,
                CritterMoney = Stats.CritterMoney,
                CritterList = [],
                UpgradeAmounts = Stats.UpgradeAmounts,
                FenceSize = 8
            };

			if (File.Exists(critterRanchersPath))
            {
                string JSON = await File.ReadAllTextAsync(critterRanchersPath);
                return JsonSerializer.Deserialize<Data>(JSON) ?? saveData;
            }

            return saveData;
		}
    }
}

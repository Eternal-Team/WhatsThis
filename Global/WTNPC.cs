using Terraria;
using Terraria.ModLoader;
using WhatsThis.UI;

namespace WhatsThis.Global
{
	internal class WTNPC : GlobalNPC
	{
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			spawnRate = BrowserUI.SpawnMultiplier == 0 ? 0 : spawnRate / BrowserUI.SpawnMultiplier;
			maxSpawns = maxSpawns * BrowserUI.SpawnMultiplier;
		}
	}
}
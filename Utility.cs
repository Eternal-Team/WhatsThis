using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace WhatsThis
{
	public enum SortMode
	{
		AZ,
		ZA,
		TypeAsc,
		TypeDesc
	}

	public enum RecipeType
	{
		Crafting,
		ChestLoot,
		MobLoot,
		Custom
	}

	public static class Utility
	{
		public static string ToString(this SortMode mode)
		{
			switch (mode)
			{
				case SortMode.AZ: return "A->Z";
				case SortMode.ZA: return "Z->A";
				case SortMode.TypeAsc: return "Type Ascending";
				case SortMode.TypeDesc: return "Type Descending";
				default: return "";
			}
		}

		public static bool ValidItem(Item item, string category)
		{
			switch (category)
			{
				case "Weapons": return (item.melee || item.ranged || item.magic || item.summon || item.thrown) && item.damage > 0;
				case "Armors": return item.defense > 0 && !item.accessory;
				case "Tools": return item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.fishingPole > 0;
				case "Accessories": return item.accessory;
				case "Placeables": return item.createTile > 0 || item.createWall > 0;
				case "Ammo": return item.ammo > 0 && item.shoot > 0;
				case "Consumables": return item.consumable || item.potion;
				case "Expert": return item.expert || item.expertOnly;
				case "Fishing": return item.fishingPole > 0 || item.bait > 0;
				case "Mounts": return item.mountType > 0;
				default: return false;
			}
		}

		public static bool ContainsAll<T>(this List<T> subset, List<T> superset) => superset.Except(subset).Any();

		public static bool IsEqual<T>(this List<T> list1, List<T> list2) => list1.All(list2.Contains) && list1.Count == list2.Count;
	}
}
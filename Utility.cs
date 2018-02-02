using Terraria;
using Terraria.GameContent.Events;
using TheOneLibrary.Utility;

namespace WhatsThis
{
	public enum RecipeType
	{
		Crafting,
		ChestLoot,
		MobLoot,
		Custom
	}

	public static class Utility
	{
		public static void StartRain()
		{
			typeof(Main).InvokeMethod<object>("StartRain", new object[] { });
		}

		public static void StopRain()
		{
			typeof(Main).InvokeMethod<object>("StopRain", new object[] { });
		}

		public static void StartSandstorm()
		{
			typeof(Sandstorm).InvokeMethod<object>("StartSandstorm", new object[] { });
		}

		public static void StopSandstorm()
		{
			typeof(Sandstorm).InvokeMethod<object>("StopSandstorm", new object[] { });
		}
	}
}
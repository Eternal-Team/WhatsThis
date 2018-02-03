using System.Linq;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
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

	public enum Events
	{
		BloodMoon,
		PumpkinMoon,
		FrostMoon,
		SolarEclipse,
		GoblinArmy,
		Pirates,
		FrostLegion,
		Martians,
		LunarEvent
	}

	public static class Utility
	{
		public static void StartRain() => typeof(Main).InvokeMethod<object>("StartRain");

		public static void StopRain() => typeof(Main).InvokeMethod<object>("StopRain");

		public static void StartSandstorm() => typeof(Sandstorm).InvokeMethod<object>("StartSandstorm");

		public static void StopSandstorm() => typeof(Sandstorm).InvokeMethod<object>("StopSandstorm");

		public static void StopAllEvents()
		{
			for (int i = 0; i < Main.npc.Length; i++) if (Main.npc[i] != null && !Main.npc[i].townNPC) Main.npc[i].active = false;

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Main.bloodMoon = false;
				Main.eclipse = false;
				Main.invasionType = 0;
				Main.stopMoonEvent();
			}
		}

		public static void StartBloodMoon()
		{
			if (Main.dayTime)
			{
				Main.dayTime = false;
				Main.time = 0.0;
			}
			Main.bloodMoon = true;
		}

		public static void StartSolarEclipse()
		{
			if (!Main.dayTime)
			{
				Main.dayTime = true;
				Main.time = 0.0;
			}
			Main.eclipse = true;
		}

		public static void StartPumpkinMoon()
		{
			StopAllEvents();

			if (Main.dayTime)
			{
				Main.dayTime = false;
				Main.time = 0.0;
			}
			Main.startPumpkinMoon();
		}

		public static void StartFrostMoon()
		{
			StopAllEvents();

			if (Main.dayTime)
			{
				Main.dayTime = false;
				Main.time = 0.0;
			}
			Main.startSnowMoon();
		}

		public static void StartLunarEvent()
		{
			WorldGen.TriggerLunarApocalypse();
		}

		public static void StartInvasion(int type)
		{
			Main.invasionDelay = 0;
			int numberOfPlayers = Main.player.Count(x => x.active);

			Main.invasionType = type;
			Main.invasionSize = 80 + 40 * numberOfPlayers;
			if (type == 3) Main.invasionSize += 40 + 20 * numberOfPlayers;
			Main.invasionWarn = 0;
			if (Main.rand.Next(2) == 0)
			{
				Main.invasionX = 0.0;
				return;
			}
			Main.invasionX = Main.maxTilesX;

			typeof(Main).InvokeMethod<object>("InvasionWarning");
		}

		public static void StartEvent(Events e)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				StopAllEvents();
				switch (e)
				{
					case Events.GoblinArmy:
						StartInvasion(1);
						break;
					case Events.FrostLegion:
						StartInvasion(2);
						break;
					case Events.Pirates:
						StartInvasion(3);
						break;
					case Events.SolarEclipse:
						StartSolarEclipse();
						break;
					case Events.BloodMoon:
						StartBloodMoon();
						break;
					case Events.PumpkinMoon:
						StartPumpkinMoon();
						break;
					case Events.FrostMoon:
						StartFrostMoon();
						break;
					case Events.LunarEvent:
						StartLunarEvent();
						break;
					case Events.Martians:
						StartInvasion(4);
						break;
				}
			}
		}
	}
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TheOneLibrary.Base.UI;
using TheOneLibrary.UI.Elements;
using TheOneLibrary.Utility;

namespace WhatsThis.UI
{
    public partial class BrowserUI
    {
        public static double pausedTime;
        public static bool pausedTimeBool;

        public static double Time
        {
            get { return Main.dayTime ? Main.time : Main.time + 54000; }
            set
            {
                pausedTime = value;

                if (value > 54000)
                {
                    Main.time = value - 54000;
                    Main.dayTime = false;
                }
                else
                {
                    Main.time = value;
                    Main.dayTime = true;
                }
            }
        }

        public static int IlluminationStrength;
        public static bool GravestonesToggled = true;

        public static int SpawnMultiplier = 1;

        public UIPanel panelItems = new UIPanel();

        public UIPanel panelInputItems = new UIPanel();
        public UITextInput inputItems = new UITextInput("Select to type");
        public UICycleButton buttonSortMode = new UICycleButton(WhatsThis.textureSortMode);

        public UIBrowserGrid gridItems = new UIBrowserGrid();
        public UIScrollbar barItems = new UIScrollbar();

        public UITextButton buttonCategories = new UITextButton("Category");
        public UITextButton buttonMods = new UITextButton("Mods");
        public UITextButton buttonMode = new UITextButton("Cheat");

        public UIElement sidePanel;

        public Regex regex;
        public bool cheatMode = true;

        public string sortMode;

        public static Dictionary<string, UIElement> sidePanels = new Dictionary<string, UIElement>();
        public static Dictionary<string, Func<Item, bool>> categories = new Dictionary<string, Func<Item, bool>>();
        public static Dictionary<string, Func<Item, Item, int>> sortModes = new Dictionary<string, Func<Item, Item, int>>();

        public List<string> currentMods = new List<string>();
        public List<string> currentCategories = new List<string>();

        public bool resizing;
    }

    public partial class BrowserUI : BaseUI
    {
        public override void OnInitialize()
        {
            panelMain.Width.Pixels = 552f;
            panelMain.MinWidth.Set(284f, 0f); // Min 3 columns
            panelMain.MaxWidth.Set(900f, 0f); // Max 17 columns 
            panelMain.Height.Pixels = 648f;
            panelMain.MinHeight.Set(296f, 0f); // Min 5 rows
            panelMain.MaxHeight.Set(904f, 0f); // Max 19 rows
            panelMain.Left.Set(100f, 0f);
            panelMain.Top.Set(100f, 0f);
            panelMain.SetPadding(0);
            panelMain.BackgroundColor = panelColor;
            panelMain.OnMouseDown += DragStart;
            panelMain.OnMouseUp += DragEnd;
            Append(panelMain);

            #region Top
            panelInputItems.Width.Set(-112, 1);
            panelInputItems.Height.Pixels = 40;
            panelInputItems.Left.Pixels = 8;
            panelInputItems.Top.Pixels = 8;
            panelMain.Append(panelInputItems);

            inputItems.Width.Precent = 1;
            inputItems.Height.Precent = 1;
            inputItems.OnTextChange += QueryItems;
            panelInputItems.Append(inputItems);

            buttonSortMode.Width.Pixels = 40;
            buttonSortMode.Height.Pixels = 40;
            buttonSortMode.HAlign = 1;
            buttonSortMode.Left.Pixels = -56;
            buttonSortMode.Top.Pixels = 8;
            buttonSortMode.HoverText += () => sortMode;
            buttonSortMode.OnClick += SortClick;
            buttonSortMode.OnRightClick += SortClick;
            panelMain.Append(buttonSortMode);

            // Options
            #endregion

            #region Items
            panelItems.Width.Set(-112, 1); // + 44
            panelItems.Height.Set(-64, 1); // + 16
            panelItems.Left.Pixels = 8;
            panelItems.Top.Pixels = 56;
            panelItems.SetPadding(0);
            panelMain.Append(panelItems);

            gridItems.Width.Set(-44, 1); // 128
            gridItems.Height.Set(-16, 1); // 128
            gridItems.Left.Pixels = 8;
            gridItems.Top.Pixels = 8;
            gridItems.ListPadding = 4;
            gridItems.OverflowHidden = true;
            panelItems.Append(gridItems);

            CalculatedStyle dimensions = gridItems.GetDimensions();
            gridItems.columns = (int)(dimensions.Width / 44);

            barItems.Height.Set(-16, 1);
            barItems.Left.Set(-28, 1);
            barItems.Top.Set(8, 0);
            barItems.SetView(100f, 1000f);
            panelItems.Append(barItems);
            gridItems.SetScrollbar(barItems);
            #endregion

            #region Right Panel
            buttonCategories.Width.Pixels = 88;
            buttonCategories.Height.Pixels = 40;
            buttonCategories.AlignRight(8);
            buttonCategories.Top.Pixels = 56;
            buttonCategories.OnClick += (a, sender) => OpenPanel(sender, "Categories");
            panelMain.Append(buttonCategories);

            buttonMods.Width.Pixels = 88;
            buttonMods.Height.Pixels = 40;
            buttonMods.AlignRight(8);
            buttonMods.Top.Pixels = 104;
            buttonMods.OnClick += (a, sender) => OpenPanel(sender, "Mods");
            panelMain.Append(buttonMods);

            buttonMode.Width.Pixels = 88;
            buttonMode.Height.Pixels = 40;
            buttonMode.AlignRight(8);
            buttonMode.AlignBottom(8);
            buttonMode.OnClick += (a, b) =>
            {
                cheatMode = !cheatMode;
                buttonMode.text = cheatMode ? "Cheat" : "Recipe";
            };
            panelMain.Append(buttonMode);
            #endregion
        }

        public void InitCategories()
        {
            categories.Add("Weapons", item => (item.melee || item.ranged || item.magic || item.summon || item.thrown) && item.damage > 0);
            categories.Add("Armors", item => item.defense > 0 && !item.accessory);
            categories.Add("Tools", item => item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.fishingPole > 0);
            categories.Add("Accessories", item => item.accessory);
            categories.Add("Placeables", item => item.createTile > 0 || item.createWall > 0);
            categories.Add("Ammo", item => item.ammo > 0 && item.shoot > 0);
            categories.Add("Consumables", item => item.consumable || item.potion);
            categories.Add("Expert", item => item.expert || item.expertOnly);
            categories.Add("Fishing", item => item.fishingPole > 0 || item.bait > 0);
            categories.Add("Mounts", item => item.mountType > 0);
        }

        public void InitSortModes()
        {
            sortModes.Add("Type Ascending", (item1, item2) => item1.type.CompareTo(item2.type));
            sortModes.Add("Type Descending", (item1, item2) => -item1.type.CompareTo(item2.type));
            sortModes.Add("A->Z", (item1, item2) => string.Compare(item1.HoverName, item2.HoverName, StringComparison.Ordinal));
            sortModes.Add("Z->A", (item1, item2) => -string.Compare(item1.HoverName, item2.HoverName, StringComparison.Ordinal));

            sortMode = sortModes.First().Key;
        }

        public void InitCheatButtons()
        {
            UIButton buttonEntity = new UIButton(WhatsThis.textureEntity);
            buttonEntity.Width.Pixels = 40;
            buttonEntity.Height.Pixels = 40;
            buttonEntity.AlignRight(56);
            buttonEntity.Top.Pixels = 152;
            buttonEntity.HoverText += () => "Items/NPCs";
            buttonEntity.OnClick += (a, sender) => OpenPanel(sender, "Entity");
            panelMain.Append(buttonEntity);

            UIButton buttonWorld = new UIButton(WhatsThis.textureWorld);
            buttonWorld.Width.Pixels = 40;
            buttonWorld.Height.Pixels = 40;
            buttonWorld.AlignRight(8);
            buttonWorld.Top.Pixels = 152;
            buttonWorld.HoverText += () => "World";
            buttonWorld.OnClick += (a, sender) => OpenPanel(sender, "World");
            panelMain.Append(buttonWorld);
        }

        public void InitPanels()
        {
            #region Search
            UIPanel panelCategories = new UIPanel();
            panelCategories.Width.Pixels = categories.Select(x => Main.fontMouseText.MeasureString(x.Key).X).OrderByDescending(x => x).First() + 28;
            panelCategories.Top.Pixels = 56;
            panelCategories.SetPadding(0);
            sidePanels.Add("Categories", panelCategories);

            UIGrid gridCategories = new UIGrid();
            gridCategories.Width.Set(-16, 1);
            gridCategories.Height.Set(-16, 1);
            gridCategories.Left.Pixels = 8;
            gridCategories.Top.Pixels = 8;
            gridCategories.ListPadding = 4;
            gridCategories.OverflowHidden = true;
            panelCategories.Append(gridCategories);

            UIScrollbar barCategories = new UIScrollbar();
            barCategories.SetView(100f, 1000f);
            gridCategories.SetScrollbar(barCategories);

            UIPanel panelMods = new UIPanel();
            panelMods.Width.Pixels = 300;
            panelMods.Top.Pixels = 104;
            panelMods.SetPadding(0);
            sidePanels.Add("Mods", panelMods);

            UIGrid gridMods = new UIGrid();
            gridMods.Width.Set(-16, 1);
            gridMods.Height.Set(-16, 1);
            gridMods.Left.Pixels = 8;
            gridMods.Top.Pixels = 8;
            gridMods.ListPadding = 4;
            gridMods.OverflowHidden = true;
            panelMods.Append(gridMods);

            UIScrollbar barMods = new UIScrollbar();
            barMods.SetView(100f, 1000f);
            gridMods.SetScrollbar(barMods);
            #endregion

            #region Entity
            UIPanel panelEntity = new UIPanel();
            panelEntity.Width.Pixels = 296;
            panelEntity.Top.Pixels = 156;
            panelEntity.SetPadding(0);
            sidePanels.Add("Entity", panelEntity);

            #region Trash
            UIPanel panelTrash = new UIPanel();
            panelTrash.Width.Set(0f, 1f);
            panelTrash.Height.Pixels = 84;
            panelTrash.SetPadding(0);
            panelEntity.Append(panelTrash);

            UIText textTrash = new UIText("Trash");
            textTrash.HAlign = 0.5f;
            textTrash.Top.Pixels = 8;
            panelTrash.Append(textTrash);

            UIButton buttonTrashWorldItems = new UIButton(Main.itemTexture[ItemID.Compass]);
            buttonTrashWorldItems.Width.Pixels = 40;
            buttonTrashWorldItems.Height.Pixels = 40;
            buttonTrashWorldItems.Left.Pixels = 8;
            buttonTrashWorldItems.Top.Pixels = 36;
            buttonTrashWorldItems.HoverText += () => "Delete items from world";
            buttonTrashWorldItems.OnClick += (a, b) =>
            {
                Main.NewText("Deleted " + Main.item.Count(x => x.active) + " items");
                for (int i = 0; i < Main.item.Length; i++) Main.item[i].active = false;
            };
            panelTrash.Append(buttonTrashWorldItems);

            UIButton buttonTrashItems = new UIButton(Main.inventoryBackTexture);
            buttonTrashItems.Width.Pixels = 40;
            buttonTrashItems.Height.Pixels = 40;
            buttonTrashItems.Left.Pixels = 56;
            buttonTrashItems.Top.Pixels = 36;
            buttonTrashItems.HoverText += () => "Delete unfavourited items from inventory";
            buttonTrashItems.OnClick += (a, b) =>
            {
                Main.NewText("Deleted " + Main.LocalPlayer.inventory.Count(x => !x.IsAir && !x.favorited) + " items", Color.Yellow);
                for (int i = 0; i < Main.LocalPlayer.inventory.Length; i++) if (!Main.LocalPlayer.inventory[i].favorited) Main.LocalPlayer.inventory[i].TurnToAir();
            };
            panelTrash.Append(buttonTrashItems);

            UIButton buttonTrashNPCs = new UIButton(Main.npcHeadBossTexture[29]);
            buttonTrashNPCs.Width.Pixels = 40;
            buttonTrashNPCs.Height.Pixels = 40;
            buttonTrashNPCs.Left.Pixels = 104;
            buttonTrashNPCs.Top.Pixels = 36;
            buttonTrashNPCs.HoverText += () => "Delete NPCs from world";
            buttonTrashNPCs.OnClick += (a, b) =>
            {
                Main.NewText("Deleted " + Main.npc.Count(x => x.active && !x.townNPC) + " NPCs");
                for (int i = 0; i < Main.npc.Length; i++) if (Main.npc[i] != null && !Main.npc[i].townNPC) Main.npc[i].active = false;
            };
            panelTrash.Append(buttonTrashNPCs);
            #endregion

            #region Invasion
            UIPanel panelInvasion = new UIPanel();
            panelInvasion.Width.Set(0f, 1f);
            panelInvasion.Height.Pixels = 132;
            panelInvasion.Top.Pixels = 92;
            panelInvasion.SetPadding(0);
            panelEntity.Append(panelInvasion);

            UIText textInvasion = new UIText("Invasion");
            textInvasion.HAlign = 0.5f;
            textInvasion.Top.Pixels = 8;
            panelInvasion.Append(textInvasion);

            UIButton buttonStopAll = new UIButton(WhatsThis.textureMagnet);
            buttonStopAll.Width.Pixels = 40;
            buttonStopAll.Height.Pixels = 40;
            buttonStopAll.Left.Pixels = 8;
            buttonStopAll.Top.Pixels = 36;
            buttonStopAll.HoverText += () => "Stop all invasions";
            buttonStopAll.OnClick += (a, b) => TheOneLibrary.Utility.Utility.StopAllEvents();
            panelInvasion.Append(buttonStopAll);

            UIButton buttonBloodMoon = new UIButton(Main.itemTexture[ItemID.BloodWater]);
            buttonBloodMoon.Width.Pixels = 40;
            buttonBloodMoon.Height.Pixels = 40;
            buttonBloodMoon.Left.Pixels = 56;
            buttonBloodMoon.Top.Pixels = 36;
            buttonBloodMoon.HoverText += () => "Start Blood Moon";
            buttonBloodMoon.OnClick += (a, b) => TheOneLibrary.Utility.Utility.StartEvent(Events.BloodMoon);
            panelInvasion.Append(buttonBloodMoon);

            UIButton buttonPumpkinMoon = new UIButton(Main.extraTexture[12]);
            buttonPumpkinMoon.Width.Pixels = 40;
            buttonPumpkinMoon.Height.Pixels = 40;
            buttonPumpkinMoon.Left.Pixels = 104;
            buttonPumpkinMoon.Top.Pixels = 36;
            buttonPumpkinMoon.HoverText += () => "Start Pumpkin Moon";
            buttonPumpkinMoon.OnClick += (a, b) => TheOneLibrary.Utility.Utility.StartEvent(Events.PumpkinMoon);
            panelInvasion.Append(buttonPumpkinMoon);

            UIButton buttonFrostMoon = new UIButton(Main.extraTexture[8]);
            buttonFrostMoon.Width.Pixels = 40;
            buttonFrostMoon.Height.Pixels = 40;
            buttonFrostMoon.Left.Pixels = 152;
            buttonFrostMoon.Top.Pixels = 36;
            buttonFrostMoon.HoverText += () => "Start Frost Moon";
            buttonFrostMoon.OnClick += (a, b) => TheOneLibrary.Utility.Utility.StartEvent(Events.FrostMoon);
            panelInvasion.Append(buttonFrostMoon);

            UIButton buttonSolarEclipse = new UIButton(Main.itemTexture[ItemID.SolarTablet]);
            buttonSolarEclipse.Width.Pixels = 40;
            buttonSolarEclipse.Height.Pixels = 40;
            buttonSolarEclipse.Left.Pixels = 200;
            buttonSolarEclipse.Top.Pixels = 36;
            buttonSolarEclipse.HoverText += () => "Start Solar Eclipse";
            buttonSolarEclipse.OnClick += (a, b) => TheOneLibrary.Utility.Utility.StartEvent(Events.SolarEclipse);
            panelInvasion.Append(buttonSolarEclipse);

            UIButton buttonLunarEvent = new UIButton(Main.itemTexture[ItemID.CelestialSigil]);
            buttonLunarEvent.Width.Pixels = 40;
            buttonLunarEvent.Height.Pixels = 40;
            buttonLunarEvent.Left.Pixels = 248;
            buttonLunarEvent.Top.Pixels = 36;
            buttonLunarEvent.HoverText += () => "Start Lunar Event";
            buttonLunarEvent.OnClick += (a, b) => TheOneLibrary.Utility.Utility.StartEvent(Events.LunarEvent);
            panelInvasion.Append(buttonLunarEvent);

            UIButton buttonGoblinArmy = new UIButton(Main.extraTexture[9]);
            buttonGoblinArmy.Width.Pixels = 40;
            buttonGoblinArmy.Height.Pixels = 40;
            buttonGoblinArmy.Left.Pixels = 8;
            buttonGoblinArmy.Top.Pixels = 84;
            buttonGoblinArmy.HoverText += () => "Start Goblin Army";
            buttonGoblinArmy.OnClick += (a, b) => TheOneLibrary.Utility.Utility.StartEvent(Events.GoblinArmy);
            panelInvasion.Append(buttonGoblinArmy);

            UIButton buttonPirates = new UIButton(Main.extraTexture[11]);
            buttonPirates.Width.Pixels = 40;
            buttonPirates.Height.Pixels = 40;
            buttonPirates.Left.Pixels = 56;
            buttonPirates.Top.Pixels = 84;
            buttonPirates.HoverText += () => "Start Pirates";
            buttonPirates.OnClick += (a, b) => TheOneLibrary.Utility.Utility.StartEvent(Events.Pirates);
            panelInvasion.Append(buttonPirates);

            UIButton buttonFrostLegion = new UIButton(Main.extraTexture[7]);
            buttonFrostLegion.Width.Pixels = 40;
            buttonFrostLegion.Height.Pixels = 40;
            buttonFrostLegion.Left.Pixels = 104;
            buttonFrostLegion.Top.Pixels = 84;
            buttonFrostLegion.HoverText += () => "Start Frost Legion";
            buttonFrostLegion.OnClick += (a, b) => TheOneLibrary.Utility.Utility.StartEvent(Events.FrostLegion);
            panelInvasion.Append(buttonFrostLegion);

            UIButton buttonMartians = new UIButton(Main.extraTexture[10]);
            buttonMartians.Width.Pixels = 40;
            buttonMartians.Height.Pixels = 40;
            buttonMartians.Left.Pixels = 152;
            buttonMartians.Top.Pixels = 84;
            buttonMartians.HoverText += () => "Start Martian Madness";
            buttonMartians.OnClick += (a, b) => TheOneLibrary.Utility.Utility.StartEvent(Events.Martians);
            panelInvasion.Append(buttonMartians);
            #endregion

            #region Other
            UIPanel panelOther = new UIPanel();
            panelOther.Width.Set(0f, 1f);
            panelOther.Height.Pixels = 108;
            panelOther.Top.Pixels = 232;
            panelOther.SetPadding(0);
            panelEntity.Append(panelOther);

            UIText textOther = new UIText("Other");
            textOther.HAlign = 0.5f;
            textOther.Top.Pixels = 8;
            panelOther.Append(textOther);

            UIButton buttonMagnet = new UIButton(WhatsThis.textureMagnet);
            buttonMagnet.Width.Pixels = 40;
            buttonMagnet.Height.Pixels = 40;
            buttonMagnet.Left.Pixels = 8;
            buttonMagnet.Top.Pixels = 36;
            buttonMagnet.HoverText += () => "Magnet all items";
            buttonMagnet.OnClick += (a, b) =>
            {
                Player player = Main.LocalPlayer;

                for (int i = 0; i < Main.item.Length; i++)
                {
                    if (Main.item[i].active)
                    {
                        Main.item[i].position = player.position;
                        if (Main.netMode == NetmodeID.MultiplayerClient) NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i, Main.item[i].netID);
                    }
                }
            };
            panelOther.Append(buttonMagnet);

            UISlider sliderMultiplier = new UISlider(typeof(BrowserUI).GetField("SpawnMultiplier"), 0, 25);
            sliderMultiplier.Width.Set(-16f, 1f);
            sliderMultiplier.Left.Pixels = 8;
            sliderMultiplier.Top.Pixels = 84;
            sliderMultiplier.HoverText += value => $"Spawn rate multiplier: {value}x";
            panelOther.Append(sliderMultiplier);
            #endregion
            #endregion

            #region World
            UIPanel panelWorld = new UIPanel();
            panelWorld.Width.Pixels = 296;
            panelWorld.Top.Pixels = 156;
            panelWorld.SetPadding(0);
            sidePanels.Add("World", panelWorld);

            #region Time
            UIPanel panelTime = new UIPanel();
            panelTime.Width.Set(0f, 1f);
            panelTime.Height.Pixels = 64;
            panelTime.SetPadding(0);
            panelWorld.Append(panelTime);

            UIText textTime = new UIText("Time");
            textTime.HAlign = 0.5f;
            textTime.Top.Pixels = 8;
            panelTime.Append(textTime);

            UICycleButton buttonPause = new UICycleButton(WhatsThis.texturePause, WhatsThis.texturePlay);
            buttonPause.Width.Pixels = 16;
            buttonPause.Height.Pixels = 16;
            buttonPause.Left.Pixels = 8;
            buttonPause.Top.Pixels = 36;
            buttonPause.OnClick += (a, b) =>
            {
                pausedTimeBool = !pausedTimeBool;
                if (pausedTimeBool) pausedTime = Time;
            };
            panelTime.Append(buttonPause);

            UIStepSlider sliderTime = new UIStepSlider(typeof(BrowserUI).GetProperty("Time"), 0, 86400, 0, 27000, 54000, 70200);
            sliderTime.Width.Set(-40, 1);
            sliderTime.Left.Pixels = 32;
            sliderTime.Top.Pixels = 36;
            sliderTime.HoverText += time =>
            {
                string text4 = "AM";
                double num6 = Main.time;
                if (!Main.dayTime) num6 += 54000.0;
                num6 = num6 / 86400.0 * 24.0;
                const double num7 = 7.5;
                num6 = num6 - num7 - 12.0;
                if (num6 < 0.0) num6 += 24.0;
                if (num6 >= 12.0) text4 = "PM";
                int num8 = (int)num6;
                double num9 = num6 - num8;
                num9 = (int)(num9 * 60.0);
                string text5 = string.Concat(num9);
                if (num9 < 10.0) text5 = "0" + text5;
                if (num8 > 12) num8 -= 12;
                if (num8 == 0) num8 = 12;
                return string.Concat(num8, ":", text5, " ", text4);
            };
            panelTime.Append(sliderTime);
            #endregion

            #region Weather
            UIPanel panelWeather = new UIPanel();
            panelWeather.Width.Set(0f, 1f);
            panelWeather.Height.Pixels = 84;
            panelWeather.Top.Pixels = 72;
            panelWeather.SetPadding(0);
            panelWorld.Append(panelWeather);

            UIText textWeather = new UIText("Weather");
            textWeather.HAlign = 0.5f;
            textWeather.Top.Pixels = 8;
            panelWeather.Append(textWeather);

            UIButton buttonRain = new UIButton(Main.itemTexture[ItemID.Umbrella]);
            buttonRain.Width.Pixels = 40;
            buttonRain.Height.Pixels = 40;
            buttonRain.Left.Pixels = 8;
            buttonRain.Top.Pixels = 36;
            buttonRain.HoverText += () => Main.raining ? "Stop rain" : "Start rain";
            buttonRain.OnClick += (a, b) =>
            {
                if (Main.raining) TheOneLibrary.Utility.Utility.StopRain();
                else TheOneLibrary.Utility.Utility.StartRain();
            };
            panelWeather.Append(buttonRain);

            UIButton buttonSandstorm = new UIButton(Main.itemTexture[ItemID.SandstorminaBottle]);
            buttonSandstorm.Width.Pixels = 40;
            buttonSandstorm.Height.Pixels = 40;
            buttonSandstorm.Left.Pixels = 56;
            buttonSandstorm.Top.Pixels = 36;
            buttonSandstorm.HoverText += () => Sandstorm.Happening ? "Stop sandstorm" : "Start sandstorm";
            buttonSandstorm.OnClick += (a, b) =>
            {
                if (Sandstorm.Happening) TheOneLibrary.Utility.Utility.StopSandstorm();
                else TheOneLibrary.Utility.Utility.StartSandstorm();
            };
            panelWeather.Append(buttonSandstorm);
            #endregion

            #region Other
            panelOther = new UIPanel();
            panelOther.Width.Set(0f, 1f);
            panelOther.Height.Pixels = 108;
            panelOther.Top.Pixels = 164;
            panelOther.SetPadding(0);
            panelWorld.Append(panelOther);

            textOther = new UIText("Other");
            textOther.HAlign = 0.5f;
            textOther.Top.Pixels = 8;
            panelOther.Append(textOther);

            UISlider sliderIllumination = new UISlider(typeof(BrowserUI).GetField("IlluminationStrength"));
            sliderIllumination.Width.Set(-16f, 1f);
            sliderIllumination.Left.Pixels = 8;
            sliderIllumination.Top.Pixels = 36;
            sliderIllumination.HoverText += value => $"Illumination strength: {value} %";
            panelOther.Append(sliderIllumination);

            UIButton buttonMapReveal = new UIButton(Main.mapIconTexture[0]);
            buttonMapReveal.Width.Pixels = 40;
            buttonMapReveal.Height.Pixels = 40;
            buttonMapReveal.Left.Pixels = 8;
            buttonMapReveal.Top.Pixels = 60;
            buttonMapReveal.HoverText += () => "Reveal map";
            buttonMapReveal.OnClick += (a, b) =>
            {
                for (int i = 0; i < Main.maxTilesX; i++)
                {
                    for (int j = 0; j < Main.maxTilesY; j++)
                    {
                        if (WorldGen.InWorld(i, j)) Main.Map.Update(i, j, 255);
                    }
                }
                Main.refreshMap = true;
            };
            panelOther.Append(buttonMapReveal);

            UIButton buttonToggleGravestones = new UIButton(Main.projectileTexture[ProjectileID.Tombstone]);
            buttonToggleGravestones.Width.Pixels = 40;
            buttonToggleGravestones.Height.Pixels = 40;
            buttonToggleGravestones.Left.Pixels = 56;
            buttonToggleGravestones.Top.Pixels = 60;
            buttonToggleGravestones.HoverText += () => GravestonesToggled ? "Disable gravestones" : "Enable gravestones";
            buttonToggleGravestones.OnClick += (a, b) => { GravestonesToggled = !GravestonesToggled; };
            panelOther.Append(buttonToggleGravestones);

            UIButton buttonSpawnPoint = new UIButton(Main.itemTexture[ItemID.Bed]);
            buttonSpawnPoint.Width.Pixels = 40;
            buttonSpawnPoint.Height.Pixels = 40;
            buttonSpawnPoint.Left.Pixels = 104;
            buttonSpawnPoint.Top.Pixels = 60;
            buttonSpawnPoint.HoverText += () => $"Current spawn point: {Main.spawnTileX};{Main.spawnTileY}";
            buttonSpawnPoint.OnClick += (a, b) =>
            {
                Player player = Main.LocalPlayer;

                Main.spawnTileX = (int)(player.position.X - 8 + player.width / 2f) / 16;
                Main.spawnTileY = (int)(player.position.Y + player.height) / 16;
            };
            panelOther.Append(buttonSpawnPoint);
            #endregion
            #endregion
        }

        public void PopulateMods()
        {
            UIGrid gridMods = (UIGrid)typeof(UIElement).GetFieldValue<List<UIElement>>("Elements", sidePanels["Mods"]).First(x => x is UIGrid);

            gridMods.Clear();
            foreach (Mod mod in ModLoader.GetLoadedMods().Select(ModLoader.GetMod).Where(x => !x.Name.StartsWith("ModLoader") && typeof(Mod).GetFieldValue<Dictionary<string, ModItem>>("items", x).Count > 0))
            {
                UIModItem modItem = new UIModItem(mod);
                modItem.Width.Precent = 1;
                modItem.Height.Pixels = 40;
                modItem.OnClick += (e, element) =>
                {
                    if (currentMods.Contains(modItem.mod.Name))
                    {
                        modItem.SetInactive();
                        currentMods.Remove(modItem.mod.Name);

                        UpdateItems();

                        if (!currentMods.Any()) gridMods.items.ForEach(x => ((UIModItem)x).SetActive());
                    }
                    else
                    {
                        if (!currentMods.Any())
                        {
                            currentMods.Clear();
                            gridMods.items.ForEach(x => ((UIModItem)x).SetInactive());
                        }

                        modItem.SetActive();
                        currentMods.Add(modItem.mod.Name);

                        UpdateItems();
                    }
                };
                gridMods.Add(modItem);
            }
        }

        public void PopulateCategories()
        {
            UIGrid gridCategories = (UIGrid)typeof(UIElement).GetFieldValue<List<UIElement>>("Elements", sidePanels["Categories"]).First(x => x is UIGrid);

            gridCategories.Clear();
            foreach (KeyValuePair<string, Func<Item, bool>> categoryName in categories)
            {
                UICategoryItem category = new UICategoryItem(categoryName.Key);
                category.Width.Precent = 1;
                category.Height.Pixels = 40;
                category.OnClick += (e, element) =>
                {
                    if (currentCategories.Contains(category.category))
                    {
                        category.SetInactive();
                        currentCategories.Remove(category.category);

                        UpdateItems();

                        if (currentCategories.Count == 0) gridCategories.items.ForEach(x => ((UICategoryItem)x).SetActive());
                    }
                    else
                    {
                        if (currentCategories.Count == 0)
                        {
                            currentCategories.Clear();
                            gridCategories.items.ForEach(x => ((UICategoryItem)x).SetInactive());
                        }

                        category.SetActive();
                        currentCategories.Add(category.category);
                        UpdateItems();
                    }
                };

                gridCategories.Add(category);
            }
        }

        public void OpenPanel(UIElement sender, string key)
        {
            if (sidePanels.ContainsKey(key))
            {
                if (sidePanel == sidePanels[key])
                {
                    RemoveChild(sidePanel);
                    sidePanel = null;
                }
                else
                {
                    if (sidePanel != null) RemoveChild(sidePanel);

                    CalculatedStyle dimensions = panelMain.GetDimensions();
                    sidePanel = sidePanels[key];
                    sidePanel.Id = sender.Top.Pixels.ToString();
                    sidePanel.Height.Set(dimensions.Height - sender.Top.Pixels, 0);
                    sidePanel.Left.Set(dimensions.X + dimensions.Width, 0f);
                    sidePanel.Top.Set(dimensions.Y + sender.Top.Pixels, 0f);
                    sidePanel.Recalculate();
                    sidePanel.RecalculateChildren();
                    Append(sidePanel);
                    RecalculatePanels();
                }
            }
        }

        private void SortClick(UIMouseEvent evt, UIElement element)
        {
            sortMode = sortModes.ElementAt(buttonSortMode.index).Key;

            gridItems.UpdateOrder();
            gridItems.RecalculateChildren();
        }

        public void UpdateItems()
        {
            for (int i = 0; i < gridItems.Count; i++) gridItems.items[i].visible = gridItems.items[i].PassFilters();
            gridItems.Recalculate();
            gridItems.RecalculateChildren();
        }

        public void QueryItems()
        {
            string pattern = inputItems.GetText().ToLower();
            try
            {
                regex = new Regex(pattern.TrimStart('#'));
                panelInputItems.BorderColor = Color.Black;

                UpdateItems();
            }
            catch (ArgumentException)
            {
                panelInputItems.BorderColor = Color.Red;
            }
        }

        public void DisplayRecipe()
        {
            //if (!inputItems.focused)
            //{
            //}
        }

        public void DisplayUsage()
        {
            //if (!inputItems.focused)
            //{
            //}
        }

        public void RecalculatePanels()
        {
            CalculatedStyle dimensions = panelMain.GetDimensions();

            if (sidePanel != null)
            {
                float top = float.Parse(sidePanel.Id);
                sidePanel.Height.Set(dimensions.Height - top, 0);
                sidePanel.Left.Set(dimensions.X + dimensions.Width, 0f);
                sidePanel.Top.Set(dimensions.Y + top, 0f);
                sidePanel.Recalculate();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (pausedTimeBool) Time = pausedTime;

            base.Update(gameTime);
        }

        #region Dragging & Resizing
        public override void DragStart(UIMouseEvent evt, UIElement listeningElement)
        {
            if (evt.Target != panelMain) return;

            CalculatedStyle dimensions = panelMain.GetInnerDimensions();

            if (new Rectangle((int)(dimensions.X + dimensions.Width - 8), (int)(dimensions.Y + dimensions.Height - 8), 8, 8).Contains(evt.MousePosition))
            {
                offset = new Vector2(evt.MousePosition.X - dimensions.X - dimensions.Width, evt.MousePosition.Y - dimensions.Y - dimensions.Height);
                resizing = true;
            }
            else
            {
                offset = new Vector2(evt.MousePosition.X - panelMain.Left.Pixels, evt.MousePosition.Y - panelMain.Top.Pixels);
                dragging = true;
            }
        }

        public override void DragEnd(UIMouseEvent evt, UIElement listeningElement)
        {
            resizing = false;
            dragging = false;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = panelMain.GetOuterDimensions();

            if (panelMain.ContainsPoint(Main.MouseScreen) || (sidePanel?.ContainsPoint(Main.MouseScreen) ?? false))
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.LocalPlayer.showItemIcon = false;
                Main.ItemIconCacheUpdate(0);
            }

            if (dragging)
            {
                panelMain.Left.Set(MathHelper.Clamp(Main.MouseScreen.X - offset.X, 0, Main.screenWidth - dimensions.Width - (sidePanel?.GetDimensions().Width ?? 0)), 0f);
                panelMain.Top.Set(MathHelper.Clamp(Main.MouseScreen.Y - offset.Y, 0, Main.screenHeight - dimensions.Height), 0f);
                panelMain.Recalculate();

                RecalculatePanels();
            }
            if (resizing)
            {
                panelMain.Width.Set((Main.MouseScreen.X - dimensions.X - offset.X - 156).ToNearest(44) + 156, 0);
                panelMain.Height.Set((Main.MouseScreen.Y - dimensions.Y - offset.Y - 80).ToNearest(44) + 80, 0);
                panelMain.Recalculate();
                panelMain.RecalculateChildren();

                CalculatedStyle dimensionsGrid = gridItems.GetDimensions();
                gridItems.columns = (int)Math.Round(dimensionsGrid.Width / 44);

                RecalculatePanels();
            }
        }
        #endregion
    }
}
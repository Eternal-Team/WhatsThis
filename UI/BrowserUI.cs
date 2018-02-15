using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TheOneLibrary.Base.UI;
using TheOneLibrary.UI.Elements;
using TheOneLibrary.Utils;
using WhatsThis.UI.Elements;

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

		public static int SpawnMultiplier = 1;

		public static int IlluminationStrength = 0;

		public static bool GravestonesToggled = true;

		public UIPanel panelItems = new UIPanel();

		public UIPanel panelInputItems = new UIPanel();
		public UITextInput inputItems = new UITextInput("Select to type");
		public UICycleButton buttonSortMode = new UICycleButton(WhatsThis.textureSortMode);

		public UIBrowserGrid gridItems = new UIBrowserGrid();
		public UIScrollbar barItems = new UIScrollbar();

		public UITextButton buttonCategories = new UITextButton("Category");
		public UITextButton buttonMods = new UITextButton("Mods");
		public UITextButton buttonCheatMenu = new UITextButton("Cheat Menu");
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
			InitCategories();
			InitSortModes();
			InitPanels();

			panelMain.Width.Pixels = 552f;
			panelMain.MinWidth.Set(284f, 0f); // Min 3 columns
			panelMain.MaxWidth.Set(900f, 0f); // Max 17 columns 
			panelMain.Height.Pixels = 648f;
			panelMain.MinHeight.Set(296f, 0f); // Min 5 rows
			panelMain.MaxHeight.Set(912f, 0f); // Max 19 rows
			panelMain.Center();
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

			buttonCheatMenu.Width.Pixels = 88;
			buttonCheatMenu.Height.Pixels = 40;
			buttonCheatMenu.AlignRight(8);
			buttonCheatMenu.Top.Pixels = 152;
			buttonCheatMenu.OnClick += (a, sender) =>
			{
				WhatsThis.Instance.CheatUI.visible = !WhatsThis.Instance.CheatUI.visible;
			};
			panelMain.Append(buttonCheatMenu);

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

		public void InitPanels()
		{
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
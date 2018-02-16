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
	public class MobUI : BaseUI
	{
		public UIPanel panelInputMobs = new UIPanel();
		public UITextInput inputMobs = new UITextInput("Select to type");

		public UIPanel panelMobs = new UIPanel();
		public UINPCGrid gridMobs = new UINPCGrid();
		public UIScrollbar barMobs = new UIScrollbar();

		public UICycleButton buttonSortMode = new UICycleButton(WhatsThis.textureSortMode);
		public UITextButton buttonCategories = new UITextButton("Category");
		public UITextButton buttonMods = new UITextButton("Mods");
		public UITextButton buttonMode = new UITextButton("Cheat");
		public bool cheatMode = true;

		public static Dictionary<string, UIElement> sidePanels = new Dictionary<string, UIElement>();
		public static Dictionary<string, Func<NPC, bool>> categories = new Dictionary<string, Func<NPC, bool>>();
		public static Dictionary<string, Func<NPC, NPC, int>> sortModes = new Dictionary<string, Func<NPC, NPC, int>>();

		public List<string> currentMods = new List<string>();
		public List<string> currentCategories = new List<string>();
		public UIElement sidePanel;

		public Regex regex;
		public string sortMode;
		public bool resizing;

		public override void OnInitialize()
		{
			InitCategories();
			InitSortModes();
			InitPanels();

			panelMain.Width.Pixels = 552f;
			panelMain.MinWidth.Set(552f, 0f); // Min 7 columns
			panelMain.MaxWidth.Set(804f, 0f); // Max 17 columns 
			panelMain.Height.Pixels = 648f;
			panelMain.MinHeight.Set(384f, 0f); // Min 7 rows
			panelMain.MaxHeight.Set(912f, 0f); // Max 19 rows
			panelMain.Center();
			panelMain.SetPadding(0);
			panelMain.BackgroundColor = PanelColor;
			panelMain.OnMouseDown += DragStart;
			panelMain.OnMouseUp += DragEnd;
			Append(panelMain);

			panelInputMobs.Width.Set(-112, 1);
			panelInputMobs.Height.Pixels = 40;
			panelInputMobs.Left.Pixels = 8;
			panelInputMobs.Top.Pixels = 8;
			panelMain.Append(panelInputMobs);

			inputMobs.Width.Precent = 1;
			inputMobs.Height.Precent = 1;
			inputMobs.OnTextChange += QueryMobs;
			panelInputMobs.Append(inputMobs);

			panelMobs.Width.Set(-112, 1); // + 44
			panelMobs.Height.Set(-64, 1); // + 16
			panelMobs.Left.Pixels = 8;
			panelMobs.Top.Pixels = 56;
			panelMobs.SetPadding(0);
			panelMain.Append(panelMobs);

			gridMobs.Width.Set(-44, 1); // 128
			gridMobs.Height.Set(-16, 1); // 128
			gridMobs.Left.Pixels = 8;
			gridMobs.Top.Pixels = 8;
			gridMobs.ListPadding = 4;
			gridMobs.OverflowHidden = true;
			panelMobs.Append(gridMobs);

			CalculatedStyle dimensions = gridMobs.GetDimensions();
			gridMobs.columns = (int)(dimensions.Width / 44);

			barMobs.Height.Set(-16, 1);
			barMobs.Left.Set(-28, 1);
			barMobs.Top.Set(8, 0);
			barMobs.SetView(100f, 1000f);
			panelMobs.Append(barMobs);
			gridMobs.SetScrollbar(barMobs);

			buttonSortMode.Width.Pixels = 40;
			buttonSortMode.Height.Pixels = 40;
			buttonSortMode.HAlign = 1;
			buttonSortMode.Left.Pixels = -56;
			buttonSortMode.Top.Pixels = 8;
			buttonSortMode.HoverText += () => sortMode;
			buttonSortMode.OnClick += SortClick;
			buttonSortMode.OnRightClick += SortClick;
			panelMain.Append(buttonSortMode);

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
		}

		public void UpdateItems()
		{
			for (int i = 0; i < gridMobs.Count; i++) gridMobs.items[i].visible = gridMobs.items[i].PassFilters();
			gridMobs.Recalculate();
		}

		public void QueryMobs()
		{
			string pattern = inputMobs.GetText().ToLower();
			try
			{
				regex = new Regex(pattern);
				panelInputMobs.BorderColor = Color.Black;

				UpdateItems();
			}
			catch (ArgumentException)
			{
				panelInputMobs.BorderColor = Color.Red;
			}
		}

		public void InitCategories()
		{
			categories.Add("Boss", npc => npc.boss);
			categories.Add("Town NPC", npc => npc.townNPC);
			categories.Add("Hostile", npc => !npc.friendly);
			categories.Add("Friendly", npc => npc.friendly);
		}

		public void InitSortModes()
		{
			sortModes.Add("Type Ascending", (npc1, npc2) => npc1.type.CompareTo(npc2.type));
			sortModes.Add("Type Descending", (npc1, npc2) => -npc1.type.CompareTo(npc2.type));
			sortModes.Add("A->Z", (npc1, npc2) => string.Compare(Lang.GetNPCNameValue(npc1.netID), Lang.GetNPCNameValue(npc2.netID), StringComparison.Ordinal));
			sortModes.Add("Z->A", (npc1, npc2) => -string.Compare(Lang.GetNPCNameValue(npc1.netID), Lang.GetNPCNameValue(npc2.netID), StringComparison.Ordinal));

			sortMode = sortModes.First().Key;
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

		public void PopulateMods()
		{
			UIGrid gridMods = (UIGrid)typeof(UIElement).GetFieldValue<List<UIElement>>("Elements", sidePanels["Mods"]).First(x => x is UIGrid);

			gridMods.Clear();
			foreach (Mod mod in ModLoader.GetLoadedMods().Select(ModLoader.GetMod).Where(x => !x.Name.StartsWith("ModLoader") && typeof(Mod).GetFieldValue<Dictionary<string, ModNPC>>("npcs", x).Count > 0))
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
			foreach (KeyValuePair<string, Func<NPC, bool>> categoryName in categories)
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

			gridMobs.UpdateOrder();
			gridMobs.RecalculateChildren();
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

			if (panelMain.ContainsPoint(Main.MouseScreen))
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);
			}

			if (dragging)
			{
				panelMain.Left.Set(MathHelper.Clamp(Main.MouseScreen.X - offset.X, 0, Main.screenWidth - dimensions.Width), 0f);
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

				CalculatedStyle dimensionsGrid = gridMobs.GetDimensions();
				gridMobs.columns = (int)Math.Round(dimensionsGrid.Width / 44);

				RecalculatePanels();
			}
		}
		#endregion
	}
}
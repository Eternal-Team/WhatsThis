using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TheOneLibrary.Base.UI;
using TheOneLibrary.UI.Elements;
using TheOneLibrary.Utility;

namespace WhatsThis.UI
{
	public class BrowserUI : BaseUI
	{
		public UIPanel panelItems = new UIPanel();

		public UIPanel panelInputItems = new UIPanel();
		public UITextInput inputItems = new UITextInput("Select to type");
		public UICycleButton buttonSortMode = new UICycleButton(WhatsThis.textureSortMode);

		public UIBrowserGrid gridItems = new UIBrowserGrid();
		public UIScrollbar barItems = new UIScrollbar();

		public UITextButton buttonMode = new UITextButton("C");

		public UITextButton buttonMods = new UITextButton("Mods");
		public UITextButton buttonCategories = new UITextButton("Category");

		public UIPanel panelMods = new UIPanel();
		public UIGrid gridMods = new UIGrid();
		public UIScrollbar barMods = new UIScrollbar();

		public UIPanel panelCategories = new UIPanel();
		public UIGrid gridCategories = new UIGrid();
		public UIScrollbar barCategories = new UIScrollbar();

		//public Regex regex;
		//public bool caseSensitive;
		//public bool searchName = true;

		public bool cheatMode = true;

		//public SortMode sortMode = SortMode.TypeAsc;
		//public List<Mod> currentMods = new List<Mod>();

		//public string[] allCategories = { "Weapons", "Tools", "Armors", "Accessories", "Placeables", "Ammo", "Consumables", "Expert", "Fishing", "Mounts" };
		//public List<string> currentCategories = new List<string>();

		public override void OnInitialize()
		{
			panelMain.Width.Pixels = 552;
			panelMain.MinWidth.Set(284, 0); // Min 3 columns
			panelMain.MaxWidth.Set(900, 0); // Max 17 columns 
			panelMain.Height.Pixels = 648;
			panelMain.MinHeight.Set(208, 0); // Min 3 rows
			panelMain.MaxHeight.Set(904, 0); // Max 19 rows
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
			buttonSortMode.HoverText = "Type ascending";
			buttonSortMode.OnClick += (evt, element) => SortIcons();
			buttonSortMode.OnRightClick += (evt, element) => SortIcons(true);
			panelMain.Append(buttonSortMode);

			buttonMode.Width.Pixels = 40;
			buttonMode.Height.Pixels = 40;
			buttonMode.HAlign = 1;
			buttonMode.Left.Pixels = -8;
			buttonMode.Top.Pixels = 8;
			buttonMode.OnClick += (a, b) =>
			{
				//doesn't work
				cheatMode = !cheatMode;
				buttonMode.uiText.SetText(cheatMode ? "C" : "R");
			};
			panelMain.Append(buttonMode);
			#endregion

			#region Items
			panelItems.Width.Set(-112, 1); // + 44
			panelItems.Height.Set(-64, 1); // 16
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
			gridItems.SetScrollbar(barItems);
			panelItems.Append(barItems);
			#endregion

			//buttonMods.Width.Pixels = 88;
			//buttonMods.Height.Pixels = 40;
			//buttonMods.Left.Pixels = 8;
			//buttonMods.Top.Pixels = 56;
			//buttonMods.OnClick += OpenMods;
			//panelMain.Append(buttonMods);

			//buttonCategories.Width.Pixels = 88;
			//buttonCategories.Height.Pixels = 40;
			//buttonCategories.Left.Pixels = 8;
			//buttonCategories.Top.Pixels = 104;
			//buttonCategories.OnClick += OpenCategories;
			//panelMain.Append(buttonCategories);

			//panelMods.Width.Pixels = ModLoader.GetLoadedMods().Select(x => Main.fontMouseText.MeasureString(ModLoader.GetMod(x).DisplayName).X).OrderByDescending(x => x).First() + 28;
			//panelMods.Height.Set(-56, 0.5f);
			//panelMods.Left.Pixels = panelMain.GetDimensions().X - panelMods.Width.Pixels + 2;
			//panelMods.Top.Pixels = panelMain.GetDimensions().Y + 56;
			//panelMods.SetPadding(0);

			//gridMods.Width.Set(-16, 1);
			//gridMods.Height.Set(-16, 1);
			//gridMods.Left.Pixels = 8;
			//gridMods.Top.Pixels = 8;
			//gridMods.ListPadding = 4;
			//gridMods.OverflowHidden = true;
			//panelMods.Append(gridMods);

			//barMods.SetView(100f, 1000f);
			//gridMods.SetScrollbar(barMods);

			//InitMods();

			//panelCategories.Width.Pixels = allCategories.Select(x => Main.fontMouseText.MeasureString(x).X).OrderByDescending(x => x).First() + 28;
			//panelCategories.Height.Set(-56, 0.5f);
			//panelCategories.Left.Pixels = panelMain.GetDimensions().X - panelCategories.Width.Pixels + 2;
			//panelCategories.Top.Pixels = panelMain.GetDimensions().Y + 56;
			//panelCategories.SetPadding(0);

			//gridCategories.Width.Set(-16, 1);
			//gridCategories.Height.Set(-16, 1);
			//gridCategories.Left.Pixels = 8;
			//gridCategories.Top.Pixels = 8;
			//gridCategories.ListPadding = 4;
			//gridCategories.OverflowHidden = true;
			//panelCategories.Append(gridCategories);

			//barCategories.SetView(100f, 1000f);
			//gridCategories.SetScrollbar(barCategories);

			//InitCategories();
		}

		public void InitMods()
		{
			//foreach (string modName in ModLoader.GetLoadedMods())
			//{
			//	UIModItem mod = new UIModItem(ModLoader.GetMod(modName));
			//	mod.Width.Precent = 1;
			//	mod.Height.Pixels = 40;
			//	mod.OnClick += (e, element) =>
			//	{
			//		if (currentMods.Contains(mod.mod))
			//		{
			//			mod.SetInactive();
			//			currentMods.Remove(mod.mod);

			//			ScanItems();

			//			if (currentMods.Count == 0) foreach (UIElement item in gridMods.items) (item as UIModItem)?.SetActive();
			//		}
			//		else
			//		{
			//			if (currentMods.Count == 0)
			//			{
			//				currentMods.Clear();
			//				foreach (UIElement item in gridMods.items) (item as UIModItem)?.SetInactive();
			//			}

			//			mod.SetActive();
			//			currentMods.Add(mod.mod);

			//			ScanItems();
			//		}
			//	};

			//	if (currentMods.Contains(mod.mod) || currentMods.Count == 0) mod.SetActive();
			//	else mod.SetInactive();
			//	gridMods.Add(mod);
			//	mod.OnInitialize();
			//}
		}

		public void InitCategories()
		{
			//    foreach (string categoryName in allCategories)
			//    {
			//        UICategoryItem category = new UICategoryItem(categoryName);
			//        category.Width.Precent = 1;
			//        category.Height.Pixels = 40;
			//        category.OnClick += (e, element) =>
			//        {
			//            if (currentCategories.Contains(category.category))
			//            {
			//                category.SetInactive();
			//                currentCategories.Remove(category.category);

			//                ScanItems();

			//                if (currentCategories.Count == 0) foreach (UIElement item in gridCategories.items) (item as UICategoryItem)?.SetActive();
			//            }
			//            else
			//            {
			//                if (currentCategories.Count == 0)
			//                {
			//                    currentCategories.Clear();
			//                    foreach (UIElement item in gridCategories.items) (item as UICategoryItem)?.SetInactive();
			//                }

			//                category.SetActive();
			//                currentCategories.Add(category.category);
			//                ScanItems();
			//            }
			//        };

			//        if (currentCategories.Contains(category.category) || currentCategories.Count == 0) category.SetActive();
			//        else category.SetInactive();
			//        gridCategories.Add(category);
			//        category.OnInitialize();
			//    }
		}

		private void OpenMods(UIMouseEvent evt, UIElement listeningElement)
		{
			//if (HasChild(panelCategories)) RemoveChild(panelCategories);

			//if (HasChild(panelMods)) RemoveChild(panelMods);
			//else Append(panelMods);
		}

		private void OpenCategories(UIMouseEvent evt, UIElement listeningElement)
		{
			//if (HasChild(panelMods)) RemoveChild(panelMods);

			//if (HasChild(panelCategories)) RemoveChild(panelCategories);
			//else Append(panelCategories);
		}

		private void SortIcons(bool rightClick = false)
		{
			//sortMode = rightClick ? sortMode.PreviousEnum() : sortMode.NextEnum();
			//buttonSortMode.HoverText = Utility.ToString(sortMode);

			//gridItems.UpdateOrder();
			//gridItems.RecalculateChildren();
		}

		public void ScanItems()
		{
			//for (int i = 0; i < gridItems.Count; i++) gridItems.items[i].visible = gridItems.items[i].PassFilters();
			//gridItems.Recalculate();
			//gridItems.RecalculateChildren();
		}

		public void QueryItems()
		{
			//string pattern = caseSensitive ? inputItems.GetText() : inputItems.GetText().ToLower();

			//if (pattern.StartsWith("#"))
			//{
			//	pattern = pattern.TrimStart('#');
			//	searchName = false;
			//}
			//else searchName = true;

			//try
			//{
			//	regex = new Regex(pattern);
			//}
			//catch (ArgumentException)
			//{
			//	regex = null;
			//}

			//if (regex != null)
			//{
			//	//buttonRegexValid.opacityActive = 0.5f;
			//	//buttonRegexValid.HoverText = "Regex is valid";
			//	panelInputItems.BorderColor = Color.Black;
			//	for (int i = 0; i < gridItems.Count; i++) gridItems.items[i].visible = gridItems.items[i].PassFilters();
			//	gridItems.Recalculate();
			//	gridItems.RecalculateChildren();
			//}
			//else
			//{
			//	panelInputItems.BorderColor = Color.Red;
			//}
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

		public void QueryContainers()
		{
			//if (!inputItems.focused)
			//{
			//	WhatsThis.Instance.foundContainers.Clear();

			//	Mod ContainerLib = ModLoader.GetMod("ContainerLib2");

			//	if (ContainerLib != null)
			//	{
			//		foreach (Point16 position in TileEntity.ByPosition.Keys)
			//		{
			//			if ((bool)ContainerLib.Call("IsContainer", position) && ((List<Item>)ContainerLib.Call("GetInventory", position)).Any(x => x.type == Main.HoverItem.type)) WhatsThis.Instance.foundContainers.Add(position);
			//		}

			//		for (int i = 0; i < Main.chest.Length; i++)
			//		{
			//			if (Main.chest[i] != null && Main.chest[i].item.Any(x => x.type == Main.HoverItem.type)) WhatsThis.Instance.foundContainers.Add(new Point16(Main.chest[i].x, Main.chest[i].y));
			//		}

			//		if (WhatsThis.Instance.foundContainers.Any()) WhatsThis.Instance.timer = 300;
			//	}
			//}
		}

		#region Dragging & Resizing
		public bool resizing;
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
				panelMain.Left.Set(Main.MouseScreen.X - offset.X, 0f);
				panelMain.Top.Set(Main.MouseScreen.Y - offset.Y, 0f);

				panelMain.Recalculate();
			}
			if (resizing)
			{
				panelMain.Width.Set((Main.MouseScreen.X - dimensions.X - offset.X - 156).ToNearest(44) + 156, 0);
				panelMain.Height.Set((Main.MouseScreen.Y - dimensions.Y - offset.Y - 80).ToNearest(44) + 80, 0);

				CalculatedStyle dimensionsGrid = gridItems.GetDimensions();
				gridItems.columns = (int)System.Math.Round(dimensionsGrid.Width / 44);

				//barItems.Top.Precent = 8 / dimensionsGrid.Height;
				//barItems.Height.Precent = (dimensionsGrid.Height - 32) / dimensionsGrid.Height;

				panelMain.Recalculate();
				panelMain.RecalculateChildren();
			}

			base.DrawSelf(spriteBatch);
		}
		#endregion
	}
}
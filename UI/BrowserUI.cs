using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
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

		public UIBrowserGrid gridItems = new UIBrowserGrid();
		public UIScrollbar barItems = new UIScrollbar();

		public UIPanel panelInputItems = new UIPanel();
		public UITextInput inputItems = new UITextInput("Select to type");
		public UIButton buttonRegexValid = new UIButton(TextureManager.Load("Images/CoolDown"));

		public UICycleButton buttonCaseSensitivity = new UICycleButton(ModLoader.GetTexture("WhatsThis/Textures/CaseSensitive"), 32, 32);
		public UICycleButton buttonSortMode = new UICycleButton(ModLoader.GetTexture("WhatsThis/Textures/SortMode"), 40, 40);
		public UITextButton buttonMods = new UITextButton("Mods");
		public UITextButton buttonCategories = new UITextButton("Category");

		public UIPanel panelMods = new UIPanel();
		public UIGrid gridMods = new UIGrid();
		public UIScrollbar barMods = new UIScrollbar();

		public UIPanel panelCategories = new UIPanel();
		public UIGrid gridCategories = new UIGrid();
		public UIScrollbar barCategories = new UIScrollbar();

		public UITextButton buttonMode = new UITextButton("Recipe Mode");

		public Regex regex;
		public bool caseSensitive;
		public bool searchName = true;

		public bool recipeLock;
		public bool usageLock;
		public bool findLock;

		public bool cheatMode = true;

		public SortMode sortMode = SortMode.TypeAsc;
		public List<Mod> currentMods = new List<Mod>();

		public string[] allCategories = { "Weapons", "Tools", "Armors", "Accessories", "Placeables", "Ammo", "Consumables", "Expert", "Fishing", "Mounts" };
		public List<string> currentCategories = new List<string>();

		public override void OnInitialize()
		{
			panelMain.Width.Pixels = 552;
			panelMain.Height.Precent = 0.5f;
			panelMain.Center();
			panelMain.SetPadding(0);
			panelMain.BackgroundColor = panelColor;
			panelMain.OnMouseDown += DragStart;
			panelMain.OnMouseUp += DragEnd;
			Append(panelMain);

			buttonMode.Width.Pixels = 88;
			buttonMode.Height.Pixels = 40;
			buttonMode.Left.Pixels = 8;
			buttonMode.Top.Set(-48, 1);
			buttonMode.OnClick += (a, b) =>
			{
				cheatMode = !cheatMode;
				buttonMode.Text = cheatMode ? "Cheat Mode" : "Recipe Mode";
				buttonMode.OnInitialize();
			};
			panelMain.Append(buttonMode);

			#region Sorting
			buttonCaseSensitivity.Width.Pixels = 40;
			buttonCaseSensitivity.Height.Pixels = 40;
			buttonCaseSensitivity.Left.Pixels = 8;
			buttonCaseSensitivity.Top.Pixels = 8;
			buttonCaseSensitivity.SetFrame(1);
			buttonCaseSensitivity.Text = "Ignoring case";
			buttonCaseSensitivity.OnClick += (evt, element) =>
			{
				caseSensitive = !caseSensitive;
				buttonCaseSensitivity.Text = caseSensitive ? "Following case" : "Ignoring case";
				if (caseSensitive) buttonCaseSensitivity.SetFrame();
				else buttonCaseSensitivity.SetFrame(1);
			};
			panelMain.Append(buttonCaseSensitivity);

			buttonSortMode.Width.Pixels = 40;
			buttonSortMode.Height.Pixels = 40;
			buttonSortMode.Left.Pixels = 56;
			buttonSortMode.Top.Pixels = 8;
			buttonSortMode.SetFrame();
			buttonSortMode.Text = "Type ascending";
			buttonSortMode.OnClick += (evt, element) => SortIcons();
			buttonSortMode.OnRightClick += (evt, element) => SortIcons(true);
			panelMain.Append(buttonSortMode);

			buttonMods.Width.Pixels = 88;
			buttonMods.Height.Pixels = 40;
			buttonMods.Left.Pixels = 8;
			buttonMods.Top.Pixels = 56;
			buttonMods.OnClick += OpenMods;
			panelMain.Append(buttonMods);

			buttonCategories.Width.Pixels = 88;
			buttonCategories.Height.Pixels = 40;
			buttonCategories.Left.Pixels = 8;
			buttonCategories.Top.Pixels = 104;
			buttonCategories.OnClick += OpenCategories;
			panelMain.Append(buttonCategories);
			#endregion

			#region Panel Mods
			panelMods.Width.Pixels = ModLoader.GetLoadedMods().Select(x => Main.fontMouseText.MeasureString(ModLoader.GetMod(x).DisplayName).X).OrderByDescending(x => x).First() + 28;
			panelMods.Height.Set(-56, 0.5f);
			panelMods.Left.Pixels = panelMain.GetDimensions().X - panelMods.Width.Pixels + 2;
			panelMods.Top.Pixels = panelMain.GetDimensions().Y + 56;
			panelMods.SetPadding(0);

			gridMods.Width.Set(-16, 1);
			gridMods.Height.Set(-16, 1);
			gridMods.Left.Pixels = 8;
			gridMods.Top.Pixels = 8;
			gridMods.ListPadding = 4;
			gridMods.OverflowHidden = true;
			panelMods.Append(gridMods);

			barMods.SetView(100f, 1000f);
			gridMods.SetScrollbar(barMods);

			InitMods();
			#endregion

			#region Panel Category
			panelCategories.Width.Pixels = allCategories.Select(x => Main.fontMouseText.MeasureString(x).X).OrderByDescending(x => x).First() + 28;
			panelCategories.Height.Set(-56, 0.5f);
			panelCategories.Left.Pixels = panelMain.GetDimensions().X - panelCategories.Width.Pixels + 2;
			panelCategories.Top.Pixels = panelMain.GetDimensions().Y + 56;
			panelCategories.SetPadding(0);

			gridCategories.Width.Set(-16, 1);
			gridCategories.Height.Set(-16, 1);
			gridCategories.Left.Pixels = 8;
			gridCategories.Top.Pixels = 8;
			gridCategories.ListPadding = 4;
			gridCategories.OverflowHidden = true;
			panelCategories.Append(gridCategories);

			barCategories.SetView(100f, 1000f);
			gridCategories.SetScrollbar(barCategories);

			InitCategories();
			#endregion

			#region Items
			panelInputItems.Width.Set(-112, 1);
			panelInputItems.Height.Pixels = 40;
			panelInputItems.Left.Pixels = 104;
			panelInputItems.Top.Pixels = 8;
			panelMain.Append(panelInputItems);

			inputItems.Width.Precent = 1;
			inputItems.Height.Precent = 1;
			inputItems.OnTextChange += QueryItems;
			panelInputItems.Append(inputItems);

			buttonRegexValid.Width.Pixels = 16;
			buttonRegexValid.Height.Pixels = 16;
			buttonRegexValid.Left.Set(-8, 1);
			buttonRegexValid.VAlign = 0.5f;
			buttonRegexValid.opacityActive = 0.5f;
			buttonRegexValid.Text = "Regex is valid";
			inputItems.Append(buttonRegexValid);

			panelItems.Width.Set(-112, 1);
			panelItems.Height.Set(-64, 1);
			panelItems.Left.Pixels = 104;
			panelItems.Top.Pixels = 56;
			panelItems.SetPadding(0);
			panelMain.Append(panelItems);

			gridItems.Width.Set(-44, 1);
			gridItems.Height.Set(-16, 1);
			gridItems.Left.Pixels = 8;
			gridItems.Top.Pixels = 8;
			gridItems.ListPadding = 4;
			gridItems.OverflowHidden = true;
			panelItems.Append(gridItems);

			CalculatedStyle dimensions = gridItems.GetDimensions();
			gridItems.columns = (int)(dimensions.Width / 44);

			dimensions = panelItems.GetDimensions();
			barItems.Height.Precent = (dimensions.Height - 32) / dimensions.Height;
			barItems.Left.Set(-28, 1);
			barItems.Top.Precent = 16f / dimensions.Height;
			barItems.SetView(100f, 1000f);
			gridItems.SetScrollbar(barItems);
			panelItems.Append(barItems);
			#endregion
		}

		public void InitMods()
		{
			foreach (string modName in ModLoader.GetLoadedMods())
			{
				UIModItem mod = new UIModItem(ModLoader.GetMod(modName));
				mod.Width.Precent = 1;
				mod.Height.Pixels = 40;
				mod.OnClick += (e, element) =>
				{
					if (currentMods.Contains(mod.mod))
					{
						mod.SetInactive();
						currentMods.Remove(mod.mod);

						ScanItems();

						if (currentMods.Count == 0) foreach (UIElement item in gridMods.items) (item as UIModItem)?.SetActive();
					}
					else
					{
						if (currentMods.Count == 0)
						{
							currentMods.Clear();
							foreach (UIElement item in gridMods.items) (item as UIModItem)?.SetInactive();
						}

						mod.SetActive();
						currentMods.Add(mod.mod);

						ScanItems();
					}
				};

				if (currentMods.Contains(mod.mod) || currentMods.Count == 0) mod.SetActive();
				else mod.SetInactive();
				gridMods.Add(mod);
				mod.OnInitialize();
			}
		}

		public void InitCategories()
		{
			foreach (string categoryName in allCategories)
			{
				UICategoryItem category = new UICategoryItem(categoryName);
				category.Width.Precent = 1;
				category.Height.Pixels = 40;
				category.OnClick += (e, element) =>
				{
					if (currentCategories.Contains(category.category))
					{
						category.SetInactive();
						currentCategories.Remove(category.category);

						ScanItems();

						if (currentCategories.Count == 0) foreach (UIElement item in gridCategories.items) (item as UICategoryItem)?.SetActive();
					}
					else
					{
						if (currentCategories.Count == 0)
						{
							currentCategories.Clear();
							foreach (UIElement item in gridCategories.items) (item as UICategoryItem)?.SetInactive();
						}

						category.SetActive();
						currentCategories.Add(category.category);
						ScanItems();
					}
				};

				if (currentCategories.Contains(category.category) || currentCategories.Count == 0) category.SetActive();
				else category.SetInactive();
				gridCategories.Add(category);
				category.OnInitialize();
			}
		}

		private void OpenMods(UIMouseEvent evt, UIElement listeningElement)
		{
			if (HasChild(panelCategories)) RemoveChild(panelCategories);

			if (HasChild(panelMods)) RemoveChild(panelMods);
			else Append(panelMods);
		}

		private void OpenCategories(UIMouseEvent evt, UIElement listeningElement)
		{
			if (HasChild(panelMods)) RemoveChild(panelMods);

			if (HasChild(panelCategories)) RemoveChild(panelCategories);
			else Append(panelCategories);
		}

		private void SortIcons(bool rightClick = false)
		{
			sortMode = rightClick ? sortMode.PreviousEnum() : sortMode.NextEnum();
			buttonSortMode.Text = Utility.ToString(sortMode);
			switch (sortMode)
			{
				case SortMode.AZ: buttonSortMode.SetFrame(2); break;
				case SortMode.ZA: buttonSortMode.SetFrame(3); break;
				case SortMode.TypeAsc: buttonSortMode.SetFrame(); break;
				case SortMode.TypeDesc: buttonSortMode.SetFrame(1); break;
				default: buttonSortMode.SetFrame(); break;
			}

			gridItems.UpdateOrder();
			gridItems.innerList.Recalculate();
		}

		public override void Load()
		{
			Main.LocalPlayer.showItemIcon = false;
			Main.ItemIconCacheUpdate(0);

			Main.blockInput = true;
			Main.editSign = true;
			Main.chatRelease = false;
		}

		public override void Unload()
		{
			Main.blockInput = false;
			Main.editSign = false;
			Main.chatRelease = true;
		}

		public void ScanItems()
		{
			for (int i = 0; i < gridItems.Count; i++) gridItems.items[i].visible = gridItems.items[i].PassFilters();
			gridItems.Recalculate();
			gridItems.RecalculateChildren();
		}

		public void QueryItems()
		{
			string pattern = caseSensitive ? inputItems.GetText() : inputItems.GetText().ToLower();

			if (pattern.StartsWith("#"))
			{
				pattern = pattern.TrimStart('#');
				searchName = false;
			}
			else searchName = true;

			try
			{
				regex = new Regex(pattern);
			}
			catch (ArgumentException)
			{
				regex = null;
			}

			if (regex != null)
			{
				buttonRegexValid.opacityActive = 0.5f;
				buttonRegexValid.Text = "Regex is valid";
				for (int i = 0; i < gridItems.Count; i++) gridItems.items[i].visible = gridItems.items[i].PassFilters();
				gridItems.Recalculate();
				gridItems.RecalculateChildren();
			}
			else
			{
				buttonRegexValid.opacityActive = 1f;
				buttonRegexValid.Text = "Regex is not valid";
			}
		}

		public void DisplayRecipe()
		{
			if (!inputItems.focused)
			{
				recipeLock = true;
			}
		}

		public void DisplayUsage()
		{
			if (!inputItems.focused)
			{
				usageLock = true;
			}
		}

		public void QueryContainers()
		{
			if (!inputItems.focused)
			{
				WhatsThis.Instance.foundContainers.Clear();

				Mod ContainerLib = ModLoader.GetMod("ContainerLib2");

				if (ContainerLib != null)
				{
					foreach (Point16 position in TileEntity.ByPosition.Keys)
					{
						if ((bool)ContainerLib.Call("IsContainer", position) && ((List<Item>)ContainerLib.Call("GetInventory", position)).Any(x => x.type == Main.HoverItem.type)) WhatsThis.Instance.foundContainers.Add(position);
					}

					for (int i = 0; i < Main.chest.Length; i++)
					{
						if (Main.chest[i] != null && Main.chest[i].item.Any(x => x.type == Main.HoverItem.type)) WhatsThis.Instance.foundContainers.Add(new Point16(Main.chest[i].x, Main.chest[i].y));
					}

					if (WhatsThis.Instance.foundContainers.Any()) WhatsThis.Instance.timer = 300;
				}

				findLock = true;
			}
		}
	}
}
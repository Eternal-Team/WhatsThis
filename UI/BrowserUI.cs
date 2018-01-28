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
using TheOneLibrary.Base;
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

        public UITextButton buttonMods = new UITextButton("Mods");
        public UITextButton buttonCategories = new UITextButton("Category");

        public UIPanel panelMods = new UIPanel();
        public UIGrid gridMods = new UIGrid();
        public UIScrollbar barMods = new UIScrollbar();

        public UIPanel panelCategories = new UIPanel();
        public UIGrid gridCategories = new UIGrid();
        public UIScrollbar barCategories = new UIScrollbar();

        public UITextButton buttonMode = new UITextButton("Cheat");

        public Regex regex;
        public bool cheatMode = true;

        public string sortMode;

        [Null] public static Dictionary<string, Func<Item, bool>> categories = new Dictionary<string, Func<Item, bool>>();
        [Null] public static Dictionary<string, Func<Item, Item, int>> sortModes = new Dictionary<string, Func<Item, Item, int>>();

        public List<string> currentMods = new List<string>();
        public List<string> currentCategories = new List<string>();

        public override void OnInitialize()
        {
            panelMain.Width.Pixels = 552;
            panelMain.MinWidth.Set(284, 0); // Min 3 columns
            panelMain.MaxWidth.Set(900, 0); // Max 17 columns 
            panelMain.Height.Pixels = 648;
            panelMain.MinHeight.Set(212, 0); // Min 3 rows
            panelMain.MaxHeight.Set(904, 0); // Max 19 rows
            panelMain.Left.Set(100, 0);
            panelMain.Top.Set(100, 0);
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
            gridItems.SetScrollbar(barItems);
            panelItems.Append(barItems);
            #endregion

            #region Right Panel
            buttonCategories.Width.Pixels = 88;
            buttonCategories.Height.Pixels = 40;
            buttonCategories.HAlign = 1;
            buttonCategories.Left.Pixels = -8;
            buttonCategories.Top.Pixels = 56;
            buttonCategories.OnClick += OpenCategories;
            panelMain.Append(buttonCategories);

            buttonMods.Width.Pixels = 88;
            buttonMods.Height.Pixels = 40;
            buttonMods.HAlign = 1;
            buttonMods.Left.Pixels = -8;
            buttonMods.Top.Pixels = 104;
            buttonMods.OnClick += OpenMods;
            panelMain.Append(buttonMods);

            buttonMode.Width.Pixels = 88;
            buttonMode.Height.Pixels = 40;
            buttonMode.HAlign = 1;
            buttonMode.Left.Pixels = -8;
            buttonMode.VAlign = 1;
            buttonMode.Top.Pixels = -8;
            buttonMode.OnClick += (a, b) =>
            {
                cheatMode = !cheatMode;
                buttonMode.uiText.SetText(cheatMode ? "Cheat" : "Recipe");
            };
            panelMain.Append(buttonMode);
            #endregion

            #region Side Panels
            panelCategories.Width.Pixels = categories.Select(x => Main.fontMouseText.MeasureString(x.Key).X).OrderByDescending(x => x).First() + 28;
            panelCategories.Left.Pixels = panelMain.GetDimensions().Width;
            panelCategories.Top.Pixels = 56;
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

            panelMods.Width.Pixels = 300;
            panelMods.Left.Pixels = panelMain.GetDimensions().Width;
            panelMods.Top.Pixels = 104;
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

        public void PopulateMods()
        {
            gridMods.Clear();
            foreach (Mod mod in ModLoader.GetLoadedMods().Select(ModLoader.GetMod).Where(x => !x.Name.StartsWith("ModLoader") && typeof(Mod).GetField<Dictionary<string, ModItem>>("items", x).Count > 0))
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

        private void OpenMods(UIMouseEvent evt, UIElement listeningElement)
        {
            if (HasChild(panelCategories)) RemoveChild(panelCategories);

            if (HasChild(panelMods)) RemoveChild(panelMods);
            else
            {
                Append(panelMods);
                RecalculatePanels();
            }
        }

        private void OpenCategories(UIMouseEvent evt, UIElement listeningElement)
        {
            if (HasChild(panelMods)) RemoveChild(panelMods);

            if (HasChild(panelCategories)) RemoveChild(panelCategories);
            else
            {
                Append(panelCategories);
                RecalculatePanels();
            }
        }

        private void SortClick(UIMouseEvent evt, UIElement element)
        {
            sortMode = sortModes.ElementAt(buttonSortMode.index).Key;
            buttonSortMode.HoverText = sortMode;

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

        public void RecalculatePanels()
        {
            CalculatedStyle dimensions = panelMain.GetDimensions();
            panelMods.Height.Set(dimensions.Height - 104, 0);
            panelMods.Left.Set(dimensions.X + dimensions.Width, 0f);
            panelMods.Top.Set(dimensions.Y + 104, 0f);

            panelMods.Recalculate();
            gridMods.Recalculate();

            panelCategories.Height.Set(dimensions.Height - 56, 0);
            panelCategories.Left.Set(dimensions.X + dimensions.Width, 0f);
            panelCategories.Top.Set(dimensions.Y + 56, 0f);

            panelCategories.Recalculate();
            gridCategories.Recalculate();
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

                CalculatedStyle dimensionsGrid = gridItems.GetDimensions();
                gridItems.columns = (int)Math.Round(dimensionsGrid.Width / 44);

                RecalculatePanels();
            }
        }
        #endregion
    }
}
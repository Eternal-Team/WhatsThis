using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;
using TheOneLibrary.Recipe;
using TheOneLibrary.Utility;
using WhatsThis.UI;

namespace WhatsThis
{
	public class WhatsThis : Mod
	{
		public static WhatsThis Instance;

		public BrowserUI BrowserUI;
		public UserInterface IBrowserUI;

		public RecipeUI RecipeUI;
		public UserInterface IRecipeUI;

		public static ModHotKey OpenBrowser;

		internal int timer;
		internal List<Point16> foundContainers = new List<Point16>();

		private bool browserLock;

		public WhatsThis()
		{
			Properties = new ModProperties
			{
				Autoload = true,
				AutoloadBackgrounds = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}

		public override void Load()
		{
			Instance = this;

			OpenBrowser = RegisterHotKey("Open Browser", Keys.Add.ToString());

			if (!Main.dedServ)
			{
				BrowserUI = new BrowserUI();
				BrowserUI.Activate();
				IBrowserUI = new UserInterface();
				IBrowserUI.SetState(BrowserUI);

				RecipeUI = new RecipeUI();
				RecipeUI.Activate();
				IRecipeUI = new UserInterface();
				IRecipeUI.SetState(RecipeUI);
			}
		}

		public override void PostSetupContent()
		{
			for (int i = 0; i < ItemLoader.ItemCount; i++)
			{
				Item item = new Item();
				item.SetDefaults(i);

				if (!item.IsAir)
				{
					UIBrowserIcon slot = new UIBrowserIcon(i, item);
					BrowserUI.gridItems.Add(slot);
				}
			}
		}

		public override void Unload()
		{
			Instance = null;
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int InventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			int SmartIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Smart Cursor Targets"));

			if (InventoryIndex != -1)
			{
				if (BrowserUI.visible)
				{
					layers.Insert(InventoryIndex, new LegacyGameInterfaceLayer(
					"WhatsThis: Browser",
					delegate
					{
						IBrowserUI.Update(Main._drawInterfaceGameTime);
						BrowserUI.Draw(Main.spriteBatch);

						return true;
					}, InterfaceScaleType.UI));
				}

				if (RecipeUI.visible)
				{
					layers.Insert(InventoryIndex, new LegacyGameInterfaceLayer(
					"WhatsThis: Recipe",
					delegate
					{
						IRecipeUI.Update(Main._drawInterfaceGameTime);
						RecipeUI.Draw(Main.spriteBatch);

						return true;
					}, InterfaceScaleType.UI));
				}
			}

			if (SmartIndex != -1)
			{
				layers.Insert(InventoryIndex, new LegacyGameInterfaceLayer(
					"WhatsThis: ContainerOverlay",
					delegate
					{
						if (--timer > 0)
						{
							foreach (Point16 position in foundContainers)
							{
								Tile tile = Main.tile[position.X, position.Y];
								TileObjectData data = TileObjectData.GetTileData(tile.type, 0);

								if (data != null)
								{
									Main.spriteBatch.DrawOutline(position, position + new Point16(data.Width - 1, data.Height - 1), Color.Goldenrod * (timer / 300f), 4);
								}
							}
						}

						return true;
					}));
			}
		}

		public override void PostUpdateInput()
		{
			if (!Main.HoverItem.IsAir)
			{
				if (Main.keyState.IsKeyDown(Keys.R) && Main.HoverItem.HasRecipes())
				{
					if (!BrowserUI.recipeLock)
					{
						if (BrowserUI.visible)
						{
							BrowserUI.visible = false;
							RecipeUI.wasInBrowser = true;
						}
						if (!RecipeUI.visible) RecipeUI.Toggle();
						RecipeUI.DisplayRecipe(Main.HoverItem);
					}

					BrowserUI.recipeLock = true;
				}
				else BrowserUI.recipeLock = false;
				if (Main.keyState.IsKeyDown(Keys.U) && Main.HoverItem.HasUsages())
				{
					if (!BrowserUI.usageLock)
					{
						if (BrowserUI.visible)
						{
							BrowserUI.visible = false;
							RecipeUI.wasInBrowser = true;
						}
						if (!RecipeUI.visible) RecipeUI.Toggle();
						RecipeUI.DisplayRecipe(Main.HoverItem);
					}
				}
				else BrowserUI.usageLock = false;
				if (Main.keyState.IsKeyDown(Keys.F))
				{
					if (!BrowserUI.findLock) BrowserUI.QueryContainers();
				}
				else BrowserUI.findLock = false;
			}

			//if (Main.keyState.IsKeyDown(Keys.E))
			//{
			//	if (!browserLock)
			//	{
			//		if (RecipeUI.visible && RecipeUI.wasInBrowser)
			//		{
			//			RecipeUI.visible = false;
			//			BrowserUI.visible = true;
			//			RecipeUI.wasInBrowser = false;
			//			browserLock = true;
			//			return;
			//		}

			//		BrowserUI.Toggle();
			//		browserLock = true;
			//	}
			//}
			//else browserLock = false;

			if (RecipeUI != null)
			{
				if (Main.keyState.IsKeyDown(Keys.Back))
				{
					if (!RecipeUI.backLock) RecipeUI.GoBack();
				}
				else RecipeUI.backLock = false;
			}

			if (BrowserUI != null && BrowserUI.visible && Main.keyState.IsKeyDown(Keys.RightControl) && Main.keyState.IsKeyDown(Keys.F)) BrowserUI.inputItems.Focus();
		}

		public override void AddRecipes()
		{
			ItemRecipe recipe = new ItemRecipe();
			recipe.AddIngredient(ItemID.CopperBar, 5);
			recipe.AddIngredient(ItemID.StoneBlock, 100);
			recipe.SetResult(ItemID.Abeemination);
			recipe.AddRecipe();
		}
	}
}
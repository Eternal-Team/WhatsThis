using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;
using TheOneLibrary.Base;
using TheOneLibrary.Recipe;
using TheOneLibrary.Utility;
using WhatsThis.UI;

namespace WhatsThis
{
	public class WhatsThis : Mod
	{
		[Null] public static WhatsThis Instance;

		public const string TexturePath = "WhatsThis/Textures/";

		public BrowserUI BrowserUI;
		public UserInterface IBrowserUI;

		public RecipeUI RecipeUI;
		public UserInterface IRecipeUI;

		[Null] public static ModHotKey OpenBrowser;
		[Null] public static ModHotKey ShowRecipe;
		[Null] public static ModHotKey ShowUsage;
		[Null] public static ModHotKey FindItem;
		[Null] public static ModHotKey PrevRecipe;

		[Null] public static List<Texture2D> textureSortMode;

		internal int timer;
		internal List<Point16> foundContainers = new List<Point16>();

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

			OpenBrowser = this.Register("Open Browser", Keys.OemCloseBrackets);
			ShowRecipe = this.Register("Show Recipe", Keys.R);
			ShowUsage = this.Register("Show Usage", Keys.U);
			FindItem = this.Register("Find Item in containers", Keys.F);
			PrevRecipe = this.Register("Previous recipe", Keys.Back);

			if (!Main.dedServ)
			{
				textureSortMode = new List<Texture2D>();
				for (int i = 0; i < 4; i++) textureSortMode.Add(ModLoader.GetTexture(TexturePath + "SortMode_" + i));

				IBrowserUI = new UserInterface();
				BrowserUI = new BrowserUI();
				BrowserUI.Activate();
				IBrowserUI.SetState(BrowserUI);

				IRecipeUI = new UserInterface();
				RecipeUI = new RecipeUI();
				RecipeUI.Activate();
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
			this.UnloadNullableTypes();

			GC.Collect();
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int InventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			int SmartIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Smart Cursor Targets"));

			if (InventoryIndex != -1)
			{
				if (BrowserUI.visible)
				{
					layers.Insert(InventoryIndex + 1, new LegacyGameInterfaceLayer(
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
				// Postdraw tiles
				//layers.Insert(InventoryIndex, new LegacyGameInterfaceLayer(
				//    "WhatsThis: ContainerOverlay",
				//    delegate
				//    {
				//        if (--timer > 0)
				//        {
				//            foreach (Point16 position in foundContainers)
				//            {
				//                Tile tile = Main.tile[position.X, position.Y];
				//                TileObjectData data = TileObjectData.GetTileData(tile.type, 0);

				//                if (data != null)
				//                {
				//                    Main.spriteBatch.DrawOutline(position, position + new Point16(data.Width - 1, data.Height - 1), Color.Goldenrod * (timer / 300f), 4);
				//                }
				//            }
				//        }

				//        return true;
				//    }));
			}
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
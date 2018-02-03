using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
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

		public static List<Texture2D> textureSortMode = new List<Texture2D>();
		[Null] public static Texture2D texturePause;
		[Null] public static Texture2D texturePlay;
		[Null] public static Texture2D textureEntity;
		[Null] public static Texture2D textureWorld;
		[Null] public static Texture2D textureMagnet;
		[Null] public static Texture2D textureMapDeath;

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
				for (int i = 0; i < 4; i++) textureSortMode.Add(ModLoader.GetTexture(TexturePath + "SortMode_" + i));
				texturePause = ModLoader.GetTexture(TexturePath + "Pause");
				texturePlay = ModLoader.GetTexture(TexturePath + "Play");
				textureEntity = ModLoader.GetTexture(TexturePath + "Entity");
				textureWorld = ModLoader.GetTexture(TexturePath + "World");
				textureMagnet = ModLoader.GetTexture(TexturePath + "Magnet");
				textureMapDeath = TextureManager.Load("Images" + Path.DirectorySeparatorChar + "MapDeath");

				IBrowserUI = new UserInterface();
				BrowserUI = new BrowserUI();

				BrowserUI.InitCategories();
				BrowserUI.InitSortModes();
				BrowserUI.InitCheatButtons();
				BrowserUI.InitPanels();

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
			for (int i = 1; i < ItemLoader.ItemCount; i++)
			{
				Item item = new Item();
				item.SetDefaults(i, false);
				if (item.type == 0) continue;

				UIBrowserIcon slot = new UIBrowserIcon(item);
				BrowserUI.gridItems.Add(slot);
			}

			BrowserUI.PopulateCategories();
			BrowserUI.PopulateMods();
		}

		public override void Unload()
		{
			BrowserUI.categories.Clear();
			BrowserUI.sortModes.Clear();
			BrowserUI.sidePanels.Clear();

			textureSortMode.Clear();

			this.UnloadNullableTypes();

			GC.Collect();
		}

		public override void PreSaveAndQuit()
		{
			BrowserUI.visible = false;
			RecipeUI.visible = false;
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int InventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

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
					layers.Insert(InventoryIndex + 1, new LegacyGameInterfaceLayer(
						"WhatsThis: Recipe",
						delegate
						{
							IRecipeUI.Update(Main._drawInterfaceGameTime);
							RecipeUI.Draw(Main.spriteBatch);

							return true;
						}, InterfaceScaleType.UI));
				}
			}
		}

		public override void PostDrawFullscreenMap(ref string mouseText)
		{
			Main.spriteBatch.DrawString(Main.fontMouseText, "Right Click to teleport", new Vector2(15, Main.screenHeight - 80), Color.White);

			int mapWidth = Main.maxTilesX * 16;
			int mapHeight = Main.maxTilesY * 16;
			Vector2 cursorPosition = new Vector2(Main.mouseX, Main.mouseY);

			cursorPosition.X -= Main.screenWidth / 2f;
			cursorPosition.Y -= Main.screenHeight / 2f;

			Vector2 mapPosition = Main.mapFullscreenPos;
			Vector2 cursorWorldPosition = mapPosition;

			cursorPosition /= 16;
			cursorPosition *= 16 / Main.mapFullscreenScale;

			cursorWorldPosition += cursorPosition;
			cursorWorldPosition *= 16;

			if (Main.mouseRight && Main.keyState.IsKeyUp(Keys.LeftControl))
			{
				Player player = Main.LocalPlayer;

				cursorWorldPosition.Y -= player.height;
				if (cursorWorldPosition.X < 0) cursorWorldPosition.X = 0;
				else if (cursorWorldPosition.X + player.width > mapWidth) cursorWorldPosition.X = mapWidth - player.width;
				if (cursorWorldPosition.Y < 0) cursorWorldPosition.Y = 0;
				else if (cursorWorldPosition.Y + player.height > mapHeight) cursorWorldPosition.Y = mapHeight - player.height;

				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					player.Teleport(cursorWorldPosition, 1);
					player.position = cursorWorldPosition;
					player.velocity = Vector2.Zero;
					player.fallStart = (int)(player.position.Y / 16f);
				}
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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;
using TheOneLibrary.Base.UI;
using TheOneLibrary.Recipe;
using TheOneLibrary.UI.Elements;
using TheOneLibrary.Utils;
using WhatsThis.UI.Elements;

namespace WhatsThis.UI
{
	public class RecipeUI : BaseUI
	{
		public static Stack<ItemRecipe> recipeHistory = new Stack<ItemRecipe>();

		public UIText textTile = new UIText("Player");
		public UITextButton buttonTileBack = new UITextButton("<", 4);
		public UITextButton buttonTileForward = new UITextButton(">", 4);

		public UIPanel panelRecipe = new UIPanel();
		public UIPanel panelIngredients = new UIPanel();
		public UIGrid gridIngredients = new UIGrid(4);
		public UIPanel panelResults = new UIPanel();
		public UIGrid gridResults = new UIGrid(4);
		public UIButton buttonArrow = new UIButton(ModLoader.GetTexture("WhatsThis/Textures/Arrow"));

		public UIText textRecipe = new UIText("0/0");
		public UITextButton buttonRecipeBack = new UITextButton("<", 4);
		public UITextButton buttonRecipeForward = new UITextButton(">", 4);

		public bool wasInBrowser;

		public List<ItemRecipe> allRecipes = new List<ItemRecipe>();
		public List<List<int>> tiles = new List<List<int>>();

		public int currentTileIndex;
		public int currentRecipeIndex;

		public override void OnInitialize()
		{
			panelMain.Width.Precent = 0.25f;
			panelMain.Height.Precent = 0.5f;
			panelMain.Center();
			panelMain.SetPadding(0);
			Append(panelMain);

			#region Heading
			textTile.HAlign = 0.5f;
			textTile.Top.Pixels = 16;
			panelMain.Append(textTile);

			buttonTileBack.Width.Pixels = 32;
			buttonTileBack.Height.Pixels = 32;
			buttonTileBack.Left.Pixels = 8;
			buttonTileBack.Top.Pixels = 8;
			buttonTileBack.OnClick += TileBack;
			panelMain.Append(buttonTileBack);

			buttonTileForward.Width.Pixels = 32;
			buttonTileForward.Height.Pixels = 32;
			buttonTileForward.Left.Set(-40, 1);
			buttonTileForward.Top.Pixels = 8;
			buttonTileForward.OnClick += TileForward;
			panelMain.Append(buttonTileForward);
			#endregion

			panelRecipe.Width.Set(-16, 1);
			panelRecipe.Height.Set(-96, 1);
			panelRecipe.Left.Pixels = 8;
			panelRecipe.Top.Pixels = 48;
			panelRecipe.SetPadding(0);
			panelMain.Append(panelRecipe);

			buttonArrow.Width.Pixels = 40;
			buttonArrow.Height.Pixels = 20;
			buttonArrow.Center();
			panelRecipe.Append(buttonArrow);

			panelIngredients.Width.Set(-36, 0.5f);
			panelIngredients.Height.Set(-16, 1);
			panelIngredients.Left.Pixels = 8;
			panelIngredients.Top.Pixels = 8;
			panelRecipe.Append(panelIngredients);

			gridIngredients.Width.Precent = 1;
			gridIngredients.Height.Precent = 1;
			gridIngredients.ListPadding = 4;
			gridIngredients.OverflowHidden = true;
			panelIngredients.Append(gridIngredients);

			panelResults.Width.Set(-36, 0.5f);
			panelResults.Height.Set(-16, 1);
			panelResults.Left.Set(28, 0.5f);
			panelResults.Top.Pixels = 8;
			panelRecipe.Append(panelResults);

			gridResults.Width.Precent = 1;
			gridResults.Height.Precent = 1;
			gridResults.ListPadding = 4;
			gridResults.OverflowHidden = true;
			panelResults.Append(gridResults);

			textRecipe.HAlign = 0.5f;
			textRecipe.Top.Set(-32, 1);
			panelMain.Append(textRecipe);

			buttonRecipeBack.Width.Pixels = 32;
			buttonRecipeBack.Height.Pixels = 32;
			buttonRecipeBack.Left.Pixels = 8;
			buttonRecipeBack.Top.Set(-40, 1);
			buttonRecipeBack.OnClick += RecipeBack;
			panelMain.Append(buttonRecipeBack);

			buttonRecipeForward.Width.Pixels = 32;
			buttonRecipeForward.Height.Pixels = 32;
			buttonRecipeForward.Left.Set(-40, 1);
			buttonRecipeForward.Top.Set(-40, 1);
			buttonRecipeForward.OnClick += RecipeForward;
			panelMain.Append(buttonRecipeForward);
		}

		private void TileBack(UIMouseEvent evt, UIElement listeningElement)
		{
			if (tiles.Any())
			{
				currentTileIndex--;
				if (currentTileIndex < 0) currentTileIndex = tiles.Count - 1;

				ChangeTile();
			}
		}

		private void TileForward(UIMouseEvent evt, UIElement listeningElement)
		{
			if (tiles.Any())
			{
				currentTileIndex++;
				if (currentTileIndex > tiles.Count - 1) currentTileIndex = 0;

				ChangeTile();
			}
		}

		private void RecipeBack(UIMouseEvent evt, UIElement listeningElement)
		{
			currentRecipeIndex--;
			if (currentRecipeIndex < 0) currentRecipeIndex = allRecipes.Count(x => x.requiredTiles.IsEqual(tiles[currentTileIndex])) - 1;

			PopulateRecipe();
		}

		private void RecipeForward(UIMouseEvent evt, UIElement listeningElement)
		{
			currentRecipeIndex++;
			if (currentRecipeIndex > allRecipes.Count(x => x.requiredTiles.IsEqual(tiles[currentTileIndex])) - 1) currentRecipeIndex = 0;

			PopulateRecipe();
		}

		public void ChangeTile()
		{
			textTile.SetText(tiles.Any() && tiles[currentTileIndex].Any() ? tiles[currentTileIndex].Select(x => Lang._mapLegendCache[MapHelper.TileToLookup(x, 0)].Value).Aggregate((current, next) => current + ", " + next) : "Player");
			currentRecipeIndex = 0;
			PopulateRecipe();
		}

		public void PopulateRecipe()
		{
			gridIngredients.Clear();
			gridResults.Clear();

			ItemRecipe currentRecipe = allRecipes.Any() ? allRecipes.Where(x => x.requiredTiles.IsEqual(tiles[currentTileIndex])).ToList()[currentRecipeIndex] : null;

			if (currentRecipe != null)
			{
				textRecipe.SetText($"{currentRecipeIndex + 1}/{allRecipes.Count(x => x.requiredTiles.IsEqual(tiles[currentTileIndex]))}");

				for (int i = 0; i < currentRecipe.requiredItem.Count; i++)
				{
					UIIngredient ing = new UIIngredient(i, currentRecipe.requiredItem[i]);
					gridIngredients.Add(ing);
				}

				for (int i = 0; i < currentRecipe.createItems.Count; i++)
				{
					UIIngredient ing = new UIIngredient(i, currentRecipe.createItems[i]);
					gridResults.Add(ing);
				}
			}
		}

		public void DisplayRecipe(Item item, bool usage = false)
		{
			tiles.Clear();
			allRecipes.Clear();

			foreach (ItemRecipe recipe in usage ? item.GetUsages() : item.GetRecipes())
			{
				allRecipes.Add(recipe);
				if (!tiles.Any(x => x.IsEqual(recipe.requiredTiles))) tiles.Add(recipe.requiredTiles);
			}

			currentTileIndex = 0;
			currentRecipeIndex = 0;

			ChangeTile();
			PopulateRecipe();

			if (tiles.Any()) recipeHistory.Push(allRecipes.Where(x => x.requiredTiles.IsEqual(tiles[currentTileIndex])).ToList()[currentRecipeIndex]);
		}

		public void GoBack()
		{
		}

		public override void Update(GameTime gameTime)
		{
			if (tiles.Any() && tiles[currentTileIndex].Any())
			{
				Main.LocalPlayer.AdjTiles();
				textTile.TextColor = tiles[currentTileIndex].All(x => Main.LocalPlayer.adjTile[x]) ? Color.Lime : Color.Red;
			}
			else textTile.TextColor = Color.White;
		}
	}
}
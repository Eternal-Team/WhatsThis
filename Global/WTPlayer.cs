﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using TheOneLibrary.Utils;
using WhatsThis.UI;

namespace WhatsThis.Global
{
	public class WTPlayer : ModPlayer
	{
		public List<Vector2> waypoints = new List<Vector2>();

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			BrowserUI BrowserUI = WhatsThis.Instance.BrowserUI;
			RecipeUI RecipeUI = WhatsThis.Instance.RecipeUI;

			if (!Main.HoverItem.IsAir)
			{
				//if (WhatsThis.ShowRecipe.JustPressed && Main.HoverItem.HasRecipes())
				//{
				//	if (BrowserUI.visible)
				//	{
				//		BrowserUI.visible = false;
				//		RecipeUI.wasInBrowser = true;
				//	}
				//	if (!RecipeUI.visible) RecipeUI.Toggle();
				//	RecipeUI.DisplayRecipe(Main.HoverItem);
				//}
				//if (WhatsThis.ShowUsage.JustPressed && Main.HoverItem.HasUsages())
				//{
				//	if (BrowserUI.visible)
				//	{
				//		BrowserUI.visible = false;
				//		RecipeUI.wasInBrowser = true;
				//	}
				//	if (!RecipeUI.visible) RecipeUI.Toggle();
				//	RecipeUI.DisplayRecipe(Main.HoverItem);
				//}
				if (WhatsThis.FindItem.JustPressed && !Keys.RightControl.IsKeyDown())
				{
					mod.GetModWorld<WTWorld>().QueryContainers();
				}
			}

			if (WhatsThis.OpenBrowser.JustPressed)
			{
				//if (RecipeUI.visible && RecipeUI.wasInBrowser)
				//{
				//	RecipeUI.visible = false;
				//	BrowserUI.visible = true;
				//	RecipeUI.wasInBrowser = false;
				//	return;
				//}

				BrowserUI.Toggle();
			}

			//if (RecipeUI != null)
			//{
			//	if (WhatsThis.PrevRecipe.JustPressed)
			//	{
			//		RecipeUI.GoBack();
			//	}
			//}

			//if (BrowserUI != null && BrowserUI.visible && Keys.RightControl.IsKeyDown() && Keys.F.IsKeyDown()) BrowserUI.inputItems.Focus();
		}
	}
}
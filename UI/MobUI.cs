using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TheOneLibrary.Base.UI;
using TheOneLibrary.UI.Elements;
using TheOneLibrary.Utils;

namespace WhatsThis.UI
{
	public class MobUI : BaseUI
	{
		public UIPanel panelInputMobs = new UIPanel();
		public UITextInput inputMobs = new UITextInput("Select to type");

		public UIPanel panelMobs = new UIPanel();
		public UIGridVisibility gridMobs = new UIGridVisibility();
		public UIScrollbar barMobs = new UIScrollbar();

		public Regex regex;
		public bool resizing;

		public override void OnInitialize()
		{
			panelMain.Width.Pixels = 552f;
			panelMain.MinWidth.Set(188f, 0f); // Min 3 columns
			panelMain.MaxWidth.Set(804f, 0f); // Max 17 columns 
			panelMain.Height.Pixels = 648f;
			panelMain.MinHeight.Set(208f, 0f); // Min 5 rows
			panelMain.MaxHeight.Set(912f, 0f); // Max 19 rows
			panelMain.Center();
			panelMain.SetPadding(0);
			panelMain.BackgroundColor = panelColor;
			panelMain.OnMouseDown += DragStart;
			panelMain.OnMouseUp += DragEnd;
			Append(panelMain);

			panelInputMobs.Width.Set(-16, 1);
			panelInputMobs.Height.Pixels = 40;
			panelInputMobs.Left.Pixels = 8;
			panelInputMobs.Top.Pixels = 8;
			panelMain.Append(panelInputMobs);

			inputMobs.Width.Precent = 1;
			inputMobs.Height.Precent = 1;
			inputMobs.OnTextChange += QueryMobs;
			panelInputMobs.Append(inputMobs);

			panelMobs.Width.Set(-16, 1); // + 44
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
		}

		public void UpdateItems()
		{
			for (int i = 0; i < gridMobs.Count; i++) gridMobs.items[i].visible = gridMobs.items[i].PassFilters();
			gridMobs.Recalculate();
			gridMobs.RecalculateChildren();
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
			}
			if (resizing)
			{
				panelMain.Width.Set((Main.MouseScreen.X - dimensions.X - offset.X - 156).ToNearest(44) + 156, 0);
				panelMain.Height.Set((Main.MouseScreen.Y - dimensions.Y - offset.Y - 80).ToNearest(44) + 80, 0);
				panelMain.Recalculate();
				panelMain.RecalculateChildren();

				CalculatedStyle dimensionsGrid = gridMobs.GetDimensions();
				gridMobs.columns = (int)Math.Round(dimensionsGrid.Width / 44);
			}
		}
		#endregion
	}
}
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using TheOneLibrary.Base.UI;
using TheOneLibrary.UI.Elements;
using TheOneLibrary.Utils;

namespace WhatsThis.UI.Elements
{
	public class UICategoryItem : BaseElement
	{
		public string category;

		public Color PanelColor = BaseUI.panelColor;
		public Color TextColor = Color.White;

		public UICategoryItem(string category)
		{
			this.category = category;
		}

		public void SetInactive()
		{
			PanelColor = Color.LightGray * 0.7f;
			TextColor = Color.LightGray;
		}

		public void SetActive()
		{
			PanelColor = BaseUI.panelColor;
			TextColor = Color.White;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();

			spriteBatch.DrawPanel(dimensions, TheOneLibrary.TheOneLibrary.backgroundTexture, PanelColor);
			spriteBatch.DrawPanel(dimensions, TheOneLibrary.TheOneLibrary.borderTexture, Color.Black);

			Vector2 size = category.Measure();
			float scale = Math.Min((dimensions.Width - 16f) / size.X, (dimensions.Height - 16f) / size.Y);

			Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, category, dimensions.X + dimensions.Width / 2f, dimensions.Y + dimensions.Height / 2f, TextColor, Color.Black, new Vector2(size.X / 2f, 10), scale);
		}
	}
}
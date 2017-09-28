using BaseLib.Elements;
using BaseLib.UI;
using BaseLib.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace WhatsThis.UI
{
	public class UICategoryItem : BaseElement
	{
		public string category;

		private UIPanel panel = new UIPanel();
		private UIText uiText = new UIText("Mod");

		public UICategoryItem(string category)
		{
			this.category = category;

			panel.Width.Precent = 1;
			panel.Height.Precent = 1;
			panel.BackgroundColor = BaseUI.panelColor;
			panel.SetPadding(0);
			base.Append(panel);

			uiText.SetText(category);
			uiText.Center();
			panel.Append(uiText);
		}

		public override void OnInitialize()
		{
			CalculatedStyle dimensions = panel.GetDimensions();
			if (dimensions.Width > 0 && dimensions.Height > 0)
			{
				float textScale = Math.Min((dimensions.Width - 12) / Main.fontMouseText.MeasureString(category).X, (dimensions.Height - 12) / Main.fontMouseText.MeasureString(category).Y);
				uiText.SetText(category, textScale, false);
			}
		}

		public void SetInactive()
		{
			panel.BackgroundColor = Color.LightGray * 0.7f;
			uiText.TextColor = Color.LightGray;
		}

		public void SetActive()
		{
			panel.BackgroundColor = BaseUI.panelColor;
			uiText.TextColor = Color.White;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (visible) base.DrawSelf(spriteBatch);
		}
	}
}
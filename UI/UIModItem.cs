using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TheOneLibrary.Base.UI;
using TheOneLibrary.UI.Elements;
using TheOneLibrary.Utility;

namespace WhatsThis.UI
{
	public class UIModItem : BaseElement
	{
		public Mod mod;

		private UIPanel panel = new UIPanel();
		private UIText uiText = new UIText("Mod");

		public UIModItem(Mod mod)
		{
			this.mod = mod;

			panel.Width.Precent = 1;
			panel.Height.Precent = 1;
			panel.BackgroundColor = BaseUI.panelColor;
			panel.SetPadding(0);
			Append(panel);

			uiText.SetText(mod.DisplayName);
			uiText.Center();
			panel.Append(uiText);
		}

		public override void OnInitialize()
		{
			CalculatedStyle dimensions = panel.GetDimensions();
			if (dimensions.Width > 0 && dimensions.Height > 0)
			{
				float textScale = Math.Min((dimensions.Width - 12) / Main.fontMouseText.MeasureString(mod.DisplayName).X, (dimensions.Height - 12) / Main.fontMouseText.MeasureString(mod.DisplayName).Y);
				uiText.SetText(mod.DisplayName, textScale, false);
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
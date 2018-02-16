using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TheOneLibrary.Base.UI;
using TheOneLibrary.UI.Elements;
using TheOneLibrary.Utils;

namespace WhatsThis.UI.Elements
{
	public class UIModItem : BaseElement
	{
		public Mod mod;
		public Texture2D iconTexture;

		public Color textColor = Color.White;
		public Color bgColor = BaseUI.PanelColor;

		public UIModItem(Mod mod)
		{
			this.mod = mod;

			if (mod.File.HasFile("icon.png")) iconTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, new MemoryStream(mod.File.GetFile("icon.png")));
		}

		public void SetInactive()
		{
			bgColor = Color.LightGray * 0.7f;
			textColor = Color.LightGray;
		}

		public void SetActive()
		{
			bgColor = BaseUI.PanelColor;
			textColor = Color.White;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();

			spriteBatch.DrawPanel(dimensions, TheOneLibrary.TheOneLibrary.backgroundTexture, bgColor);
			spriteBatch.DrawPanel(dimensions, TheOneLibrary.TheOneLibrary.borderTexture, Color.Black);

			if (iconTexture != null)
			{
				spriteBatch.Draw(iconTexture, new Rectangle((int)(dimensions.X + 8), (int)(dimensions.Y + 4), (int)(dimensions.Height - 8), (int)(dimensions.Height - 8)), Color.White);
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, mod.DisplayName, dimensions.X + dimensions.Height + 4, dimensions.Y + dimensions.Height / 2f - 10, textColor, Color.Black, Vector2.Zero);
			}
			else Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, mod.DisplayName, dimensions.X + 8, dimensions.Y + dimensions.Height / 2f - 10, textColor, Color.Black, Vector2.Zero);
		}
	}
}
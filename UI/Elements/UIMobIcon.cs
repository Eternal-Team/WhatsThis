using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using TheOneLibrary.UI.Elements;

namespace WhatsThis.UI.Elements
{
	public class UIMobIcon : BaseElement
	{
		public Texture2D backgroundTexture = Main.inventoryBackTexture;

		public NPC npc;

		public UIMobIcon(NPC npc)
		{
			Width.Pixels = 40;
			Height.Pixels = 40;

			this.npc = npc;
		}

		public override void Click(UIMouseEvent evt)
		{
			if (WhatsThis.Instance.MobUI.cheatMode)
			{
				int x = (int)Main.LocalPlayer.Bottom.X;
				int y = (int)Main.LocalPlayer.Bottom.Y - 320;

				NPC.NewNPC(x, y, npc.type); 
			}
			// else display drops in RecipeUI
		}

		public override int CompareTo(object obj) => MobUI.sortModes[WhatsThis.Instance.MobUI.sortMode].Invoke(npc, (obj as UIMobIcon)?.npc);

		public override bool PassFilters()
		{
			bool result = true;

			if (WhatsThis.Instance.MobUI.currentMods.Count > 0) result &= npc.modNPC != null && WhatsThis.Instance.MobUI.currentMods.Contains(npc.modNPC.mod.Name);

			if (WhatsThis.Instance.MobUI.currentCategories.Count > 0) result &= WhatsThis.Instance.MobUI.currentCategories.Any(x => MobUI.categories[x].Invoke(npc));

			if (WhatsThis.Instance.MobUI.inputMobs.GetText().Length > 0) result &= WhatsThis.Instance.MobUI.regex.Matches(Lang.GetNPCNameValue(npc.type).ToLower()).Count > 0;

			return result;
		}

		internal int frameCounter;
		internal int frameTimer;
		private const int frameDelay = 7;

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetInnerDimensions();

			Main.instance.LoadNPC(npc.type);

			Texture2D npcTexture = Main.npcTexture[npc.type];

			spriteBatch.Draw(backgroundTexture, new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

			if (++frameTimer > frameDelay)
			{
				frameCounter++;
				frameTimer = 0;
				if (frameCounter > Main.npcFrameCount[npc.type] - 1) frameCounter = 0;
			}

			Rectangle rectangle = new Rectangle(0, Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] * frameCounter, Main.npcTexture[npc.type].Width, Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type]);

			int width = npcTexture.Width;
			int height = npcTexture.Height / Main.npcFrameCount[npc.type];

			float drawScale = 2f;
			float availableWidth = dimensions.Width - 6;
			if (width * drawScale > availableWidth || height * drawScale > availableWidth)
			{
				if (width > height) drawScale = availableWidth / width;
				else drawScale = availableWidth / height;
			}
			Vector2 drawPosition = dimensions.Position();
			drawPosition.X += dimensions.Width / 2f - width * drawScale / 2f;
			drawPosition.Y += dimensions.Width / 2f - height * drawScale / 2f;

			Color color = npc.color != Color.Transparent ? new Color(npc.color.R, npc.color.G, npc.color.B, 255f) : new Color(1f, 1f, 1f);

			Main.spriteBatch.Draw(npcTexture, drawPosition, rectangle, color, 0, Vector2.Zero, drawScale, SpriteEffects.None, 0);
		}
	}
}
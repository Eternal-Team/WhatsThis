﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using TheOneLibrary.UI.Elements;

namespace WhatsThis.UI.Elements
{
	public class UIIngredient : BaseElement
	{
		public Texture2D backgroundTexture = Main.inventoryBackTexture;

		public Item item;
		public int index;

		public UIIngredient(int index, Item item)
		{
			Width.Pixels = 40;
			Height.Pixels = 40;

			this.index = index;
			this.item = item;
		}

		public override void Click(UIMouseEvent evt)
		{
			//recipe
		}

		public override void RightClick(UIMouseEvent evt)
		{
			//usage
		}

		public override int CompareTo(object obj)
		{
			UIIngredient other = obj as UIIngredient;
			return index.CompareTo(other.index);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetInnerDimensions();

			float scale = Math.Min(dimensions.Width / backgroundTexture.Width, dimensions.Height / backgroundTexture.Height);
			spriteBatch.Draw(backgroundTexture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

			if (!item.IsAir)
			{
				Texture2D itemTexture = Main.itemTexture[item.type];
				Rectangle rect = Main.itemAnimations[item.type] != null ? Main.itemAnimations[item.type].GetFrame(itemTexture) : itemTexture.Frame();
				Color newColor = Color.White;
				float pulseScale = 1f;
				ItemSlot.GetItemLight(ref newColor, ref pulseScale, item);
				int height = rect.Height;
				int width = rect.Width;
				float drawScale = 1f;
				const float availableWidth = 32;
				if (width > availableWidth || height > availableWidth)
				{
					if (width > height) drawScale = availableWidth / width;
					else drawScale = availableWidth / height;
				}
				drawScale *= scale;
				Vector2 vector = backgroundTexture.Size() * scale;
				Vector2 position2 = dimensions.Position() + vector / 2f - rect.Size() * drawScale / 2f;
				Vector2 origin = rect.Size() * (pulseScale / 2f - 0.5f);

				if (ItemLoader.PreDrawInInventory(item, spriteBatch, position2, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale))
				{
					spriteBatch.Draw(itemTexture, position2, rect, item.GetAlpha(newColor), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
					if (item.color != Color.Transparent) spriteBatch.Draw(itemTexture, position2, rect, item.GetColor(Color.White), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
				}
				ItemLoader.PostDrawInInventory(item, spriteBatch, position2, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale);
				if (ItemID.Sets.TrapSigned[item.type]) spriteBatch.Draw(Main.wireTexture, dimensions.Position() + new Vector2(40f, 40f) * scale, new Rectangle(4, 58, 8, 8), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
				if (item.stack > 1) ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, item.stack.ToString(), dimensions.Position() + new Vector2(10f, 26f) * scale, Color.White, 0f, Vector2.Zero, new Vector2(scale), -1f, scale);

				if (IsMouseHovering)
				{
					Main.LocalPlayer.showItemIcon = false;
					Main.ItemIconCacheUpdate(0);
					Main.HoverItem = item.Clone();
					Main.hoverItemName = Main.HoverItem.Name;
				}
			}
		}
	}
}
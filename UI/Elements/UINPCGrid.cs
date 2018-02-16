using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;
using TheOneLibrary.Base.UI;
using TheOneLibrary.UI.Elements;
using TheOneLibrary.Utils;

namespace WhatsThis.UI.Elements
{
	public class UINPCGrid : UIElement
	{
		public delegate bool ElementSearchMethod(BaseElement element);

		private class UIInnerList : UIElement
		{
			public override bool ContainsPoint(Vector2 point) => true;

			protected override void DrawChildren(SpriteBatch spriteBatch)
			{
				Vector2 position = Parent.GetDimensions().Position();
				Vector2 dimensions = new Vector2(Parent.GetDimensions().Width, Parent.GetDimensions().Height);
				foreach (UIElement current in Elements)
				{
					Vector2 position2 = current.GetDimensions().Position();
					Vector2 dimensions2 = new Vector2(current.GetDimensions().Width, current.GetDimensions().Height);
					if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2)) current.Draw(spriteBatch);
				}
			}
		}

		public List<BaseElement> items = new List<BaseElement>();
		protected UIScrollbar scrollbar;
		internal UIElement innerList = new UIInnerList();
		private float innerListHeight;
		public float ListPadding = 5f;

		public int Count => items.Count;

		public int columns = 1;

		public UINPCGrid(int columns = 1)
		{
			this.columns = columns;
			innerList.OverflowHidden = false;
			innerList.Width.Set(0f, 1f);
			innerList.Height.Set(0f, 1f);
			OverflowHidden = true;
			Append(innerList);
		}

		public float GetTotalHeight()
		{
			return innerListHeight;
		}

		public void Goto(ElementSearchMethod searchMethod, bool center = false)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].visible && searchMethod(items[i]))
				{
					scrollbar.ViewPosition = items[i].Top.Pixels;
					if (center) scrollbar.ViewPosition = items[i].Top.Pixels - GetInnerDimensions().Height / 2 + items[i].GetOuterDimensions().Height / 2;
					return;
				}
			}
		}
		
		public virtual void Add(BaseElement item)
		{
			items.Add(item);
			innerList.Append(item);
			UpdateOrder();
			innerList.Recalculate();
		}

		public virtual bool Remove(BaseElement item)
		{
			innerList.RemoveChild(item);
			UpdateOrder();
			return items.Remove(item);
		}

		public virtual void Clear()
		{
			innerList.RemoveAllChildren();
			items.Clear();
		}

		public override void Recalculate()
		{
			base.Recalculate();
			UpdateScrollbar();
		}

		public override void ScrollWheel(UIScrollWheelEvent evt)
		{
			base.ScrollWheel(evt);
			if (scrollbar != null) scrollbar.ViewPosition -= evt.ScrollWheelValue;
		}

		public override void RecalculateChildren()
		{
			float top = 0f;
			float left = 0f;

			base.RecalculateChildren();

			innerList.RemoveAllChildren();

			// this is a bit wonky
			int index = 0;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].visible)
				{
					items[i].Top.Set(top, 0f);
					items[i].Left.Set(left, 0f);
					items[i].Recalculate();
					if (index % columns == columns - 1 && index < items.Count - 1)
					{
						top += items[i].GetOuterDimensions().Height + ListPadding;
						left = 0;
					}
					else left += items[i].GetOuterDimensions().Width + ListPadding;

					innerList.Append(items[i]);
					index++;
				}
			}
			if (items.Count > 0) top += ListPadding + items[0].GetOuterDimensions().Height;
			innerListHeight = top;
		}

		private void UpdateScrollbar()
		{
			scrollbar?.SetView(GetInnerDimensions().Height, innerListHeight);
		}

		public void SetScrollbar(UIScrollbar scrollbar)
		{
			this.scrollbar = scrollbar;
			UpdateScrollbar();
		}

		public void RemoveScrollbar()
		{
			scrollbar = null;
		}

		public void UpdateOrder()
		{
			items.Sort(SortMethod);
			UpdateScrollbar();
		}

		public int SortMethod(UIElement item1, UIElement item2) => item1.CompareTo(item2);

		public override List<SnapPoint> GetSnapPoints()
		{
			List<SnapPoint> list = new List<SnapPoint>();
			SnapPoint item;
			if (GetSnapPoint(out item)) list.Add(item);
			foreach (BaseElement current in items) list.AddRange(current.GetSnapPoints());
			return list;
		}

		private NPC npc;
		private List<Tuple<Texture2D, object, Color>> tooltip;
		public override void MouseOver(UIMouseEvent evt)
		{
			if (evt.Target is UIMobIcon)
			{
				npc = ((UIMobIcon)evt.Target).npc;
				tooltip = GetTooltip(npc).ToList();
			}
		}

		public override void MouseOut(UIMouseEvent evt) => npc = null;

		public IEnumerable<Tuple<Texture2D, object, Color>> GetTooltip(NPC npc)
		{
			yield return Tuple.Create<Texture2D, object, Color>(null, Lang.GetNPCNameValue(npc.type), Color.White);
			yield return Tuple.Create<Texture2D, object, Color>(Main.heartTexture, "Max Health: " + npc.lifeMax, Color.White);
			yield return Tuple.Create<Texture2D, object, Color>(Main.itemTexture[ItemID.CopperShortsword], "Damage: " + npc.damage, Color.White);
			yield return Tuple.Create<Texture2D, object, Color>(Main.extraTexture[58], "Defense: " + npc.defense, Color.White);
			yield return Tuple.Create<Texture2D, object, Color>(Main.itemTexture[ItemID.CobaltShield], "KB Resistance: " + npc.knockBackResist, Color.White);
			yield return Tuple.Create<Texture2D, object, Color>(null, npc.friendly ? "Friendly" : "Hostile", Color.White);
			if (npc.boss) yield return Tuple.Create<Texture2D, object, Color>(null, "Boss", Main.DiscoColor);
			if (npc.townNPC) yield return Tuple.Create<Texture2D, object, Color>(null, "Town NPC", Color.LightBlue);
		}

		public void DrawPanel(SpriteBatch spriteBatch)
		{
			if (npc != null)
			{
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);
				Main.mouseText = true;

				PlayerInput.SetZoom_UI();
				int hackedMouseX = Main.mouseX;
				int hackedMouseY = Main.mouseY;
				PlayerInput.SetZoom_UI();
				PlayerInput.SetZoom_Test();

				int posX = Main.mouseX + 10;
				int posY = Main.mouseY + 10;
				if (hackedMouseX != -1 && hackedMouseY != -1)
				{
					posX = hackedMouseX + 10;
					posY = hackedMouseY + 10;
				}
				if (Main.ThickMouse)
				{
					posX += 6;
					posY += 6;
				}

				Vector2 vector = new Vector2(40 + tooltip.Select(x => x.Item2.ToString().Measure().X).Max(), tooltip.Count * 20 + (tooltip.Count + 1) * 8);

				CalculatedStyle dimensions = GetDimensions();

				if (posX + vector.X + 4f > dimensions.X + dimensions.Width) posX = (int)(dimensions.X + dimensions.Width - vector.X - 4f);
				if (posY + vector.Y + 4f > dimensions.Y + dimensions.Height) posY = (int)(dimensions.Y + dimensions.Height - vector.Y - 4f);

				spriteBatch.DrawPanel(new Rectangle(posX, posY, (int)vector.X, (int)vector.Y), TheOneLibrary.TheOneLibrary.backgroundTexture, BaseUI.PanelColor * 1.3f);
				spriteBatch.DrawPanel(new Rectangle(posX, posY, (int)vector.X, (int)vector.Y), TheOneLibrary.TheOneLibrary.borderTexture, Color.Black);

				for (int i = 0; i < tooltip.Count; i++)
				{
					var tuple = tooltip[i];

					if (tuple.Item1 != null) spriteBatch.Draw(tuple.Item1, new Rectangle(posX + 8, posY + i * 20 + (i + 1) * 8, 16, 16), Color.White);
					Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, tuple.Item2.ToString(), posX + 32, posY + i * 20 + (i + 1) * 8, tuple.Item3, Color.Black, Vector2.Zero);
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.End();

			RasterizerState state = new RasterizerState { ScissorTestEnable = true };

			Rectangle prevRect = spriteBatch.GraphicsDevice.ScissorRectangle;

			spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(GetClippingRectangle(spriteBatch), spriteBatch.GraphicsDevice.ScissorRectangle);

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, state, null, Main.UIScaleMatrix);

			DrawSelf(spriteBatch);
			typeof(UIInnerList).InvokeMethod<object>("DrawChildren", new object[] { spriteBatch }, innerList);
			DrawPanel(spriteBatch);

			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = prevRect;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.UIScaleMatrix);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (scrollbar != null) innerList.Top.Set(-scrollbar.GetValue(), 0f);
			Recalculate();
		}
	}
}
using BaseLib.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace WhatsThis.UI
{
	public class UIBrowserGrid : UIElement
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

		public UIBrowserGrid(int columns = 1)
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

		public override void Update(GameTime gameTime)
		{
			for (int i = 0; i < items.Count; i++) if (items[i].visible) items[i].Update(gameTime);

			base.Update(gameTime);
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

			innerList.Recalculate();
			UpdateScrollbar();
		}

		public override void ScrollWheel(UIScrollWheelEvent evt)
		{
			base.ScrollWheel(evt);
			if (scrollbar != null) scrollbar.ViewPosition -= evt.ScrollWheelValue;
		}

		public override void RecalculateChildren()
		{
			base.RecalculateChildren();
			float top = 0f;
			float left = 0f;

			innerList.RemoveAllChildren();

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
			foreach (UIElement current in items) list.AddRange(current.GetSnapPoints());
			return list;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (scrollbar != null) innerList.Top.Set(-scrollbar.GetValue(), 0f);
			Recalculate();
		}
	}
}
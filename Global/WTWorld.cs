using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TheOneLibrary.Storage;
using TheOneLibrary.Utils;

namespace WhatsThis.Global
{
	public class WTWorld : ModWorld
	{
		internal int timer;
		internal List<Point16> foundContainers = new List<Point16>();

		public override void PostDrawTiles()
		{
			RasterizerState rasterizer = Main.gameMenu || Main.LocalPlayer.gravDir == 1.0 ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			if (--timer > 0)
			{
				foreach (Point16 position in foundContainers)
				{
					Tile tile = Main.tile[position.X, position.Y];
					TileObjectData data = TileObjectData.GetTileData(tile.type, 0);

					if (data != null) Main.spriteBatch.DrawOutline(position, position + new Point16(data.Width - 1, data.Height - 1), Color.Goldenrod * (timer / 300f), 4);
				}
			}

			Main.spriteBatch.End();
		}

		public void QueryContainers()
		{
			foundContainers.Clear();

			foundContainers.AddRange(TileEntity.ByPosition.Where(x => x.Value is IContainerTile && ((IContainerTile)x.Value).GetItems().Any(y => y.type == Main.HoverItem.type)).Select(x => x.Key));
			foundContainers.AddRange(Main.chest.Where(x => x != null && x.item.Any(y => y.type == Main.HoverItem.type)).Select(x => new Point16(x.x, x.y)));

			timer = 300;
		}
	}
}
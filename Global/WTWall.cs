using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using WhatsThis.UI;

namespace WhatsThis.Global
{
	public class WTWall : GlobalWall
	{
		public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
		{
			if (BrowserUI.IlluminationStrength > 0)
			{
				r = MathHelper.Clamp(r + BrowserUI.IlluminationStrength / 100f, 0, 1);
				g = MathHelper.Clamp(g + BrowserUI.IlluminationStrength / 100f, 0, 1);
				b = MathHelper.Clamp(b + BrowserUI.IlluminationStrength / 100f, 0, 1);
			}
		}
	}
}
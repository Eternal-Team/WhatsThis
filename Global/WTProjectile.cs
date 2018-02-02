using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WhatsThis.UI;

namespace WhatsThis.Global
{
	public class WTProjectile : GlobalProjectile
	{
		public static int[] gravestoneProjectiles =
		{
			ProjectileID.Tombstone,
			ProjectileID.GraveMarker,
			ProjectileID.CrossGraveMarker,
			ProjectileID.Headstone,
			ProjectileID.Gravestone,
			ProjectileID.Obelisk,
			ProjectileID.RichGravestone1, ProjectileID.RichGravestone2,ProjectileID.RichGravestone3,
			ProjectileID.RichGravestone4, ProjectileID.RichGravestone5,
		};

		public override bool Autoload(ref string name) => true;

		public override bool PreAI(Projectile projectile)
		{
			if (BrowserUI.GravestonesToggled)
			{
				if (gravestoneProjectiles.Contains(projectile.type))
				{
					projectile.active = false;
					return false;
				}
			}
			return base.PreAI(projectile);
		}
	}
}

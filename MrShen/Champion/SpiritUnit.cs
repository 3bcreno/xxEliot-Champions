using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.Common;
using SharpDX;

namespace MrShen.Champion
{
    internal class SpiritUnit
    {
        public static AIBaseClient SwordUnit
        {
            get
            {
                List<AIMinionClient> xSword = ObjectManager.Get<AIMinionClient>().Where(m => m.IsAlly && m.CharacterData.SkinName.ToLower().Contains("shenspirit")).ToList();
                return xSword.OrderBy(o => o.NetworkId).LastOrDefault();
            }
        }

        public static void Initialize()
        {
            Game.OnUpdate += GameOnOnUpdate;
        }

        public static void HitWithSword()
        {
            if (!MrShen.Champion.PlayerSpells.Q.IsReady())
            {
                return;
            }

            var toPolygon = new Common.Geometry.Rectangle(ObjectManager.Player.Position.To2D(), SwordUnit.Position.To2D(), 50);
            var x = toPolygon.ToPolygon();

            if (HeroManager.Enemies.Find(
                    e =>
                        !e.IsDead &&
                        ObjectManager.Player.Distance(SwordUnit) > ObjectManager.Player.Distance(e.Position) &&
                        x.IsInside(e.Position)) != null)
            {
                MrShen.Champion.PlayerSpells.Q.Cast();
            }
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            HitWithSword();
        }
    }
}

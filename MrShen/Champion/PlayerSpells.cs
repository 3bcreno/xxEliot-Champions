using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.Common;
using SharpDX;

namespace MrShen.Champion
{
    internal static class PlayerSpells
    {
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q, W, E, R;
        public static void Initialize()
        {
            Q = new Spell(SpellSlot.Q, 475f);
            Q.SetTargetted(0.25f, float.MaxValue);
            SpellList.Add(Q);

            W = new Spell(SpellSlot.W);

            E = new Spell(SpellSlot.E, 650f);
            E.SetSkillshot(0f, 50f, float.MaxValue, false, SkillshotType.SkillshotLine);
            SpellList.Add(E);

            R = new Spell(SpellSlot.R);

        }

        public static void CastE(AIBaseClient t)
        {
            if (!E.CanCast(t))
            {
                return;
            }
            var hithere = t.Position + Vector3.Normalize(t.PreviousPosition - ObjectManager.Player.Position) * 60;
            if (hithere.Distance(ObjectManager.Player.Position) < PlayerSpells.E.Range)
            {
                MrShen.Champion.PlayerSpells.E.Cast(hithere);
            }

        }

    }
}

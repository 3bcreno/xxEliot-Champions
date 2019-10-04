using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.Common;
using MrShen.Champion;
using MrShen.Common;

namespace MrShen.Modes
{
    internal static class ModeJungle
    {
        public static EnsoulSharp.Common.Menu LocalMenu { get; private set; }
        public static Spell Q => MrShen.Champion.PlayerSpells.Q;
        public static Spell W => MrShen.Champion.PlayerSpells.W;
        public static Spell E => MrShen.Champion.PlayerSpells.E;
        public static Spell R => MrShen.Champion.PlayerSpells.R;

        public static void Initialize(EnsoulSharp.Common.Menu MenuConfig)
        {
            LocalMenu = new EnsoulSharp.Common.Menu("Jungle", "Jungle");
            {
                LocalMenu.AddItem(
                    new MenuItem("Jungle.UseQ", "Q:").SetValue(false)
                        .SetFontStyle(System.Drawing.FontStyle.Regular, MrShen.Champion.PlayerSpells.Q.MenuColor()));
                LocalMenu.AddItem(
                    new MenuItem("Jungle.UseW", "W:").SetValue(new StringList(new[] { "Off", "On", "Just for Big Mobs" },
                        2)).SetFontStyle(FontStyle.Regular, MrShen.Champion.PlayerSpells.W.MenuColor()));
                LocalMenu.AddItem(
                    new MenuItem("Jungle.UseE", "E:").SetValue(new StringList(new[] { "Off", "On", "Just for Big Mobs" },
                        2)).SetFontStyle(FontStyle.Regular, MrShen.Champion.PlayerSpells.W.MenuColor()));
                LocalMenu.AddItem(new MenuItem("Jungle.Energy", "Min. Energy Percent: ").SetValue(new Slider(50, 100, 0)));
                MenuConfig.AddSubMenu(LocalMenu);
            }

            //Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Game.OnUpdate += GameOnOnUpdate;
        }

        public static void Obj_AI_Hero_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe && sender.IsEnemy && sender is AIMinionClient && args.Target.IsMe)
            {

            }
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (Modes.MenuConfig.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
            {
                return;
            }

            var jungleMobs = MobManager.GetMobs(Q.Range, MobManager.MobTypes.All);

            if (jungleMobs != null)
            {
                if (Q.IsReady() && jungleMobs.IsValidTarget(Q.Range) && LocalMenu.Item("Jungle.UseQ").GetValue<bool>())
                {
                    Q.Cast();
                }

                if (W.IsReady() && LocalMenu.Item("Jungle.UseW").GetValue<StringList>().SelectedIndex != 0)
                {
                    if (MrShen.Champion.SpiritUnit.SwordUnit.Position.Distance(ObjectManager.Player.Position) < 350f &&
                        jungleMobs.Position.Distance(SpiritUnit.SwordUnit.Position) < 450)
                    {
                        W.Cast();
                    }
                    else if (Q.IsReady() && ObjectManager.Player.Distance(jungleMobs) <= Q.Range)
                    {
                        Q.Cast();
                    }
                }

                if (E.IsReady() && LocalMenu.Item("Jungle.UseE").GetValue<StringList>().SelectedIndex != 0)
                {
                    switch (LocalMenu.Item("Jungle.UseE").GetValue<StringList>().SelectedIndex)
                    {
                        case 1:
                            {
                                E.Cast(jungleMobs.Position);
                                break;
                            }
                        case 2:
                            {
                                jungleMobs = MobManager.GetMobs(E.Range, MobManager.MobTypes.BigBoys);
                                if (jungleMobs != null)
                                {
                                    E.Cast(jungleMobs.Position);
                                }
                                break;
                            }
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.Common;
using MrShen.Champion;

namespace MrShen.Modes
{
    using MrShen.Common;
    internal class ModeCombo
    {
        public static EnsoulSharp.Common.Menu LocalMenu { get; private set; }

        public static Spell Q => PlayerSpells.Q;
        public static Spell W => PlayerSpells.W;
        public static Spell E => PlayerSpells.E;
        public static void Initialize(EnsoulSharp.Common.Menu MenuConfig)
        {
            LocalMenu = new EnsoulSharp.Common.Menu("Combo", "Combo");
            {
                LocalMenu.AddItem(new MenuItem("Combo.PassiveControl", "Passive Control:").SetValue(new StringList(new[] { "Off", "On" }, 1)).SetFontStyle(FontStyle.Regular, W.MenuColor()));
                LocalMenu.AddItem(new MenuItem("Combo.UseW", "W:").SetValue(true).SetFontStyle(FontStyle.Regular, W.MenuColor()));
                LocalMenu.AddItem(new MenuItem("Combo.UseE", "E: Auto use if enemy under ally turret").SetValue(new StringList(new[] { "Off", "On" }, 1)).SetFontStyle(FontStyle.Regular, E.MenuColor()));

                MenuConfig.AddSubMenu(LocalMenu);
            }
            Game.OnUpdate += GameOnOnUpdate;

            AIBaseClient.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }


        public static void Obj_AI_Hero_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (MenuConfig.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                return;
            }
            if (sender.Type != GameObjectType.AIHeroClient)
            {
                return;
            }

            if (!sender.IsValid || sender.Team == ObjectManager.Player.Team)
            {
                return;
            }

            if (!sender.IsMe && sender.IsEnemy && sender is AIHeroClient && args.Target.IsMe)
            {
                if (ObjectManager.Player.Position.Distance(SpiritUnit.SwordUnit.Position) < 350 && W.IsReady())
                {
                    W.Cast();
                }
                else if (Q.IsReady() && ObjectManager.Player.HasPassive())
                {
                    Q.Cast();
                }

            }
        }

        private static void GameOnOnUpdate(EventArgs args)
        {


            if (LocalMenu.Item("Combo.UseE").GetValue<StringList>().SelectedIndex == 1 && E.IsReady())
            {
                var t = TargetSelector.GetTarget(E.Range, EnsoulSharp.Common.TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(E.Range) && t.UnderAllyTurret())
                {
                    PlayerSpells.CastE(t);
                }
            }


            if (MenuConfig.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                ExecuteCombo();
            }
        }

        private static void ExecuteCombo()
        {
            var t = TargetSelector.GetTarget(E.Range, EnsoulSharp.Common.TargetSelector.DamageType.Magical);
            if (!t.IsValidTarget(E.Range))
            {
                return;
            }

            if (LocalMenu.Item("Combo.UseE").GetValue<StringList>().SelectedIndex == 1 && t.UnderAllyTurret() && E.IsReady())
            {
                PlayerSpells.CastE(t);
            }

            if (Q.IsReady() && t.IsValidTarget(Q.Range))
            {
                Q.Cast();
            }

            if (E.IsReady())
            {
                PlayerSpells.CastE(t);
            }


            if (t.IsValidTarget(550) && Common.SummonerManager.IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell(Common.SummonerManager.IgniteSlot) == SpellState.Ready &&
                ObjectManager.Player.GetSummonerSpellDamage(t, Damage.DamageSummonerSpell.Ignite) > t.Health)
            {
                ObjectManager.Player.Spellbook.CastSpell(Common.SummonerManager.IgniteSlot, t);
            }

            ExecuteCastItems(t);
        }

        private static void ExecuteCastItems(AIHeroClient t)
        {
            foreach (var item in MrShen.Common.ItemManager.ItemDb)
            {
                if (item.Value.ItemType == MrShen.Common.ItemManager.EnumItemType.AoE && item.Value.TargetingType == MrShen.Common.ItemManager.EnumItemTargettingType.EnemyHero)
                {
                    if (t.IsValidTarget(item.Value.Item.Range) && item.Value.Item.IsReady()) item.Value.Item.Cast();
                }

                if (item.Value.ItemType == MrShen.Common.ItemManager.EnumItemType.Targeted
                    && item.Value.TargetingType == MrShen.Common.ItemManager.EnumItemTargettingType.EnemyHero)
                {
                    if (t.IsValidTarget(item.Value.Item.Range) && item.Value.Item.IsReady()) item.Value.Item.Cast(t);
                }
            }
        }

        private static void ExecuteComboFlashE()
        {

        }
    }
}

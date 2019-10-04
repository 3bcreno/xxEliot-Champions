using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.Common;
using MrShen.Common;

namespace MrShen.Modes
{
    using Properties;

    using SharpDX;
    using SharpDX.Direct3D9;
    internal class ModeSelector
    {
        enum TargetSelect
        {
            Shen,
            EnsoulSharp
        }
        public static Menu LocalMenu;
        public static Font Text;

        private static TargetSelect Selector = LocalMenu.Item("TS").GetValue<StringList>().SelectedIndex == 0 
            ? TargetSelect.Shen 
            : TargetSelect.EnsoulSharp;
        public static void Initialize(EnsoulSharp.Common.Menu mainMenu)
        {
            new Render.Sprite(Resources.selectedchampion, new Vector2())
            {
                PositionUpdate = () => DrawPosition,
                Scale = new Vector2(1f, 1f),
                VisibleCondition = sender => DrawSprite
            }.Add();

            Text = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Malgun Gothic", Height = 21, OutputPrecision = FontPrecision.Default, Weight = FontWeight.Bold, Quality = FontQuality.ClearTypeNatural });
            LocalMenu = new Menu("Target Selector", "AssassinTargetSelector").SetFontStyle(FontStyle.Regular, SharpDX.Color.Cyan);

            var menuTargetSelector = new Menu("Target Selector", "TargetSelector");
            {
                EnsoulSharp.Common.TargetSelector.AddToMenu(menuTargetSelector);
            }
            LocalMenu.AddItem(
                new MenuItem("TS", "Active Target Selector:").SetValue(
                    new StringList(new[] { "Shen Target Selector", "EnsoulSharp Target Selector" })))
                .SetFontStyle(FontStyle.Regular, SharpDX.Color.GreenYellow)
                .ValueChanged += (sender, args) =>
                {
                    LocalMenu.Items.ForEach(
                        i =>
                        {
                            i.Show();
                            switch (args.GetNewValue<StringList>().SelectedIndex)
                            {
                                case 0:
                                    {
                                        if (i.Tag == 22)
                                        {
                                            i.Show(false);
                                        }
                                        break;
                                    }

                                case 1:
                                    {
                                        if (i.Tag == 11 || i.Tag == 12)
                                        {
                                            i.Show(false);
                                        }
                                        break;
                                    }
                            }
                        });
                };
            menuTargetSelector.Items.ForEach(i =>
            {
                LocalMenu.AddItem(i);
                i.SetTag(22);
            });
            LocalMenu.AddItem(new MenuItem("Set", "Target Select Mode:").SetValue(new StringList(new[] { "Single Target Select", "Multi Target Select" }))).SetFontStyle(FontStyle.Regular, SharpDX.Color.LightCoral).SetTag(11);
            LocalMenu.AddItem(new MenuItem("ModeSelector.Range", "Range (Recommend: Max):"))
                .SetValue(new Slider((int)(MrShen.Champion.PlayerSpells.E.Range * 3),
                                        (int)MrShen.Champion.PlayerSpells.E.Range,
                                        (int)(MrShen.Champion.PlayerSpells.Q.Range * 5))).SetTag(11);
            LocalMenu.AddItem(new MenuItem("Targets", "Targets:").SetFontStyle(FontStyle.Regular, SharpDX.Color.Aqua).SetTag(11));
            foreach (AIHeroClient e in HeroManager.Enemies)
            {
                LocalMenu.AddItem(
                    new MenuItem("enemy_" + e.CharacterName, $"{MrShen.Common.DrawHelper.MenuTab}Focus {e.CharacterName}")
                        .SetValue(false)).SetTag(12);

            }

            LocalMenu.AddItem(new MenuItem("Draw.Title", "Drawings").SetFontStyle(FontStyle.Regular, SharpDX.Color.Aqua).SetTag(11));
            LocalMenu.AddItem(new MenuItem("Draw.Range", MrShen.Common.DrawHelper.MenuTab + "ModeSelector.Range").SetValue(new Circle(true, System.Drawing.Color.Gray)).SetTag(11));
            LocalMenu.AddItem(new MenuItem("Draw.Enemy", MrShen.Common.DrawHelper.MenuTab + "ActiveJungle Enemy").SetValue(new Circle(true, System.Drawing.Color.GreenYellow)).SetTag(11));
            LocalMenu.AddItem(new MenuItem("Draw.Status", MrShen.Common.DrawHelper.MenuTab + "Show Enemy:").SetValue(new StringList(new[] { "Off", "Text", "Picture", "Line", "All" }, 0)));

            mainMenu.AddSubMenu(LocalMenu);
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;

            RefreshMenuItemsStatus();
        }
        public static void DrawText(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            var drawEnemy = LocalMenu.Item("Draw.Enemy").GetValue<Circle>();
            if (drawEnemy.Active)
            {
                var t = GetTarget(MrShen.Champion.PlayerSpells.Q.Range, EnsoulSharp.Common.TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    Render.Circle.DrawCircle(t.Position, (float)(t.BoundingRadius * 1.5), drawEnemy.Color);
                }
            }

            if (Selector != TargetSelect.Shen)
            {
                return;
            }

            Circle rangeColor = LocalMenu.Item("Draw.Range").GetValue<Circle>();
            int range = LocalMenu.Item("ModeSelector.Range").GetValue<Slider>().Value;
            if (rangeColor.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, range, rangeColor.Color);
            }

            int drawStatus = LocalMenu.Item("Draw.Status").GetValue<StringList>().SelectedIndex;
            if (drawStatus == 1 || drawStatus == 4)
            {
                foreach (
                    var e in
                        HeroManager.Enemies.Where(
                            e =>
                                e.IsVisible && !e.IsDead && LocalMenu.Item("enemy_" + e.CharacterName) != null &&
                                LocalMenu.Item("enemy_" + e.CharacterName).GetValue<bool>()))
                {
                    DrawText(Text, "1st Priority Target",
                        e.HPBarPosition.X + e.BoundingRadius / 2f - (e.CharacterData.SkinName.Length / 2f) - 27,
                        e.HPBarPosition.Y - 23, SharpDX.Color.Black);

                    DrawText(Text, "1st Priority Target",
                        e.HPBarPosition.X + e.BoundingRadius / 2f - (e.CharacterData.SkinName.Length / 2f) - 29,
                        e.HPBarPosition.Y - 25, SharpDX.Color.IndianRed);
                }
            }

            if (drawStatus == 3 || drawStatus == 4)
            {
                foreach (
                    EnsoulSharp.Common.Geometry.Polygon.Line line in
                        HeroManager.Enemies.Where(
                            e =>
                                e.IsVisible && !e.IsDead && LocalMenu.Item("enemy_" + e.CharacterName) != null &&
                                LocalMenu.Item("enemy_" + e.CharacterName).GetValue<bool>())
                            .Select(
                                e =>
                                    new EnsoulSharp.Common.Geometry.Polygon.Line(ObjectManager.Player.Position,
                                        e.Position,
                                        ObjectManager.Player.Distance(e.Position))))
                {
                    line.Draw(System.Drawing.Color.Wheat, 2);
                }
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Selector != TargetSelect.Shen)
            {
                return;
            }
            if (args.Msg == 0x201)
            {
                foreach (var objAiHero in from hero in HeroManager.Enemies
                                          where
                                              hero.Distance(Game.CursorPosCenter) < 150f && hero != null && hero.IsVisible && !hero.IsDead
                                          orderby
                                              hero.Distance(Game.CursorPosCenter) descending
                                          select
                                              hero)
                {
                    if (objAiHero != null && objAiHero.IsVisible && !objAiHero.IsDead)
                    {
                        int set = MenuConfig.LocalMenu.Item("Set").GetValue<StringList>().SelectedIndex;

                        switch (set)
                        {
                            case 0:
                                {
                                    ClearAssassinList();
                                    MenuConfig.LocalMenu.Item("enemy_" + objAiHero.CharacterName).SetValue(true);
                                    break;
                                }
                            case 1:
                                {
                                    var menuStatus =
                                        MenuConfig.LocalMenu.Item("enemy_" + objAiHero.CharacterName).GetValue<bool>();
                                    MenuConfig.LocalMenu.Item("enemy_" + objAiHero.CharacterName).SetValue(!menuStatus);
                                    break;
                                }
                        }
                    }
                }
            }
        }

        private static void RefreshMenuItemsStatus()
        {
            LocalMenu.Items.ForEach(
               i =>
               {
                   i.Show();
                   switch (Selector)
                   {
                       case TargetSelect.Shen:
                           if (i.Tag == 22)
                           {
                               i.Show(false);
                           }
                           break;
                       case TargetSelect.EnsoulSharp:
                           if (i.Tag == 11)
                           {
                               i.Show(false);
                           }
                           break;
                   }
               });
        }
        public static void ClearAssassinList()
        {
            foreach (AIHeroClient enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.IsEnemy))
            {
                LocalMenu.Item("enemy_" + enemy.CharacterName).SetValue(false);
            }
        }

        private static bool DrawSprite => true;
        private static Vector2 DrawPosition
        {
            get
            {
                var drawStatus = LocalMenu.Item("Draw.Status").GetValue<StringList>().SelectedIndex;
                if (KillableEnemy == null || (drawStatus != 2 && drawStatus != 4)) return new Vector2(0f, 0f);

                return new Vector2(
                    KillableEnemy.HPBarPosition.X + KillableEnemy.BoundingRadius / 2f,
                    KillableEnemy.HPBarPosition.Y - 70);
            }
        }
        public static bool In<T>(T source, params T[] list)
        {
            return list.Equals(source);
        }

        public static bool NotIn<T>(T source, params T[] list)
        {
            return !list.Equals(source);
        }
        public static AIHeroClient GetTarget(float vDefaultRange = 0,
            EnsoulSharp.Common.TargetSelector.DamageType vDefaultDamageType = EnsoulSharp.Common.TargetSelector.DamageType.Physical,
            IEnumerable<AIHeroClient> ignoredChamps = null)
        {

            vDefaultRange = Math.Abs(vDefaultRange) < 0.00001
                ? MrShen.Champion.PlayerSpells.Q.Range
                : LocalMenu.Item("ModeSelector.Range").GetValue<Slider>().Value;

            if (ignoredChamps == null)
            {
                ignoredChamps = new List<AIHeroClient>();
            }

            var vEnemy =
                HeroManager.Enemies.FindAll(hero => ignoredChamps.All(ignored => ignored.NetworkId != hero.NetworkId))
                    .Where(e => e.IsValidTarget(vDefaultRange))
                    .Where(e => LocalMenu.Item("enemy_" + e.CharacterName) != null)
                    .Where(e => LocalMenu.Item("enemy_" + e.CharacterName).GetValue<bool>())
                    .Where(e => ObjectManager.Player.Distance(e) < vDefaultRange);

            if (LocalMenu.Item("Set").GetValue<StringList>().SelectedIndex == 1)
            {
                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
            }

            var objAiHeroes = vEnemy as AIHeroClient[] ?? vEnemy.ToArray();

            var t = !objAiHeroes.Any() ? EnsoulSharp.Common.TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType) : objAiHeroes[0];

            return t;
        }

        private static AIHeroClient KillableEnemy
        {

            get
            {
                AIHeroClient t = GetTarget(MrShen.Champion.PlayerSpells.Q.Range);

                return t.IsValidTarget() ? t : null;
            }
        }
    }
}
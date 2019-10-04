using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using EnsoulSharp;
using EnsoulSharp.Common;
using MrShen.Champion;
using SharpDX;
using Color = SharpDX.Color;

namespace MrShen.Modes
{
    internal static class MenuConfig
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static EnsoulSharp.Common.Menu LocalMenu { get; private set; }
        public static EnsoulSharp.SDK.MenuUI.Menu EditoMeun;
        public static EnsoulSharp.SDK.MenuUI.Menu LLocalMenu { get; private set; }
        public static void Initialize()
        {
            LocalMenu = new Menu("Shen", "MrShenEnsoul", true).SetFontStyle(FontStyle.Regular, Color.GreenYellow);
            LLocalMenu = new EnsoulSharp.SDK.MenuUI.Menu("Shen", "If u Need To Edit Setting Hold Shift and edit Shen", true);
            LLocalMenu.Attach();
            var MenuTools = new Menu("Tools", "Tools");
            LocalMenu.AddSubMenu(MenuTools);
            MenuTools.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(MenuTools.SubMenu("Orbwalking"));
            Orbwalker.SetAttack(true);

            MrShen.Champion.PlayerSpells.Initialize();
            //ModeSelector.Initialize(MenuTools);

            //Common.AutoBushManager.Initialize(MenuTools);
            Common.AutoLevelManager.Initialize(MenuTools);
            Common.SummonerManager.Initialize();
            Common.ItemManager.Initialize();
            Common.CommonSkins.Initialize(MenuTools);

            ModeCombo.Initialize(LocalMenu);
            ModeJungle.Initialize(LocalMenu);
            ModeDrawing.Initialize(LocalMenu);
            ModeUlti.Initialize(LocalMenu);
            ModePerma.Initialize(LocalMenu);

            SpiritUnit.Initialize();

            Game.OnUpdate += GameOnOnUpdate;


        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (MrShen.Champion.PlayerSpells.R.IsReady() && MrShen.Modes.ModeUlti.LocalMenu.Item("SpellR.ConfirmKey").GetValue<KeyBind>().Active)
            {
                var t = ModeUlti.GetHelplessAlly;
                if (t != null && MrShen.Champion.PlayerSpells.R.IsReady())
                {
                    MrShen.Champion.PlayerSpells.R.CastOnUnit(t);
                }
            }
        }

        private static IEnumerable<object> GetSubMenu(Menu menu)
        {
            yield return menu;

            foreach (var childChild in menu.Children.SelectMany(GetSubMenu))
                yield return childChild;
        }

        private static List<AIHeroClient> GetHelplessTeamMate()
        {
            return null;
        }
    }
}

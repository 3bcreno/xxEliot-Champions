using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.Common;
using SharpDX;
using MrShen.Champion;
using MrShen.Common;
using Color = System.Drawing.Color;

namespace MrShen.Modes
{
    internal class ModeDrawing
    {
        public static EnsoulSharp.Common.Menu LocalMenu { get; private set; }
        private static Spell R => MrShen.Champion.PlayerSpells.R;

        public static void Initialize(Menu menuConfig)
        {
            LocalMenu = new EnsoulSharp.Common.Menu("Drawings", "Drawings");
            {
                var menuSword = new EnsoulSharp.Common.Menu("Sword", "Menu.Sword");
                {
                    menuSword.AddItem(
                        new MenuItem("Draw.SwordPosition", "Draw: Sword Position").SetValue(new Circle(true, Color.Red)));
                    menuSword.AddItem(
                        new MenuItem("Draw.SwordHitPosition", "Draw: Sword Enemy Hit Position").SetValue(new Circle(
                            true, Color.Wheat)));
                    LocalMenu.AddSubMenu(menuSword);
                }
                LocalMenu.AddItem(new MenuItem("Draw.Q", "Q: Extra AA Range").SetValue(new Circle(true, Color.Gray)))
                    .SetFontStyle(FontStyle.Regular, MrShen.Champion.PlayerSpells.W.MenuColor());
                LocalMenu.AddItem(new MenuItem("Draw.W", "W: Effect Range").SetValue(new Circle(true, Color.Gray)))
                    .SetFontStyle(FontStyle.Regular, MrShen.Champion.PlayerSpells.W.MenuColor());
                LocalMenu.AddItem(new MenuItem("Draw.E", "E: Range").SetValue(new Circle(false, Color.GreenYellow)))
                    .SetFontStyle(FontStyle.Regular, MrShen.Champion.PlayerSpells.R.MenuColor());
                LocalMenu.AddItem(new MenuItem("Draw.EF", "Flash + E Range").SetValue(new Circle(false, Color.Coral)))
                    .SetFontStyle(FontStyle.Regular, MrShen.Champion.PlayerSpells.E.MenuColor());
                menuConfig.AddSubMenu(LocalMenu);
            }

            Drawing.OnDraw += DrawingOnOnDraw;
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            var drawQ = LocalMenu.Item("Draw.Q").GetValue<Circle>();
            if (drawQ.Active && MrShen.Champion.PlayerSpells.Q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, MrShen.Champion.PlayerSpells.Q.Range, drawQ.Color);
            }

            var drawW = LocalMenu.Item("Draw.W").GetValue<Circle>();
            if (drawW.Active && MrShen.Champion.PlayerSpells.W.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, MrShen.Champion.PlayerSpells.W.Range, drawW.Color);
            }

            var drawE = LocalMenu.Item("Draw.E").GetValue<Circle>();
            if (drawE.Active && MrShen.Champion.PlayerSpells.E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, MrShen.Champion.PlayerSpells.E.Range, drawE.Color);
            }

            var drawEf = LocalMenu.Item("Draw.EF").GetValue<Circle>();
            if (drawEf.Active && ModePerma.FlashEActive)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, MrShen.Champion.PlayerSpells.E.Range + 410,
                    drawEf.Color);
            }

            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if (MrShen.Champion.SpiritUnit.SwordUnit != null)
            {
                var drawSwordPosition = MrShen.Modes.ModeDrawing.LocalMenu.Item("Draw.SwordPosition").GetValue<Circle>();
                if (drawSwordPosition.Active)
                {
                    Render.Circle.DrawCircle(MrShen.Champion.SpiritUnit.SwordUnit.Position, 350f, drawSwordPosition.Color);
                }
                var drawSwordHitPosition =
                    MrShen.Modes.ModeDrawing.LocalMenu.Item("Draw.SwordHitPosition").GetValue<Circle>();
                if (drawSwordHitPosition.Active)
                {
                    var toPolygon = new Common.Geometry.Rectangle(ObjectManager.Player.Position.To2D(),
                        MrShen.Champion.SpiritUnit.SwordUnit.Position.To2D(), 50);
                    var x = toPolygon.ToPolygon();

                    x.Draw(drawSwordHitPosition.Color, 1);
                }
            }
        }
    }
}
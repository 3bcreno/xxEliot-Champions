using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.Common;
using SharpDX;
using MrShen.Common;
using Color = SharpDX.Color;

namespace MrShen.Modes
{
    internal class ModeUlti
    {

        public static EnsoulSharp.Common.Menu LocalMenu;
        private static string MenuTab => "    ";
        private static Spell W => MrShen.Champion.PlayerSpells.W;
        private static Spell R => MrShen.Champion.PlayerSpells.R;
        private static KeyBind ActiveConfirmKey => LocalMenu.Item("SpellR.ConfirmKey").GetValue<KeyBind>();
        private static Vector2 pingLocation;
        private static int lastPingTickCount = 0;
        private static PingCategory pingCategory = PingCategory.Fallback;

        public static AIHeroClient GetHelplessAlly
        {
            get
            {
                IEnumerable<AIHeroClient> vMax =
                    HeroManager.Allies.Where(
                        ally =>
                            !ally.IsDead && !ally.IsMe && !ally.InShop() && !ally.HasBuff("Recall") &&
                            ally.CountEnemiesInRange(ally.UnderAllyTurret() ? 550 : 350 + 350) > 0)
                        .Where(ally =>
                            LocalMenu.Item(ally.CharacterName + ".UseRWarning").GetValue<StringList>().SelectedIndex != 0)
                        .Where(ally =>
                            ally.HealthPercent <=
                            LocalMenu.Item(ally.CharacterName + ".UseRWarning").GetValue<StringList>().SelectedIndex * 5)
                        .OrderByDescending(ally =>
                            LocalMenu.Item(ally.CharacterName + ".UseRPriority").GetValue<Slider>().Value);
                return vMax.FirstOrDefault();
            }
        }

        public static void Initialize(EnsoulSharp.Common.Menu menuConfig)
        {
            LocalMenu = new EnsoulSharp.Common.Menu("R Settings", "TeamMates").SetFontStyle(FontStyle.Regular, SharpDX.Color.GreenYellow);

            //var menuImportant = new LeagueSharp.Common.Menu("Important Ally", "Menu.Important");

            //string[] strImportantAlly = new string[5];
            //strImportantAlly[0] = "No one are important!";

            //List<Obj_AI_Hero> allyList = HeroManager.Allies.Where(a => !a.IsMe).ToList();

            //for (int i = 0; i < allyList.Count; i++)
            //{
            //    strImportantAlly[i + 1] = allyList[i].CharData.BaseSkinName;
            //}

            //menuImportant.AddItem(new MenuItem("Important.Champion", "Ally Champion:").SetValue(new StringList(strImportantAlly, 0)).SetFontStyle(FontStyle.Regular, Color.GreenYellow));
            //menuImportant.AddItem(new MenuItem("Important.ShowStatus", "Show Important Ally HP Status").SetValue(new StringList(new []{"Off", "On"}, 1)).SetFontStyle(FontStyle.Regular, Color.Aqua));
            //menuImportant.AddItem(new MenuItem("Important.ShowPosition", "Show Important Ally Position in Team Fight").SetValue(new Circle(true, System.Drawing.Color.Aqua)).SetFontStyle(FontStyle.Regular, Color.Aqua));

            //LocalMenu.AddSubMenu(menuImportant);

            foreach (var ally in HeroManager.Allies.Where(a => !a.IsMe))
            {
                var menuAlly = new EnsoulSharp.Common.Menu(ally.CharacterName, "Ally." + ally.CharacterName).SetFontStyle(FontStyle.Regular, SharpDX.Color.Coral);
                {
                    menuAlly.AddItem(new MenuItem(ally.CharacterName + ".UseW", "W: Auto Protection").SetValue(new StringList(new[] { "Don't Use", "Use if he need" }, GetProtection(ally.CharacterName) >= 3 ? 1 : 0)).SetFontStyle(FontStyle.Regular, W.MenuColor()));

                    string[] strR = new string[15];
                    strR[0] = "Off";

                    for (var i = 1; i < 15; i++)
                    {
                        strR[i] = "if hp <= % " + (i * 5);
                    }
                    menuAlly.AddItem(new MenuItem(ally.CharacterName + ".UseRWarning", "R: Warn me:").SetValue(new StringList(strR, GetProtection(ally.CharacterName) == 1 ? 2 : (GetProtection(ally.CharacterName) == 2 ? 5 : 8))).SetFontStyle(FontStyle.Regular, R.MenuColor()));
                    menuAlly.AddItem(new MenuItem(ally.CharacterName + ".UseRConfirm", "R: Confirm:").SetValue(new StringList(new[] { "I'll Confirm with Confirmation Key!", "Auto Use Ultimate!" }, GetProtection(ally.CharacterName) >= 3 ? 1 : 0)).SetFontStyle(FontStyle.Regular, R.MenuColor()));
                    menuAlly.AddItem(new MenuItem(ally.CharacterName + ".UseRPriority", "R: Priority:").SetValue(new Slider(GetProtection(ally.CharacterName) + 2, 1, 5)).SetFontStyle(FontStyle.Regular, R.MenuColor()));
                }
                LocalMenu.AddSubMenu(menuAlly);
            }

            string[] strHpBarStatus = new[] { "Off", "Priority >= 1", "Priority >= 2", "Priority >= 3", "Priority >= 4", "Priority = 5" };

            LocalMenu.AddItem(new MenuItem("SpellR.DrawHPBarStatus", "Show HP Bar Status").SetValue(new StringList(strHpBarStatus, 3)).SetFontStyle(FontStyle.Regular, SharpDX.Color.Aqua));
            LocalMenu.AddItem(new MenuItem("SpellR.WarnNotificationText", "Warn me with notification text").SetValue(new StringList(strHpBarStatus, 3)).SetFontStyle(FontStyle.Regular, SharpDX.Color.Aqua));
            LocalMenu.AddItem(new MenuItem("SpellR.WarnPingAlly", "Warn me with local ping").SetValue(new StringList(new[] { "Off", "Danger", "Fallback" }, 2)));

            string[] strAutoUltimate = new string[15];
            strAutoUltimate[0] = "Off";

            for (var i = 1; i < 15; i++)
            {
                strAutoUltimate[i] = "If my hp >= % " + (i * 5);
            }
            LocalMenu.AddItem(new MenuItem("SpellR.AutoUltimate", "Auto Ulti Condition:").SetValue(new StringList(strAutoUltimate, 8)).SetFontStyle(FontStyle.Regular, SharpDX.Color.IndianRed));
            LocalMenu.AddItem(new MenuItem("SpellR.ConfirmKey", "Ulti Confirm Key!").SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)).SetFontStyle(FontStyle.Bold, SharpDX.Color.GreenYellow));

            menuConfig.AddSubMenu(LocalMenu);


            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnDraw += DrawingOnOnDrawUlti;
            Game.OnUpdate += GameOnOnUpdate;
            AIBaseClient.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        private static void WProtection()
        {
            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsMe && ally.IsDead && LocalMenu.Item(ally.CharacterName + ".UseW").GetValue<StringList>().SelectedIndex == 1))
            {

            }
        }
        private static void DrawingOnOnDrawUlti(EventArgs args)
        {
            if (!R.IsReady())
            {
                return;
            }

            var ally = GetHelplessAlly;
            if (ally != null)
            {
                var allyConfirmUltimate = LocalMenu.Item(ally.CharacterName + ".UseRConfirm").GetValue<StringList>().SelectedIndex;
                //if (allyConfirmUltimate == 1 && R.IsReady() && ObjectManager.Player.HealthPercent >= LocalMenu.Item("SpellR.AutoUltimate").GetValue<StringList>().SelectedIndex * 5)
                if (LocalMenu.Item("SpellR.WarnNotificationText").GetValue<StringList>().SelectedIndex != 0)
                {
                    if (allyConfirmUltimate == 1 && R.IsReady())
                    {
                        if (Modes.MenuConfig.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                        {
                            DrawWarningMessage(args, "AUTO ULTIMATE: " + ally.CharacterName, Color.GreenYellow);
                            R.CastOnUnit(ally);
                        }
                        else
                        {
                            var warningText = "Press " + char.ConvertFromUtf32((int)ActiveConfirmKey.Key) + " for Ulti: " + ally.CharacterName;
                            DrawWarningMessage(args, warningText, Color.Red);
                        }
                    }
                    else
                    {
                        var warningText = "Press " + char.ConvertFromUtf32((int)ActiveConfirmKey.Key) + " for Ulti: " + ally.CharacterName;
                        DrawWarningMessage(args, warningText, Color.Red);
                    }
                }

                if (LocalMenu.Item("SpellR.WarnPingAlly").GetValue<StringList>().SelectedIndex != 0)
                {
                    switch (LocalMenu.Item("SpellR.WarnPingAlly").GetValue<StringList>().SelectedIndex)
                    {
                        case 1:
                            {
                                pingCategory = PingCategory.Danger;
                                break;
                            }
                        case 2:
                            {
                                pingCategory = PingCategory.Fallback;
                                break;
                            }
                    }

                    Ping(ally.Position.To2D());
                }
            }
        }

        private static void DrawWarningMessage(EventArgs args, string message = "", SharpDX.Color color = default(Color))
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            var xColor = color;
            DrawHelper.DrawText(DrawHelper.TextWarning, message, Drawing.Width * 0.301f, Drawing.Height * 0.422f,
                SharpDX.Color.Black);
            DrawHelper.DrawText(DrawHelper.TextWarning, message, Drawing.Width * 0.30f, Drawing.Height * 0.42f,
                xColor);
        }

        public static void Obj_AI_Hero_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe && sender.IsEnemy && sender is AIHeroClient && args.Target.IsAlly && args.Target is AIHeroClient && !args.Target.IsMe)
            {
                var ally = args.Target as AIHeroClient;
                if (!ally.IsDead)
                {
                    if (W.IsReady() &&
                        LocalMenu.Item(ally.CharacterName + ".UseW").GetValue<StringList>().SelectedIndex == 1 &&
                        ally.NetworkId == args.Target.NetworkId &&
                        ally.Position.Distance(MrShen.Champion.SpiritUnit.SwordUnit.Position) < 350)
                    {
                        W.Cast();
                    }
                }
            }
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (LocalMenu.Item("SpellR.DrawHPBarStatus").GetValue<StringList>().SelectedIndex != 0)
            {
                var allies =
                    HeroManager.Allies.Where(
                        a =>
                            !a.IsMe &&
                            LocalMenu.Item("SpellR.DrawHPBarStatus").GetValue<StringList>().SelectedIndex <=
                            LocalMenu.Item(a.CharacterName + ".UseRPriority").GetValue<Slider>().Value);
                var objAiHeroes = allies as AIHeroClient[] ?? allies.ToArray();

                for (var i = 0; i < objAiHeroes.Count(); i++)
                {
                    var x = 0.792f;
                    var y = 0.795f;
                    var width = 160;

                    var allyConfirmUltimate = LocalMenu.Item(objAiHeroes[i].CharacterName + ".UseRConfirm").GetValue<StringList>().SelectedIndex;

                    Drawing.DrawLine(Drawing.Width * x + 0, Drawing.Height * 0.479f + (float)(i + 1) * 18, Drawing.Width * y + width, Drawing.Height * 0.479f + (float)(i + 1) * 18, 16, System.Drawing.Color.DarkSlateGray);
                    Drawing.DrawLine(Drawing.Width * x + 1, Drawing.Height * 0.480f + (float)(i + 1) * 18, Drawing.Width * y + width - 1, Drawing.Height * 0.480f + (float)(i + 1) * 18, 14, System.Drawing.Color.Gray);

                    var hPercent = (int)Math.Ceiling((objAiHeroes[i].Health * width / objAiHeroes[i].MaxHealth));
                    if (hPercent > 0)
                    {
                        Drawing.DrawLine(Drawing.Width * x + 1, Drawing.Height * 0.480f + (float)(i + 1) * 18,
                            Drawing.Width * y + hPercent - 1, Drawing.Height * 0.480f + (float)(i + 1) * 18,
                            14, hPercent < 50 && hPercent > 30 ? System.Drawing.Color.Yellow : hPercent <= 30 ? System.Drawing.Color.Red : System.Drawing.Color.DarkOliveGreen);
                    }

                    DrawHelper.DrawText(DrawHelper.Text, objAiHeroes[i].CharacterName + " [R: " + (allyConfirmUltimate == 0 ? "Key confirm" : "Auto Ulti") + "]", Drawing.Width * y, Drawing.Height * 0.48f + (float)(i + 1) * 18, SharpDX.Color.Black);
                    //Utils.DrawText(Utils.Text, objAiHeroes[i].ChampionName + ": " + hPercent + "%", Drawing.Width * y, Drawing.Height * 0.48f + (float)(i + 1) * 20, SharpDX.Color.Black);
                }
            }
        }
        public static int GetProtection(string championName)
        {
            string[] lowProtection =
            {
                "Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen", "Gnar",
                "Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus", "Nunu",
                "Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "Sona",
                "Soraka", "Tahm", "Taric", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick", "Zac", "Zyra"
            };

            string[] mediumProtection =
            {
                "Aatrox", "Akali", "Darius", "Diana", "Ekko", "Elise", "Evelynn", "Fiddlesticks", "Fiora", "Fizz",
                "Galio", "Gangplank", "Gragas", "Heimerdinger", "Irelia", "Jax", "Jayce", "Kassadin", "Kayle", "Kha'Zix",
                "Lee Sin", "Lissandra", "Maokai", "Mordekaiser", "Morgana", "Nocturne", "Nidalee", "Pantheon", "Poppy",
                "RekSai", "Rengar", "Riven", "Rumble", "Ryze", "Shaco", "Swain", "Trundle", "Tryndamere", "Udyr",
                "Urgot", "Vladimir", "Vi", "XinZhao", "Yasuo", "Zilean"
            };

            string[] highProtection =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven", "Ezreal",
                "Graves", "Jhin", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
                "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
                "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
                "Zed", "Ziggs","Zoe"
            };

            if (mediumProtection.Contains(championName))
            {
                return 2;
            }

            if (highProtection.Contains(championName))
            {
                return 3;
            }

            return 1;
        }

        private static void Ping(Vector2 position)
        {
            if (Utils.TickCount - lastPingTickCount < 30 * 1000)
            {
                return;
            }

            lastPingTickCount = Utils.TickCount;
            pingLocation = position;
          
        }

        public static bool UnderAllyTurret(AIBaseClient unit)
        {
            return ObjectManager.Get<AITurretClient>().Where<AITurretClient>(turret =>
            {
                if (turret == null || !turret.IsValid || turret.Health <= 0f)
                {
                    return false;
                }
                if (!turret.IsEnemy)
                {
                    return true;
                }
                return false;
            })
                .Any<AITurretClient>(
                    turret =>
                        Vector2.Distance(unit.Position.To2D(), turret.Position.To2D()) < 900f && turret.IsAlly);
        }
    }
}

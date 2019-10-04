using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using EnsoulSharp;
using EnsoulSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using MrShen.Modes;


namespace MrShen
{
    internal class Program
    {
        public const string ChampionName = "Shen";

        private static SpellSlot TeleportSlot = ObjectManager.Player.GetSpellSlot("SummonerTeleport");

        private static float EManaCost => ObjectManager.Player.GetSpell(SpellSlot.E).ManaCost;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs e)
        {
            if (ObjectManager.Player.CharacterName != ChampionName)
            {
                return;
            }

            MrShen.Modes.MenuConfig.Initialize();
            //Shen.Champion.Drawings.Initialize();

            //Create the menu

            Game.OnUpdate += Game_OnUpdate;
            //AIBaseClient.OnProcessSpellCast += ObjOnProcessSpellCast;
            //Drawing.OnDraw += Drawing_OnDraw;
            Interrupters.OnInterrupter += Interrupter2_OnInterruptableTarget;
            Gapclosers.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Chat.Print($"<font color='#FFFFFF'>Mr{ChampionName}</font> <font color='#70DBDB'>xxEliot Loaded!</font>");
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            //var x = ObjectManager.Player.Spellbook.SpellEndTime - Game.Time;
            //Chat.Print(10 + "  :  " + (Game.Time - 17));
            //foreach (var buff in ObjectManager.Player.Buffs)
            //{
            //    Console.WriteLine(buff.Name + " : " + buff.StartTime + " : " + buff.EndTime);
            //}
            //return;
            //if (ObjectManager.Player.)
            //{
            //    Chat.Print("Passive Ok!");
            //}
            //else
            //{
            //    Chat.Print("Don't have Passive");
            //}
            return;
            //Chat.Print("------------------------------");
            //if (ObjectManager.Player.Distance(MrShen.Champion.SpiritUnit.SwordUnit.Position) < 350f)
            //{
            //    Chat.Print("You are In");
            //}
            //else
            //{
            //    Chat.Print("You are Out");

            //}
        }
        public static bool InShopRange(AIHeroClient xAlly)
        {
            return
                (from shop in ObjectManager.Get<ShopClient>() where shop.IsAlly select shop).Any<ShopClient>(
                    shop => Vector2.Distance(xAlly.Position.To2D(), shop.Position.To2D()) < 1250f);
        }

        public static int CountAlliesInRange(float range, Vector3 point)
        {
            return
                (from units in ObjectManager.Get<AIHeroClient>()
                 where units.IsAlly && units.IsVisible && !units.IsDead
                 select units).Count<AIHeroClient>(
                        units => Vector2.Distance(point.To2D(), units.Position.To2D()) <= range);
        }

        public static int CountEnemysInRange(float range, Vector3 point)
        {
            return
                (from units in ObjectManager.Get<AIHeroClient>() where units.IsValidTarget() select units)
                    .Count<AIHeroClient>(units => Vector2.Distance(point.To2D(), units.Position.To2D()) <= range);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

        }
        private static void Interrupter2_OnInterruptableTarget(
           ActiveInterrupter interrupter)
        {
            
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            //if (MrShen.Champion.PlayerSpells.E.IsReady() && gapcloser.Sender.IsValidTarget(MrShen.Champion.PlayerSpells.E.Range))
            //{
            //   MrShen.Champion.PlayerSpells.E.Cast(gapcloser.Sender.Position);
            //}
        }

        private static void ObjOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe && sender.IsEnemy && sender is AIHeroClient && args.Target.IsMe)
            {

                Chat.Print(sender.CharacterData.SkinName + " Attacking!!!");
            }

            return;
        }
    }
}

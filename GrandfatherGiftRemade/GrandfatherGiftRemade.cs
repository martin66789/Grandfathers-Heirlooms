﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using pepoHelper;

namespace GrandfatherGiftRemade
{
    class ModConfig
    {
        public int triggerDate { get; set; }
        public bool giveChest { get; set; }
        public bool traceLogging { get; set; }
        public string weaponStats { get; set; }

        public ModConfig()
        {
            this.triggerDate = 2;
            this.giveChest = true;
            this.traceLogging = true;
            this.weaponStats = "3/5/.5/0/5/0/1/-1/-1/0/.20/3";
        }
    }

    /// <summary>Mod entry point</summary>
    public class GrandfatherGiftRemade : Mod, IAssetEditor
    {
        /***** Constants *****/
        const int WEAP_ID = 20;

        /***** Properteze *****/
        private ModConfig Config;
        private SDate triggerDate;
        private bool abortMod = false;

        /***** Publique Methodes *****/
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data/weapons")) return true;
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (!asset.AssetNameEquals("Data/weapons")) return;
            IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
            string wName = this.Helper.Translation.Get("weapon.name");
            string wDesc = this.Helper.Translation.Get("weapon.desc");
            string wStat = this.Config.weaponStats;
            string wData = $"{wName}/{wDesc}/{wStat}";
            data[WEAP_ID] = wData;
            this.Monitor.Log($"weapon {WEAP_ID} set to {wData}");
        }

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            this.PrepTrigger();
            this.RegisterEvents("Mod Startup");
        }

        private void PrepTrigger()
        {
            int triggerDateDay = this.Config.triggerDate;
            if (triggerDateDay < 2) triggerDateDay = 2;
            else if (triggerDateDay > 28) triggerDateDay = 28;
            // this.triggerDate = new SDate(triggerDateDay, "spring", 1);
            SDate tD = new SDate(25, "spring", 3);
            this.Monitor.Log($"triggerDate set to {tD.Day} {tD.Season} {tD.Year}");
            this.triggerDate = tD;
        }


        /***** Private Methodes *****/

        private void traceLoggingIf(string message)
        {
            if (this.Config.traceLogging) this.Monitor.Log(message, LogLevel.Trace);
        }

        private void RegisterEvents(string reason)
        {
            var evtLoop = this.Helper.Events.GameLoop;
            evtLoop.OneSecondUpdateTicked += this.Supervisor;
            evtLoop.DayStarted += this.OnDayStarted;
            this.Monitor.Log($"Events registered: {reason}");
        }

        private void DeregisterEvents(string reason)
        {
            var evtLoop = this.Helper.Events.GameLoop;
            evtLoop.OneSecondUpdateTicked -= this.Supervisor;
            evtLoop.DayStarted -= this.OnDayStarted;
            this.Monitor.Log($"Events deregistered: {reason}");
        }

        private void Supervisor(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (this.abortMod) this.DeregisterEvents("ABORTING MOD");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            var curDate = SDate.Now();
            if (curDate > triggerDate)
            {
                this.DeregisterEvents("passed triggerDate");
            }
            if (curDate != triggerDate) return;

            this.DeregisterEvents("triggered");

            // First pop-up: Narration about what happened last night
            string message = this.Helper.Translation.Get("message1");
            pepoHelper.DialogOnBlack newDayMessage = new pepoHelper.DialogOnBlack(message);
            Game1.activeClickableMenu = newDayMessage;

            // Shift farmer to the left to leave the bed
            Farmer farmer = Game1.player;
            farmer.moveHorizTiles(-2);

            // TODO: Add a package object with interaction event -- see OnPackageOpen below

            // TODO: Remove package from map, add weapon & chest to inventory

        }

        private void OnPackageOpen(object sender)
        {
            string message = this.Helper.Translation.Get("message2");
            LetterViewerMenu letter = new LetterViewerMenu(message);
            Game1.activeClickableMenu = letter;

            // TODO: Wait until menu is gone before adding:
            Game1.player.addItemByMenuIfNecessary(new MeleeWeapon(WEAP_ID));
            // TODO: Add chest
        }

    }
}

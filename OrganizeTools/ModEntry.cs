using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace OrganizeTools
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private ModConfig Config;
        string playerName = null;
        string playerFarmName = null;
        string playerID = null;

        private Dictionary<string, int> toolSlots = null;
        private string orgKey = null;
        private string setKey = null;
        private string coffeeKey = null;
        private Dictionary<int, Dictionary<string, string>> equipSets = null;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            
            this.toolSlots = this.Config.ToolSlots;
            this.orgKey = this.Config.OrgKey;
            this.setKey = this.Config.SetKey;
            this.coffeeKey = this.Config.CoffeeKey;

            helper.Events.GameLoop.SaveLoaded += this.PlayerEnteringWorld;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.ButtonReleased += this.OnButtonReleased;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>

        private void PlayerEnteringWorld(object sender, SaveLoadedEventArgs e)
        {
            this.playerName = Game1.player.Name;
            this.playerFarmName = Game1.player.farmName;
            this.playerID = this.playerName + "-" + this.playerFarmName;

            this.equipSets = this.Config.EquipSets.ContainsKey(this.playerID) ? this.Config.EquipSets[this.playerID] : new Dictionary<int, Dictionary<string, string>>() {};
        }

        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            string keyReleased = e.Button.ToString();                
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            string keyPressed = e.Button.ToString();

            if (keyPressed == orgKey)
                OrganizeTools();
            else if (keyPressed == "MouseX1")
                DeselectSlot();
            else if (keyPressed == setKey)
                setConfig();
            else if (keyPressed == coffeeKey && this.Helper.Input.IsDown(SButton.LeftShift))
            {
                this.Helper.Input.Suppress(e.Button);
                drinkCoffee();
            }
            else if (Regex.IsMatch(keyPressed, @"\b[D][0-9]\b"))
            {
                if (this.Helper.Input.IsDown(SButton.LeftShift))
                {
                    this.Helper.Input.Suppress(e.Button);
                    setEquipSet(keyPressed);
                }
                else if (this.Helper.Input.IsDown(SButton.LeftControl))
                {
                    this.Helper.Input.Suppress(e.Button);
                    int num = keyPressed[1] - '0';
                    if (this.equipSets.ContainsKey(num))
                        equipEquipSet(keyPressed);
                }
            }
        }

        private void OrganizeTools()
        {
            List<Item> inventory = new List<Item>(Game1.player.Items);
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] != null)
                {
                    String itemName = inventory[i].DisplayName;
                    String itemType = inventory[i].ToString().Split('.').ToList().Last();

                    if (toolSlots.ContainsKey(itemType) || itemType == "MeleeWeapon" || itemType == "Slingshot")
                    {
                        int slot;
                        if (itemName == "Scythe")
                            slot = toolSlots["Scythe"];
                        else if (itemType == "MeleeWeapon" || itemType == "Slingshot")
                            slot = toolSlots["Weapon"];
                        else
                            slot = toolSlots[itemType];

                        if (slot != i)
                        {
                            Item temp = null;
                            String tempType = null;
                            if (inventory[slot] != null)
                            {
                                temp = inventory[slot];
                                tempType = temp.ToString().Split('.').ToList().Last();
                            }

                            inventory[slot] = inventory[i];
                            inventory[i] = temp;

                            if ((toolSlots.ContainsKey(itemType) || itemType == "MeleeWeapon" || itemType == "Slingshot") && tempType != itemType)
                                i = i - 1;
                        }
                    }
                }
            }
            Game1.player.setInventory(inventory);
        }

        private void DeselectSlot()
        {
            Game1.player.CurrentToolIndex = int.MaxValue;
        }

        private void setConfig()
        {
            List<Item> inventory = new List<Item>(Game1.player.Items);
            bool weaponIsSet = false;
            for (int i = 0; i < inventory.Count; i++)
            {

                string itemName = inventory[i]?.DisplayName;
                string itemType = inventory[i]?.ToString().Split('.').ToList().Last();
                if (itemName != null && itemType != null && (toolSlots.ContainsKey(itemType) || itemType == "MeleeWeapon"))
                {
                    if (itemName == "Scythe")
                        toolSlots["Scythe"] = i;
                    else if ((itemType == "MeleeWeapon" || itemType == "Slingshot") && !weaponIsSet)
                    {
                        toolSlots["Weapon"] = i;
                        weaponIsSet = true;
                    }
                    else
                        toolSlots[itemType] = i;
                }
            }

            this.Config.ToolSlots = toolSlots;
            this.Helper.WriteConfig(this.Config);
        }

        private void setEquipSet(string numKey)
        {
            int num = numKey[1] - '0';
            this.equipSets[key: num] = new Dictionary<string, string> {
                {"hat", Game1.player.hat.Get() == null ? null : Game1.player.hat.Get().DisplayName},
                {"leftRing", Game1.player.leftRing.Get() == null ? null : Game1.player.leftRing.Get().DisplayName},
                {"rightRing", Game1.player.rightRing.Get() == null ? null : Game1.player.rightRing.Get().DisplayName},
                {"boots", Game1.player.boots.Get() == null ? null : Game1.player.boots.Get().DisplayName}
            };
            
            this.Config.EquipSets[this.playerID] = this.equipSets;
            this.Helper.WriteConfig(this.Config);
        }

        private void equipEquipSet(string numKey)
        {
            Game1.playSound("shwip");
            List<Item> inventory = new List<Item>(Game1.player.Items);
            int num = numKey[1] - '0';

            foreach (string SlotName in this.equipSets[num].Keys)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    Item temp = null;

                    if (inventory[i] != null && this.equipSets[num][SlotName] == inventory[i].DisplayName)
                    {
                        if (SlotName == "hat")
                        {
                            temp = Game1.player.hat.Get();
                            Game1.player.hat.Set((StardewValley.Objects.Hat)inventory[i]);
                        }
                        else if (SlotName == "leftRing")
                        {
                            temp = Game1.player.leftRing.Get();
                            Game1.player.leftRing.Set((StardewValley.Objects.Ring)inventory[i]);
                        }
                        else if (SlotName == "rightRing")
                        {
                            temp = Game1.player.rightRing.Get();
                            Game1.player.rightRing.Set((StardewValley.Objects.Ring)inventory[i]);
                        }
                        else if (SlotName == "boots")
                        {
                            temp = Game1.player.boots.Get();
                            Game1.player.boots.Set((StardewValley.Objects.Boots)inventory[i]);
                        }

                        inventory[i] = temp;
                        break;
                    }
                    else if (inventory[i] == null && this.equipSets[num][SlotName] == null)
                    {
                        if (SlotName == "hat")
                        {
                            inventory[i] = Game1.player.hat.Get();
                            Game1.player.hat.Set(null);
                        }
                        else if (SlotName == "leftRing")
                        {
                            inventory[i] = Game1.player.leftRing.Get();
                            Game1.player.leftRing.Set(null);
                        }
                        else if (SlotName == "rightRing")
                        {
                            inventory[i] = Game1.player.rightRing.Get();
                            Game1.player.rightRing.Set(null);
                        }
                        else if (SlotName == "boots")
                        {
                            inventory[i] = Game1.player.boots.Get();
                            Game1.player.boots.Set(null);
                        }

                        break;
                    }
                }
                Game1.player.setInventory(inventory);
            }  
        }

        private void drinkCoffee()
        {
            if (Game1.player.hasItemInInventoryNamed("Coffee"))
            {
                Game1.player.eatObject((StardewValley.Object)getInventoryItemObject("Coffee"));
                Game1.player.consumeObject(395, 1);
            }
        }

        private Item getInventoryItemObject(string itemName)
        {
            foreach (Item inventoryItem in Game1.player.Items)
            {
                if (inventoryItem?.DisplayName == itemName)
                {
                    return inventoryItem;
                }
            }
            return null;
        }

        private int getInventoryItemIndex(string itemName)
        {
            IList<Item> inventory = Game1.player.Items;
            for (int i=0; i < inventory.Count; i++)
            {
                if (inventory[i] != null && inventory[i].DisplayName == itemName)
                    return i;
            }
            return int.MaxValue;
        }
    }
}
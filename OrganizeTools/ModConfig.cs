using System;
using System.Collections.Generic;

namespace OrganizeTools
{
    class ModConfig
    {
        public Dictionary<string, int> ToolSlots { get; set; } = new Dictionary<string, int>() 
        {
            {"Weapon", 5},
            {"Scythe", 6},
            {"Pickaxe", 7},
            {"Axe", 8},
            {"Hoe", 9},
            {"FishingRod", 10},
            {"WateringCan", 11}
        };

        public string OrgKey { get; set; } = "OemTilde";

        public string SetKey { get; set; } = "NumPad5";

        public string CoffeeKey { get; set; } = "E";

        public Dictionary<string, Dictionary<int, Dictionary<string, string>>> EquipSets { get; set; } = new Dictionary<string, Dictionary<int, Dictionary<string, string>>>() { };
    }
}

﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DarkwoodCustomizer;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInProcess("Darkwood.exe")]
public class Plugin : BaseUnityPlugin
{
    public static ConfigFile ConfigFile;
    public static ConfigFile StacksConfigFile;
    public static ConfigFile LanternConfigFile;
    public static ConfigFile InventoriesConfigFile;
    public static ConfigFile PlayerConfigFile;
    public const string PluginAuthor = "amione";
    public const string PluginName = "DarkwoodCustomizer";
    public const string PluginVersion = "1.1.2";
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public static ManualLogSource Log;
    public static FileSystemWatcher fileWatcher;

    // Base Plugin Values
    public static ConfigEntry<bool> LogDebug;

    // Stack Resize Values
    public static ConfigEntry<bool> ChangeStacks;
    public static ConfigEntry<bool> UseGlobalStackSize;
    public static ConfigEntry<int> StackResize;
    public static ConfigEntry<string> jsonStacks;
    public static Dictionary<string, int> CustomStacks;

    // Lantern Repair Values
    public static ConfigEntry<bool> RepairLantern;
    public static ConfigEntry<string> LanternRepairConfig;
    public static ConfigEntry<int> LanternAmountRepairConfig;
    public static ConfigEntry<float> LanternDurabilityRepairConfig;
    public static ConfigEntry<bool> LogItems;

    // Inventory Resize Values
    public static ConfigEntry<bool> RemoveExcess;

    // Workbench
    public static ConfigEntry<float> WorkbenchCraftingOffset;
    public static ConfigEntry<int> RightSlots;
    public static ConfigEntry<int> DownSlots;

    // Inventory
    public static ConfigEntry<int> InventoryRightSlots;
    public static ConfigEntry<int> InventoryDownSlots;
    public static ConfigEntry<bool> InventorySlots;

    // Hotbar
    public static ConfigEntry<int> HotbarRightSlots;
    public static ConfigEntry<int> HotbarDownSlots;
    public static ConfigEntry<bool> HotbarSlots;

    // Player Values
    public static ConfigEntry<bool> PlayerModification;
    public static ConfigEntry<float> PlayerFOV;

    // Player Stamina Values
    public static ConfigEntry<bool> PlayerStaminaModification;
    public static ConfigEntry<int> PlayerStaminaUpgrades;
    public static ConfigEntry<float> PlayerMaxStamina;
    public static ConfigEntry<float> PlayerStaminaRegenInterval;
    public static ConfigEntry<float> PlayerStaminaRegenValue;
    public static ConfigEntry<bool> PlayerInfiniteStamina;
    public static ConfigEntry<bool> PlayerInfiniteStaminaEffect;

    // Player Health Values
    public static ConfigEntry<bool> PlayerHealthModification;
    public static ConfigEntry<int> PlayerHealthUpgrades;
    public static ConfigEntry<float> PlayerMaxHealth;
    public static ConfigEntry<float> PlayerHealthRegenInterval;
    public static ConfigEntry<float> PlayerHealthRegenModifier;
    public static ConfigEntry<float> PlayerHealthRegenValue;
    public static ConfigEntry<bool> PlayerGodmode;

    // Player Speed Values
    public static ConfigEntry<bool> PlayerSpeedModification;
    public static ConfigEntry<float> PlayerWalkSpeed;
    public static ConfigEntry<float> PlayerRunSpeed;
    public static ConfigEntry<float> PlayerRunSpeedModifier;

    private void Awake()
    {
        Log = Logger;
        ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, PluginGUID + ".cfg"), true);
        StacksConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, PluginGUID + ".Stacks.cfg"), true);
        LanternConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, PluginGUID + ".Lantern.cfg"), true);
        InventoriesConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, PluginGUID + ".Inventories.cfg"), true);
        PlayerConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, PluginGUID + ".Player.cfg"), true);

        // Base Plugin config
        LogDebug = ConfigFile.Bind($"Logging", "Enable Debug Logs", true, "Whether to log debug messages, includes player information on load/change for now.");
        LogItems = ConfigFile.Bind($"Logging", "Enable Debug Logs for Items", false, "Whether to log every item, only called when the game is loading the specific item, Pro tip: enable on main menu, load your save, disable it, quit the game and open Bepinex/LogOutput.log, then you'll have all the items in the game listed");

        // Stacks config
        ChangeStacks = StacksConfigFile.Bind($"Stack Sizes", "Enable Section", false, "Whether or not stack sizes will be changed by the mod.");
        UseGlobalStackSize = StacksConfigFile.Bind($"Stack Sizes", "Enable Global Stack Size", true, "Whether to use a global stack size for all items.");
        StackResize = StacksConfigFile.Bind($"Stack Sizes", "Global Stack Resize", 50, "Number for all item stack sizes to be set to. Requires reload of save for most items to take effect (Return to Menu > Load Save)");
        var jsonStacks = StacksConfigFile.Bind($"Stack Sizes", "Custom Stacks", "{\"nail\":500,\"wood\":500}", "Warning: Enable the logs for items in the main config, if you mistake an item name the plugin wont be finishing the function, A JSON object that is a dictionary of ItemName:StackSize. Requires reload of save for most items to take effect (Return to Menu > Load Save)");
        CustomStacks = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonStacks.Value);

        // Lantern config
        RepairLantern = LanternConfigFile.Bind($"Lantern", "Enable Section", true, "Whether or not lantern can be repaired using gasoline on the workbench");
        LanternRepairConfig = LanternConfigFile.Bind($"Lantern", "Lantern Repair Item", "gasoline", "What item will be used for repairing the lantern. (recommended: gasoline or molotov)");
        LanternAmountRepairConfig = LanternConfigFile.Bind($"Lantern", "Lantern Amount of Item Used", 1, "Item amount of item to use? Ex. 1 molotov to repair");
        LanternDurabilityRepairConfig = LanternConfigFile.Bind($"Lantern", "Lantern Durability of Item Used", 0.2f, "Item durability amount to use? Ex. 0.2 of a gasoline to repair");

        // Inventories config
        RemoveExcess = InventoriesConfigFile.Bind($"Inventories", "Remove Excess Slots", true, "Whether or not to remove slots that are outside the inventory you set. For example, you set your inventory to 9x9 (81 slots) but you had a previous mod do something bigger and you have something like 128 slots extra enabling this option will remove those excess slots and bring it down to 9x9 (81)");

        // Workbench
        WorkbenchCraftingOffset = InventoriesConfigFile.Bind($"Workbench", "Workbench Crafting Offset", 1000f, "Pixels offset for the workbench crafting window, no longer requires restart, 1550 is the almost the edge of the screen on fullhd which looks nice");
        RightSlots = InventoriesConfigFile.Bind($"Workbench", "Storage Right Slots", 12, "Number that determines slots in workbench to the right, vanilla is 6");
        DownSlots = InventoriesConfigFile.Bind($"Workbench", "Storage Down Slots", 9, "Number that determines slots in workbench downward, vanilla is 8");

        // Inventory
        InventorySlots = InventoriesConfigFile.Bind($"Inventory", "Enable Section", false, "This will circumvent the inventory progression and enable this section, disable to return to default Inventory slots");
        InventoryRightSlots = InventoriesConfigFile.Bind($"Inventory", "Inventory Right Slots", 2, "Number that determines slots in inventory to the right");
        InventoryDownSlots = InventoriesConfigFile.Bind($"Inventory", "Inventory Down Slots", 9, "Number that determines slots in inventory downward");

        // Hotbar
        HotbarSlots = InventoriesConfigFile.Bind($"Hotbar", "Enable Section", false, "This will circumvent the Hotbar progression and enable this section, disable to return to default Hotbar slots");
        HotbarRightSlots = InventoriesConfigFile.Bind($"Hotbar", "Hotbar Right Slots", 1, "Number that determines slots in Hotbar to the right, requires reload of save (Return to Menu > Load Save)");
        HotbarDownSlots = InventoriesConfigFile.Bind($"Hotbar", "Hotbar Down Slots", 6, "Number that determines slots in Hotbar downward, requires reload of save (Return to Menu > Load Save)");

        // Player values
        PlayerModification = PlayerConfigFile.Bind($"Player", "Enable Section", false, "Enable this section of the mod, This section does not require restarts");
        PlayerFOV = PlayerConfigFile.Bind($"Player", "Player FoV", 90f, "Set your players' FoV (370 recommended, set to 720 if you want to always see everything)");

        // Player Stamina config
        PlayerStaminaModification = PlayerConfigFile.Bind($"Stamina", "Enable Section", false, "Enable this section of the mod, This section does not require restarts");
        PlayerMaxStamina = PlayerConfigFile.Bind($"Stamina", "Max Stamina", 100f, "Set your max stamina");
        PlayerStaminaRegenInterval = PlayerConfigFile.Bind($"Stamina", "Stamina Regen Interval", 0.05f, "Interval in seconds between stamina regeneration ticks. I believe this is the rate at which your stamina will regenerate when you are not using stamina abilities. Lowering this value will make your stamina regenerate faster, raising it will make your stamina regenerate slower.");
        PlayerStaminaRegenValue = PlayerConfigFile.Bind($"Stamina", "Stamina Regen Value", 30f, "Amount of stamina regenerated per tick. I believe this is the amount of stamina you will gain each time your stamina regenerates. Raising this value will make your stamina regenerate more per tick, lowering it will make your stamina regenerate less per tick.");
        PlayerInfiniteStamina = PlayerConfigFile.Bind($"Stamina", "Infinite Stamina", false, "On every update makes your stamina maximized");
        PlayerInfiniteStaminaEffect = PlayerConfigFile.Bind($"Stamina", "Infinite Stamina Effect", false, "Whether to draw the infinite stamina effect");

        // Player Health config
        PlayerHealthModification = PlayerConfigFile.Bind($"Health", "Enable Section", false, "Enable this section of the mod, This section does not require restarts");
        PlayerMaxHealth = PlayerConfigFile.Bind($"Health", "Max Health", 100f, "Set your max health");
        PlayerHealthRegenInterval = PlayerConfigFile.Bind($"Health", "Health Regen Interval", 5f, "Theoretically: Interval in seconds between health regeneration ticks, feel free to experiment I didn't test this out yet");
        PlayerHealthRegenModifier = PlayerConfigFile.Bind($"Health", "Health Regen Modifier", 1f, "Theoretically: Multiplier for health regen value, feel free to experiment I didn't test this out yet");
        PlayerHealthRegenValue = PlayerConfigFile.Bind($"Health", "Health Regen Value", 0f, "Theoretically: Amount of health regenerated per tick, feel free to experiment I didn't test this out yet");
        PlayerGodmode = PlayerConfigFile.Bind($"Health", "Enable Godmode", false, "Makes you invulnerable and on every update makes your health maximized");

        // Player Speed config
        PlayerSpeedModification = PlayerConfigFile.Bind($"Speed", "Enable Section", false, "Enable this section of the mod, This section does not require restarts");
        PlayerWalkSpeed = PlayerConfigFile.Bind($"Speed", "Walk Speed", 7.5f, "Set your walk speed");
        PlayerRunSpeed = PlayerConfigFile.Bind($"Speed", "Run Speed", 15f, "Set your run speed");
        PlayerRunSpeedModifier = PlayerConfigFile.Bind($"Speed", "Run Speed Modifier", 1f, "Multiplies your run speed by this value");
        LogDivider();

        Harmony Harmony = new Harmony($"{PluginGUID}");
        Log.LogInfo($"Patched InvItemClass!");
        Harmony.PatchAll(typeof(InvItemClassPatch));
        Log.LogInfo($"Patched Inventory!");
        Harmony.PatchAll(typeof(InventoryPatch));
        Log.LogInfo($"Patched Character!");
        Harmony.PatchAll(typeof(CharacterPatch));
        Log.LogInfo($"Patched Player!");
        Harmony.PatchAll(typeof(PlayerPatch));

        Log.LogInfo($"[{PluginGUID} v{PluginVersion}] has fully loaded!");
        LogDivider();

        fileWatcher = new FileSystemWatcher(Paths.ConfigPath, PluginGUID + "*.cfg");
        fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
        fileWatcher.Changed += OnFileChanged;
        fileWatcher.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        LogDivider();
        Log.LogInfo($"Reloaded configuration file.");
        LogDivider();
        switch (e.Name)
        {
            case PluginGUID + ".cfg":
                ConfigFile.Reload();
                break;
            case PluginGUID + ".Inventories.cfg":
                InventoriesConfigFile.Reload();
                break;
            case PluginGUID + ".Lantern.cfg":
                InvItemClassPatch.RefreshLantern = true;
                LanternConfigFile.Reload();
                break;
            case PluginGUID + ".Player.cfg":
                PlayerConfigFile.Reload();
                PlayerPatch.RefreshPlayer = true;
                break;
            case PluginGUID + ".Stacks.cfg":
                StacksConfigFile.Reload();
                break;
            default:
                // Handle unexpected file changes (if necessary)
                break;
        }
    }

    public static void LogDivider()
    {
        Log.LogInfo("");
        Log.LogInfo("--------------------------------------------------------------------------------");
        Log.LogInfo("");
    }
}

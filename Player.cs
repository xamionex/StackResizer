using DarkwoodCustomizer;
using HarmonyLib;
using UnityEngine;

public class PlayerPatch
{
    public static bool RefreshPlayer = true;

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    [HarmonyPostfix]
    public static void CharUpdate(Player __instance)
    {
        if (Plugin.PlayerStaminaModification.Value && Plugin.PlayerInfiniteStamina.Value)
        {
            __instance.stamina = __instance.maxStamina;
            if (Plugin.PlayerInfiniteStaminaEffect.Value)
            {
                __instance.flashStaminaBar();
            }
        }
        if (Plugin.PlayerHealthModification.Value && Plugin.PlayerGodmode.Value)
        {
            __instance.health = __instance.maxHealth;
            __instance.invulnerable = true;
        }
        else if (__instance.invulnerable) __instance.invulnerable = false;
        if (!RefreshPlayer) return;
        if (Plugin.PlayerStaminaModification.Value)
        {
            __instance.maxStamina = Plugin.PlayerMaxStamina.Value;
            __instance.staminaRegenInterval = Plugin.PlayerStaminaRegenInterval.Value;
            __instance.staminaRegenValue = Plugin.PlayerStaminaRegenValue.Value;
        }
        if (Plugin.PlayerHealthModification.Value)
        {
            __instance.maxHealth = Plugin.PlayerMaxHealth.Value;
            if (__instance.health > __instance.maxHealth)
            {
                __instance.health = __instance.maxHealth;
            }
            __instance.healthRegenInterval = Plugin.PlayerHealthRegenInterval.Value;
            __instance.healthRegenModifier = Plugin.PlayerHealthRegenModifier.Value;
            __instance.healthRegenValue = Plugin.PlayerHealthRegenValue.Value;
        }
        if (Plugin.PlayerModification.Value)
        {
            __instance.defaultFOV = Plugin.PlayerFOV.Value;
            __instance.currentDestFOV = Plugin.PlayerFOV.Value;
        }
        if (Plugin.PlayerSpeedModification.Value)
        {
            __instance.walkSpeed = Plugin.PlayerWalkSpeed.Value;
            __instance.runSpeed = Plugin.PlayerRunSpeed.Value;
            __instance.runSpeedModifier = Plugin.PlayerRunSpeedModifier.Value;
        }
        if (Plugin.LogDebug.Value)
        {
            Plugin.LogDivider();
            Plugin.Log.LogInfo($"Player has {__instance.healthUpgrades} health upgrades. Expected base game health is {100 + __instance.healthUpgrades * 25}");
            Plugin.LogDivider();
            Plugin.Log.LogInfo($"Player MaxStamina: {__instance.maxStamina}");
            Plugin.Log.LogInfo($"Player SR Interval: {__instance.staminaRegenInterval}");
            Plugin.Log.LogInfo($"Player SR Value: {__instance.staminaRegenValue}");
            Plugin.LogDivider();
            Plugin.Log.LogInfo($"Player MaxHP: {__instance.maxHealth}");
            Plugin.Log.LogInfo($"Player HPR Interval: {__instance.healthRegenInterval}");
            Plugin.Log.LogInfo($"Player HPR Modifier: {__instance.healthRegenModifier}");
            Plugin.Log.LogInfo($"Player HPR Value: {__instance.healthRegenValue}");
            Plugin.LogDivider();
            Plugin.Log.LogInfo($"Player Walk Speed: {__instance.walkSpeed}");
            Plugin.Log.LogInfo($"Player Run Speed: {__instance.runSpeed}");
            Plugin.Log.LogInfo($"Player Run Speed Modifier: {__instance.runSpeedModifier}");
            Plugin.LogDivider();
            Plugin.Log.LogInfo($"Player FoV: {__instance.currentDestFOV}");
            Plugin.LogDivider();
        }
        RefreshPlayer = false;
    }

    [HarmonyPatch(typeof(Player), nameof(Player.getHit), [typeof(float), typeof(Transform), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool)])]
    [HarmonyPrefix]
    public static void gotHit(Player __instance, float damage, Transform attackerTransform, bool CanCutInHalf, bool byPlayer, ref bool canInterrupt, bool normalHit, bool showRedScreen, bool force, bool dontShowHealthBar)
    {
        if (Plugin.PlayerCantGetInterrupted.Value)
        {
            canInterrupt = false;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.restartMeleeIndicator))]
    [HarmonyPrefix]
    public static void RestartMeleeIndicator(ref bool __runOriginal)
    {
        if (Plugin.PlayerCantGetInterrupted.Value)
        {
            __runOriginal = false;
        }
        else
        {
            __runOriginal = true;
        }
    }
}
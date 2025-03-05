using System.Collections;
using HarmonyLib;
using LethalMystery.Players;
using LethalMystery.UI;
using LethalMystery.Utils;
using UnityEngine;

namespace LethalMystery.Maps.Sabotages
{
    [HarmonyPatch]
    internal class Generator
    {
        

        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.StopInteraction))]
        [HarmonyPostfix]
        private static void FixGeneratorPatch(InteractTrigger __instance)
        {
            if (__instance.currentCooldownValue > 0f)
            {
                UserFixedGenerator();
            }
        }



        /// <summary>
        /// User was holding keybind to fix generator
        /// </summary>
        private static void UserFixedGenerator()
        {
            if (!(GOTools.GetObjectPlayerIsLookingAt().name.ToLower().Contains("doorgenerator")) && !(GOTools.GetObjectPlayerIsLookingAt().name.ToLower() == "sabo")) return;
            if (Sabotage.generatorFixed) return;
            Plugin.netHandler.sabotageReceive("lights/fix", Plugin.localPlayer.playerClientId);
        }


        /// <summary>
        /// Fix generator for everyone
        /// </summary>
        public static void FixGenerator()
        {
            Sabotage.generatorFixed = true;
            EnableFog(false);
            StartOfRound.Instance.StartCoroutine(FogCooldown());
        }



        public static void BreakGenerator()
        {
            if (Sabotage.generatorFixed == true && Sabotage.fogTimerStarted == false)
            {
                Plugin.netHandler.sabotageReceive("lights/break", Plugin.localPlayer.playerClientId);
            }
        }


        public static void EnableFog(bool value)
        {
            GameObject fog = GameObject.Find("Systems/Rendering/SpookyFog");
            if (fog != null)
            {
                fog.SetActive(value);
            }
        }

        public static IEnumerator FogCooldown()
        {
            if (Roles.CurrentRole != null && Roles.CurrentRole.Type == Roles.RoleType.employee) yield break;

            Sabotage.fogTimerStarted = true;
            yield return new WaitForSeconds(120f);
            Sabotage.fogTimerStarted = false;
            MinimapUI.lightIcon.sprite = LMAssets.LightSwitch;
        } 


        public static IEnumerator generatorEvents()
        {
            if (Sabotage.generatorFixed == true && Sabotage.fogTimerStarted == false)
            {
                if (MinimapUI.lightIcon != null)
                {
                    MinimapUI.lightIcon.sprite = LMAssets.LightSwitchSelect;
                }
                HUDManager.Instance.RadiationWarningHUD();
                Sabotage.generatorFixed = false;
                yield return new WaitForSeconds(1.2f);

                if (Roles.CurrentRole != null && Roles.CurrentRole.Type == Roles.RoleType.employee)
                {
                    EnableFog(true);
                }
               
            }

        }

    }
}

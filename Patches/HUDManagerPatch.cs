using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using LethalMystery.Players;
using UnityEngine.UI;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {


        #region Chat Commands


        [HarmonyPatch(typeof(HUDManager), "AddPlayerChatMessageClientRpc")]
        [HarmonyPrefix]
        private static bool ReadChatMessage(HUDManager __instance, ref string chatMessage, ref int playerId)
        {
            string nameOfUserWhoTyped = __instance.playersManager.allPlayerScripts[playerId].playerUsername;
            Plugin.mls.LogInfo("Chat Message: " + chatMessage + " sent by: " + nameOfUserWhoTyped);
            if (chatMessage.StartsWith('/'))
            {
                string[] temp = chatMessage.Split('/');
                string command = temp[1];

                Commands.ProcessCommandInput(command);
                chatMessage = "";
                return false;
            }

            return true;
        }


        #endregion Chat Commands


        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPostfix]
        private static void UpdatePatch(HUDManager __instance)
        {

            if (Plugin.inMeeting)
            {
                __instance.Clock.targetAlpha = 1.0f;
                __instance.Clock.canvasGroup.alpha = 1.0f;
                __instance.clockNumber.text = $"{(int)Plugin.currentMeetingCountdown}";
            }

        }

        /*
        [HarmonyPatch(typeof(HUDManager), "DisplayDaysLeft")]
        [HarmonyPostfix]
        private static void ShowResults()
        {
            HUDManager.Instance.profitQuotaDaysLeftText.text = "Monsters Won";
            HUDManager.Instance.profitQuotaDaysLeftText2.text = "";
        }
        */

        private static void ShowRole()
        {
            if (Roles.CurrentRole != null)
            {
                string text = $"{Roles.CurrentRole.Name}";
                HUDManager.Instance.profitQuotaDaysLeftText.text = text;
                HUDManager.Instance.profitQuotaDaysLeftText2.text = text;
                if (Roles.CurrentRole.Type == "employee")
                {
                    HUDManager.Instance.reachedProfitQuotaAnimator.SetTrigger("displayDaysLeftCalm");
                    HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.profitQuotaDaysLeftCalmSFX);
                }
                else
                {
                    HUDManager.Instance.reachedProfitQuotaAnimator.SetTrigger("displayDaysLeft");
                    HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.OneDayToMeetQuotaSFX);
                }
            }
        }

        private static void ShowMeeting(string type)
        {
            string text = type == "meeting" ? $"{type.ToUpper()} CALLED" : $"{type.ToUpper()} REPORTED";
            HUDManager.Instance.profitQuotaDaysLeftText.text = text;
            HUDManager.Instance.profitQuotaDaysLeftText2.text = text;
            if (type == "meeting")
            {
                HUDManager.Instance.reachedProfitQuotaAnimator.SetTrigger("displayDaysLeftCalm");
                HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.profitQuotaDaysLeftCalmSFX);
            }
            else if (type == "body")
            {
                HUDManager.Instance.reachedProfitQuotaAnimator.SetTrigger("displayDaysLeft");
                HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.OneDayToMeetQuotaSFX);
            }
        }


        public static void DisplayDaysEdit(string name)
        {
            switch (name)
            {
                case "role":
                    ShowRole();
                    break;
                case "meeting":
                    ShowMeeting("meeting");
                    break;
                case "body":
                    ShowMeeting("body");
                    break;
            }
        }

        [HarmonyPatch(typeof(HUDManager), "DisplayDaysLeft")]
        [HarmonyPrefix]
        private static bool DisplayDaysLeftPatch()
        {
            return false;
        }

    }
}

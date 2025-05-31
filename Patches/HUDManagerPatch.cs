using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.MainGame;
using LethalMystery.Players;
using LethalMystery.UI;
using LethalMystery.Utils;
using TMPro;
using UnityEngine;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {


        #region Chat Commands


        [HarmonyPatch(nameof(HUDManager.AddPlayerChatMessageServerRpc))]
        [HarmonyPrefix]
        private static bool ReadChatMessage(HUDManager __instance, ref string chatMessage)
        {
            string prefix = "";
            if (!string.IsNullOrEmpty(chatMessage.Trim()))
            {
                prefix = chatMessage[0].ToString();
            }

            if (chatMessage.Length > 150)
            {
                // possible overflow attack or smthing? idk im paranoid.
                return false;
            }


            // Plugin.netHandler.chatMsgReceive("", 0);

            if (LMConfig.PrefixSetting != null && StringAddons.CheckPrefix(prefix))
            {
                string cleanedPrefix = StringAddons.CleanPrefix(prefix);
                string[] temp = chatMessage.Split(cleanedPrefix);
                string command = temp[1];

                Commands.ProcessCommandInput(command);
                chatMessage = "";
                return false;
            }


            if (Start.hostEnableChat)
            {
                return true;
            }
            else
            {
                Commands.DisplayChatMessage($"Normal chat is  <color=#FF0000>\"Disabled\"</color>. Type {LMConfig.PrefixSetting.Value}help for a list of all commands");
                return false;
            }

        }


        #endregion Chat Commands



        /*
        [HarmonyPatch(nameof(HUDManager.DisplayDaysLeft))]
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
                string text = $"{StringAddons.Title(Roles.CurrentRole.Name)}";
                HUDManager.Instance.profitQuotaDaysLeftText.text = text;
                HUDManager.Instance.profitQuotaDaysLeftText2.text = text;
                if (Roles.CurrentRole.Type == Roles.RoleType.employee)
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

        [HarmonyPatch(nameof(HUDManager.DisplayDaysLeft))]
        [HarmonyPrefix]
        private static bool DisplayDaysLeftPatch()
        {
            return false;
        }


        [HarmonyPatch(nameof(HUDManager.ShowPlayersFiredScreen))]
        [HarmonyPrefix]
        private static void ShowEjectScreen(ref Animator ___playersFiredAnimator)
        {
            ___playersFiredAnimator.gameObject.transform.Find("MaskImage").Find("HeaderText").GetComponent<TextMeshProUGUI>().text = Plugin.firedText;
            ___playersFiredAnimator.gameObject.transform.Find("MaskImage").Find("HeaderText (1)").GetComponent<TextMeshProUGUI>().text = Plugin.firedTextSub;

        }


        /// <summary>
        /// Send message in chat if special role dropped their item
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPostfix]
        private static void DroppedItemNotif()
        {
            GOTools.CheckForWeaponInInventoryNotif();
        }




        
        [HarmonyPatch(nameof(HUDManager.FillEndGameStats))]
        [HarmonyPostfix]
        private static void RoundStats(HUDManager __instance)
        {
            foreach (KeyValuePair<ulong, string> stat in EndGame.killedByNote)
            {
                __instance.statsUIElements.playerNotesText[stat.Key].text = "Notes: \n" + stat.Value;
                __instance.statsUIElements.playerStates[stat.Key].sprite = __instance.statsUIElements.deceasedIcon;
            }

            foreach (ulong stat in EndGame.lastPlayersAlive)
            {
                __instance.statsUIElements.playerNotesText[stat].text = "Notes: \n";
                __instance.statsUIElements.playerStates[stat].sprite = __instance.statsUIElements.aliveIcon;
            }

            __instance.statsUIElements.allPlayersDeadOverlay.enabled = false;
        }
        

    }
}

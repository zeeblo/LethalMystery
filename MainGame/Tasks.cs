using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Utils;
using LethalNetworkAPI;
using UnityEngine;

namespace LethalMystery.MainGame
{

    [HarmonyPatch]
    internal class Tasks
    {
        public static bool droppedItem = false;
        public static List<string> allScraps = new List<string>();
        public static int maxQuota = 120;
        [PublicNetworkVariable]
        public static LNetworkVariable<string> currentQuota = LNetworkVariable<string>.Connect("currentQuota");

        #region Patches

        /// <summary>
        /// removes currentlyHeld scrap item in user's inventory if they're inside
        /// the ship. This also updates the current quota.
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void TaskUpdate(PlayerControllerB __instance)
        {
            if (Plugin.localPlayer == null) return;
            if (StartOfRound.Instance.shipHasLanded == false || Plugin.localPlayer.isInHangarShipRoom == false) return;
            if (Plugin.localPlayer.ItemSlots[Plugin.localPlayer.currentItemSlot] == null) return;

            string itmName = Plugin.localPlayer.ItemSlots[Plugin.localPlayer.currentItemSlot].itemProperties.itemName.ToLower().Replace("(clone)", "");
            if (!StringAddons.ContainsWhitelistedItem(itmName))
            {
                return;
            }

            Plugin.mls.LogInfo(">>> In TaskUpdate()");
            CollectScrapInSlot(__instance);
        }





        private static void CollectScrapInSlot(PlayerControllerB __instance)
        {
            Plugin.mls.LogInfo(">>> In CollectScrapInSlot()");
            if (Plugin.isHost)
            {
                Plugin.localPlayer.DestroyItemInSlotAndSync(Plugin.localPlayer.currentItemSlot);
            }
            else
            {
                string currentItem = $"{Plugin.localPlayer.playerClientId}/{Plugin.localPlayer.currentItemSlot}";
                Plugin.localPlayer.DestroyItemInSlot(Plugin.localPlayer.currentItemSlot);
                Plugin.netHandler.destroyScrapReceive(currentItem, Plugin.localPlayer.playerClientId);
            }

            HUDManager.Instance.itemSlotIcons[Plugin.localPlayer.currentItemSlot].enabled = false;

            __instance.carryWeight = 1f;
            currentQuota.Value = $"{StringAddons.AddInts(currentQuota.Value, 10)}";
            Commands.DisplayChatMessage($"Collected <color=#FF0000>+1</color> credits (check shop)");
            Plugin.terminal.groupCredits += 1;
        }


        /// <summary>
        /// Prevent user from dropping items in ship and collects it
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPrefix]
        private static bool CollectItem(PlayerControllerB __instance)
        {
            if (StringAddons.ConvertToBool(Meeting.inMeeting.Value)) return true;
            if (__instance.ItemSlots[__instance.currentItemSlot] == null) return true;
            if (__instance.ItemSlots[__instance.currentItemSlot].playerHeldBy.playerClientId != GameNetworkManager.Instance.localPlayerController.playerClientId) return true;

            string itmName = __instance.ItemSlots[__instance.currentItemSlot].itemProperties.itemName.ToLower().Replace("(clone)", "");
            if ((__instance.isInHangarShipRoom && StartOfRound.Instance.shipHasLanded && __instance.ItemSlots[__instance.currentItemSlot] != null)
                && StringAddons.ContainsWhitelistedItem(itmName))
            {
                string currentItem = $"{Plugin.localPlayer.playerClientId}/{Plugin.localPlayer.currentItemSlot}/CollectItem";
                Plugin.netHandler.showScrapReceive(currentItem, Plugin.localPlayer.playerClientId);

                HUDManager.Instance.AddNewScrapFoundToDisplay(__instance.ItemSlots[__instance.currentItemSlot]);
                droppedItem = true; // Activates CheckCollectItem()
                CollectScrapInSlot(__instance);
                return false;
            }
            return true;
        }



        /// <summary>
        /// Fixes GrabObject from throwing an error if the user tries to pickup an item
        /// outside the ship, while standing inside the ship.
        /// (Basically this allows players to pickup items from outside the ship while
        /// being inside the ship itself)
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), "BeginGrabObject")]
        [HarmonyPrefix]
        private static bool GrabFix(PlayerControllerB __instance)
        {
            if (StringAddons.ConvertToBool(Meeting.inMeeting.Value)) return true;

            if (StartOfRound.Instance.shipHasLanded && GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
            {
                Ray interactRay = new Ray(__instance.gameplayCamera.transform.position, __instance.gameplayCamera.transform.forward);
                RaycastHit hit;
                int interactableObjectsMask = (int)Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("interactableObjectsMask").GetValue();

                if (!Physics.Raycast(interactRay, out hit, __instance.grabDistance, interactableObjectsMask) || hit.collider.gameObject.layer == 8 || !(hit.collider.tag == "PhysicsProp") || __instance.twoHanded || __instance.sinkingValue > 0.73f || Physics.Linecast(__instance.gameplayCamera.transform.position, hit.collider.transform.position + __instance.transform.up * 0.16f, 1073741824, QueryTriggerInteraction.Ignore))
                {
                    return false;
                }
                GrabbableObject currentlyGrabbingObject = hit.collider.transform.gameObject.GetComponent<GrabbableObject>();
                string itmName = currentlyGrabbingObject.itemProperties.itemName.ToLower().Replace("(clone)", "");

                if (StringAddons.ContainsWhitelistedItem(itmName))
                {
                    //HUDManager.Instance.AddNewScrapFoundToDisplay(currentlyGrabbingObject);
                    //HUDManager.Instance.DisplayNewScrapFound();

                    Plugin.netHandler.showScrapReceive($"{Plugin.localPlayer.playerClientId}", Plugin.localPlayer.playerClientId);

                    if (Plugin.isHost)
                    {
                        UnityEngine.Object.Destroy(currentlyGrabbingObject.gameObject);
                    }
                    else
                    {
                        string currentItem = $"{Plugin.localPlayer.playerClientId}/destroy";
                        Plugin.netHandler.destroyScrapReceive(currentItem, Plugin.localPlayer.playerClientId);
                    }

                    currentQuota.Value = $"{StringAddons.AddInts(currentQuota.Value, 10)}";
                    Plugin.terminal.groupCredits += 1;
                    Commands.DisplayChatMessage($"Collected <color=#FF0000>+1</color> credits (check shop)");
                    return false;
                }

            }
            return true;
        }




        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            StartOfRound.Instance.profitQuotaMonitorText.text = $"PROFIT QUOTA:\n${StringAddons.ConvertToInt(currentQuota.Value)} / ${maxQuota}";
        }


        /// <summary>
        /// If user attempted to drop an item then call DisplayNewScrapFound
        /// </summary>
        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPostfix]
        private static void CheckCollectItem()
        {
            if (droppedItem)
            {
                HUDManager.Instance.DisplayNewScrapFound();
                droppedItem = false;
            }
        }


        /// <summary>
        /// Remove certain items from displaying when in the ship.
        /// These will be weapons/custom items given to players.
        /// </summary>
        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.AddNewScrapFoundToDisplay))]
        [HarmonyPrefix]
        private static bool DontDisplayWeapons(HUDManager __instance, GrabbableObject GObject)
        {
            string itmName = GObject.itemProperties.itemName.ToLower().Replace("(clone)", "");
            if (__instance.itemsToBeDisplayed.Count <= 16 && StringAddons.ContainsWhitelistedItem(itmName))
            {
                __instance.itemsToBeDisplayed.Add(GObject);
            }

            return false;
        }






        #endregion Patches





        /// <summary>
        /// Get all the scrap items that spawn on this map
        /// </summary>
        public static void AppendScraps()
        {
            if (Plugin.currentRound == null) return;

            for (int i = 0; i < Plugin.currentRound.currentLevel.spawnableScrap.Count(); i++)
            {
                string scrap = Plugin.currentRound.currentLevel.spawnableScrap[i].spawnableItem.itemName;
                allScraps.Add(scrap);
            }

            if (Plugin.isHost)
            {
                Plugin.netHandler.addScrapsToListReceive(allScraps, Plugin.localPlayer.playerClientId);
            }
        }

        public static void ResetVariables()
        {
            droppedItem = false;
            currentQuota.Value = "0";
            maxQuota = 120;
            allScraps.Clear();
        }

    }
}

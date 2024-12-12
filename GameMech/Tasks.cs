using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Utils;
using UnityEngine;

namespace LethalMystery.GameMech
{

    [HarmonyPatch]
    internal class Tasks
    {
        public static bool checkingForItems = false;
        public static bool droppedItem = false;
        public static int currentQuota = 0;
        public static int maxQuota = 120;
        public static List<Item> allScraps = new List<Item>();

        #region Patches

        /// <summary>
        /// removes currentlyHeld scrap item in user's inventory if they're inside
        /// the ship. This also updates the current quota.
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void TaskUpdate(PlayerControllerB __instance)
        {
            if (StartOfRound.Instance.shipHasLanded == false && __instance.isInHangarShipRoom == false)
            {
                return;
            }
            if (checkingForItems == false)
            {
                return;
            }
            if (__instance.ItemSlots[__instance.currentItemSlot] != null)
            {
                string itmName = __instance.ItemSlots[__instance.currentItemSlot].itemProperties.itemName.ToLower().Replace("(clone)", "");
                if (StringAddons.ContainsWhitelistedItem(itmName))
                {
                    __instance.DestroyItemInSlotAndSync(__instance.currentItemSlot);
                    HUDManager.Instance.itemSlotIcons[__instance.currentItemSlot].enabled = false;
                    __instance.carryWeight = 1f;
                    checkingForItems = false;
                    currentQuota += 10;
                }
            }


        }


        /// <summary>
        /// Prevent user from dropping items in ship and collects it
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPrefix]
        private static bool CollectItem(PlayerControllerB __instance)
        {
            if (__instance.ItemSlots[__instance.currentItemSlot] == null) return true;
            if (__instance.ItemSlots[__instance.currentItemSlot].playerHeldBy.playerClientId != GameNetworkManager.Instance.localPlayerController.playerClientId) return true;

            string itmName = __instance.ItemSlots[__instance.currentItemSlot].itemProperties.itemName.ToLower().Replace("(clone)", "");
            if ((__instance.isInHangarShipRoom && StartOfRound.Instance.shipHasLanded && __instance.ItemSlots[__instance.currentItemSlot] != null)
                && StringAddons.ContainsWhitelistedItem(itmName))
            {
                HUDManager.Instance.AddNewScrapFoundToDisplay(__instance.ItemSlots[__instance.currentItemSlot]);
                droppedItem = true;
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
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.BeginGrabObject))]
        [HarmonyPrefix]
        private static bool GrabFix(PlayerControllerB __instance)
        {
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

                if ( StringAddons.ContainsWhitelistedItem(itmName) )
                {
                    HUDManager.Instance.AddNewScrapFoundToDisplay(currentlyGrabbingObject);
                    HUDManager.Instance.DisplayNewScrapFound();
                    checkingForItems = false; // Prevent TaskUpdate() from activating
                    UnityEngine.Object.Destroy(currentlyGrabbingObject.gameObject);
                    currentQuota += 10;
                    return false;
                }

            }
            return true;
        }




        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            StartOfRound.Instance.profitQuotaMonitorText.text = $"PROFIT QUOTA:\n${currentQuota} / ${maxQuota}";
        }


        /// <summary>
        /// If user attempted to drop an item then call DisplayNewScrapFound
        /// </summary>
        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
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


        /// <summary>
        /// if DisplayNewScrapFound is called then remove currently selected item
        /// </summary>
        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.DisplayNewScrapFound))]
        [HarmonyPrefix]
        private static bool DisplayItems()
        {
            checkingForItems = true; // Activate TaskUpdate()

            return true;
        }



        #endregion Patches


        public static void AppendScraps()
        {
            if (Plugin.currentRound == null) return;

            for (int i = 0; i < Plugin.currentRound.currentLevel.spawnableScrap.Count(); i++)
            {
                Item scrap = Plugin.currentRound.currentLevel.spawnableScrap[i].spawnableItem;
                allScraps.Add(scrap);
            }

        }

        public static void ResetVariables()
        {
            checkingForItems = false;
            droppedItem = false;
            currentQuota = 0;
            maxQuota = 120;
            allScraps.Clear();
        }

    }
}

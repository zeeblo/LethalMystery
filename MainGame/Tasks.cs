using System.Linq;
using System.Reflection;
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
        public static bool checkingForItems = false;
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
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void TaskUpdate(PlayerControllerB __instance)
        {
            if (StartOfRound.Instance.shipHasLanded == false && __instance.isInHangarShipRoom == false)
            {
                checkingForItems = false;
                return;
            }
            if (checkingForItems == false)
            {
                return;
            }
            if (Plugin.localPlayer.ItemSlots[Plugin.localPlayer.currentItemSlot] == null || Plugin.localPlayer.ItemSlots[Plugin.localPlayer.currentItemSlot].playerHeldBy == null)
            {
                Plugin.mls.LogInfo(">>> item is somehow null");
                checkingForItems = false;
                return;
            }
            Plugin.mls.LogInfo(">>> abv abv above taskup");
            if (Plugin.localPlayer.ItemSlots[Plugin.localPlayer.currentItemSlot].playerHeldBy.actualClientId == Plugin.localPlayer.actualClientId)
            {
                string itmName = Plugin.localPlayer.ItemSlots[Plugin.localPlayer.currentItemSlot].itemProperties.itemName.ToLower().Replace("(clone)", "");
                Plugin.mls.LogInfo(">>> above taskup");
                if (StringAddons.ContainsWhitelistedItem(itmName))
                {
                    Plugin.mls.LogInfo(">>> In TaskUpdate()");


                    if (Plugin.isHost)
                    {
                        Plugin.localPlayer.DestroyItemInSlotAndSync(Plugin.localPlayer.currentItemSlot);
                    }
                    else
                    {
                        string currentItem = $"{Plugin.localPlayer.actualClientId}/{Plugin.localPlayer.currentItemSlot}";
                        Plugin.localPlayer.DestroyItemInSlot(Plugin.localPlayer.currentItemSlot);
                        Plugin.netHandler.destroyScrapReceive(currentItem, Plugin.localPlayer.actualClientId);
                    }

                    HUDManager.Instance.itemSlotIcons[Plugin.localPlayer.currentItemSlot].enabled = false;

                    __instance.carryWeight = 1f;
                    checkingForItems = false;
                    currentQuota.Value = $"{StringAddons.AddInts(currentQuota.Value, 10)}";
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
                string currentItem = $"{Plugin.localPlayer.actualClientId}/{Plugin.localPlayer.currentItemSlot}/CollectItem";
                Plugin.netHandler.showScrapReceive(currentItem, Plugin.localPlayer.actualClientId);

                HUDManager.Instance.AddNewScrapFoundToDisplay(__instance.ItemSlots[__instance.currentItemSlot]);
                droppedItem = true; // Activates CheckCollectItem()
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

                if (StringAddons.ContainsWhitelistedItem(itmName))
                {
                    //HUDManager.Instance.AddNewScrapFoundToDisplay(currentlyGrabbingObject);
                    //HUDManager.Instance.DisplayNewScrapFound();

                    Plugin.netHandler.showScrapReceive($"{Plugin.localPlayer.actualClientId}", Plugin.localPlayer.actualClientId);
                    checkingForItems = false; // Prevent TaskUpdate() from activating

                    if (Plugin.isHost)
                    {
                        UnityEngine.Object.Destroy(currentlyGrabbingObject.gameObject);
                    }
                    else
                    {
                        string currentItem = $"{Plugin.localPlayer.actualClientId}/destroy";
                        Plugin.netHandler.destroyScrapReceive(currentItem, Plugin.localPlayer.actualClientId);
                    }

                    currentQuota.Value = $"{StringAddons.AddInts(currentQuota.Value, 10)}";

                    return false;
                }

            }
            return true;
        }




        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            StartOfRound.Instance.profitQuotaMonitorText.text = $"PROFIT QUOTA:\n${StringAddons.ConvertToInt(currentQuota.Value)} / ${maxQuota}";
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
            Plugin.mls.LogInfo(">>> DisplayItems: set to true");
            checkingForItems = true; // Activates TaskUpdate()

            return true;
        }



        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DropAllHeldItems))]
        [HarmonyPrefix]
        private static bool DropStuff(PlayerControllerB __instance, bool itemsFall, bool disconnecting)
        {
            for (int i = 0; i < __instance.ItemSlots.Length; i++)
            {
                GrabbableObject grabbableObject = __instance.ItemSlots[i];
                if (!(grabbableObject != null))
                {
                    continue;
                }
                if (!allScraps.Contains(__instance.ItemSlots[i].itemProperties.itemName))
                {
                    continue;
                }
                if (itemsFall)
                {
                    grabbableObject.parentObject = null;
                    grabbableObject.heldByPlayerOnServer = false;
                    if (__instance.isInElevator)
                    {
                        grabbableObject.transform.SetParent(__instance.playersManager.elevatorTransform, worldPositionStays: true);
                    }
                    else
                    {
                        grabbableObject.transform.SetParent(__instance.playersManager.propsContainer, worldPositionStays: true);
                    }
                    __instance.SetItemInElevator(__instance.isInHangarShipRoom, __instance.isInElevator, grabbableObject);
                    grabbableObject.EnablePhysics(enable: true);
                    grabbableObject.EnableItemMeshes(enable: true);
                    grabbableObject.transform.localScale = grabbableObject.originalScale;
                    grabbableObject.isHeld = false;
                    grabbableObject.isPocketed = false;
                    grabbableObject.startFallingPosition = grabbableObject.transform.parent.InverseTransformPoint(grabbableObject.transform.position);
                    grabbableObject.FallToGround(randomizePosition: true);
                    grabbableObject.fallTime = UnityEngine.Random.Range(-0.3f, 0.05f);
                    if (__instance.IsOwner)
                    {
                        grabbableObject.DiscardItemOnClient();
                    }
                    else if (!grabbableObject.itemProperties.syncDiscardFunction)
                    {
                        grabbableObject.playerHeldBy = null;
                    }
                }
                if (__instance.IsOwner && !disconnecting)
                {
                    HUDManager.Instance.holdingTwoHandedItem.enabled = false;
                    HUDManager.Instance.itemSlotIcons[i].enabled = false;
                    HUDManager.Instance.ClearControlTips();
                    __instance.activatingItem = false;
                }
                __instance.ItemSlots[i] = null;
            }
            if (__instance.isHoldingObject)
            {
                __instance.isHoldingObject = false;
                if (__instance.currentlyHeldObjectServer != null)
                {
                    MethodInfo SetSpecialGrabAnimationBool = typeof(PlayerControllerB).GetMethod("SetSpecialGrabAnimationBool", BindingFlags.NonPublic | BindingFlags.Instance);
                    SetSpecialGrabAnimationBool.Invoke(__instance, new object[] { true, __instance.currentlyHeldObjectServer });
                }
                __instance.playerBodyAnimator.SetBool("cancelHolding", value: true);
                __instance.playerBodyAnimator.SetTrigger("Throw");
            }
            __instance.activatingItem = false;
            __instance.twoHanded = false;
            __instance.carryWeight = 1f;
            __instance.currentlyHeldObjectServer = null;

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
                Plugin.netHandler.addScrapsToListReceive(allScraps, Plugin.localPlayer.actualClientId);
            }
        }

        public static void ResetVariables()
        {
            checkingForItems = false;
            droppedItem = false;
            currentQuota.Value = "0";
            maxQuota = 120;
            allScraps.Clear();
        }

    }
}

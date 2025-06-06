using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;
using static LethalMystery.Players.Roles;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace LethalMystery.Players
{
    [HarmonyPatch]
    internal class MoreSlots
    {

        /// <summary>
        /// Visual of extra slot
        /// </summary>
        [HarmonyPatch(typeof(HUDManager), "Awake")]
        [HarmonyPostfix]
        private static void DisplayMoreSlotsPatch(HUDManager __instance)
        {
            List<UnityEngine.UI.Image> raw_itemSlotIconFrames = new List<UnityEngine.UI.Image>();
            List<UnityEngine.UI.Image> raw_itemSlotIcons = new List<UnityEngine.UI.Image>();
            for (int i = 0; i < __instance.itemSlotIconFrames.Length; i++)
            {
                raw_itemSlotIconFrames.Add(__instance.itemSlotIconFrames[i]);
            }
            for (int i = 0; i < __instance.itemSlotIcons.Length; i++)
            {
                raw_itemSlotIcons.Add(__instance.itemSlotIcons[i]);
            }

            UnityEngine.UI.Image itemSlotIconFrames = UnityEngine.Object.Instantiate(__instance.itemSlotIconFrames[0], __instance.Inventory.canvasGroup.transform);
            UnityEngine.UI.Image itemSlotIcons =  UnityEngine.Object.Instantiate(__instance.itemSlotIcons[0], itemSlotIconFrames.transform);
            itemSlotIconFrames.transform.localPosition = new Vector3(100f, -38.5f, -3f);

            raw_itemSlotIconFrames.Add(itemSlotIconFrames);
            raw_itemSlotIcons.Add(itemSlotIcons);

            __instance.itemSlotIconFrames = raw_itemSlotIconFrames.ToArray();
            __instance.itemSlotIcons = raw_itemSlotIcons.ToArray();

        }
        

        public static void DefaultSlots()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            localPlayer.ItemSlots = new GrabbableObject[4];

            Plugin.netHandler.SlotsReceive("0/default", Plugin.localPlayer.playerClientId);
        }



        public static void SlotAmountForServer()
        {
            //Dictionary<ulong, int> slotAmt = new Dictionary<ulong, int>();
            
            if (CurrentRole?.Name == "sheriff" || CurrentRole?.Type == RoleType.monster)
            {
                //slotAmt.Add(Plugin.localPlayer.actualClientId, 5);
                ulong playerID = Plugin.localPlayer.playerClientId;
                string slot_string = $"{playerID}/slots";
                Plugin.netHandler.SlotsReceive(slot_string, Plugin.localPlayer.playerClientId);
            }

        }



        public static void SwitchToEmptySlot()
        {
            MethodInfo SwitchToItemSlot = typeof(PlayerControllerB).GetMethod("SwitchToItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);
            int result = -1;
            if (Plugin.localPlayer.ItemSlots[Plugin.localPlayer.currentItemSlot] == null || 
                !Plugin.localPlayer.ItemSlots[Plugin.localPlayer.currentItemSlot].itemProperties.itemName.ToLower().Contains(Roles.CurrentRole?.GetItem()) )
            {
                result = Plugin.localPlayer.currentItemSlot;
            }
            else
            {
                for (int i = 0; i < Plugin.localPlayer.ItemSlots.Length; i++)
                {
                    if (Plugin.localPlayer.ItemSlots[i] == null)
                    {
                        result = i;
                        break;
                    }
                }
            }

            SwitchToItemSlot.Invoke(GameNetworkManager.Instance.localPlayerController, new object[] { result, Type.Missing });

        }
    }
}

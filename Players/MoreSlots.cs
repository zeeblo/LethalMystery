using System.Reflection;
using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;

namespace LethalMystery.Players
{
    [HarmonyPatch]
    internal class MoreSlots
    {

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Awake))]
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
        

        public static void AllowMoreSlots()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            localPlayer.ItemSlots = new GrabbableObject[5];
        }


        public static void DefaultSlots()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            localPlayer.ItemSlots = new GrabbableObject[4];
        }



    }
}

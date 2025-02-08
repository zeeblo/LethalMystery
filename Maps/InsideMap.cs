using HarmonyLib;
using UnityEngine;

namespace LethalMystery.Maps
{

    [HarmonyPatch]
    internal class InsideMap
    {

        /*
        [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.TeleportPlayer))]
        [HarmonyPrefix]
        private static bool EnterFacility(EntranceTeleport __instance)
        {
            Vector3 spawn_pos = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/spawn_pos").transform.position;
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(spawn_pos);
            GameNetworkManager.Instance.localPlayerController.isInElevator = false;
            GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
            __instance.timeAtLastUse = Time.realtimeSinceStartup;
            GameNetworkManager.Instance.localPlayerController.isInsideFactory = __instance.isEntranceToBuilding;

            return false;
        }
        */

        public static void TeleportInside()
        {
            Vector3 spawn_pos = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/spawn_pos").transform.position;
            GameNetworkManager.Instance.localPlayerController.isInElevator = false;
            GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
            GameNetworkManager.Instance.localPlayerController.isInsideFactory = true;
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(spawn_pos);
        }

        public static void SpawnInterior()
        {
            Vector3 pos = new Vector3(0f, -150, 0f);
            Plugin.Instantiate(CustomLvl.CurrentInside, pos, Quaternion.identity);
        }


    }
}

using HarmonyLib;
using UnityEngine;

namespace LethalMystery.Maps
{

    [HarmonyPatch]
    internal class InsideMap
    {

        [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.TeleportPlayer))]
        [HarmonyPrefix]
        private static bool EnterFacility()
        {
            Vector3 spawn_pos = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/spawn_pos").transform.position;
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(spawn_pos);

            return false;
        }



        public static void SpawnInterior()
        {
            Vector3 pos = new Vector3(0f, -150, 0f);
            Plugin.Instantiate(CustomLvl.CurrentInside, pos, Quaternion.identity);
        }


    }
}

using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalMystery.Maps
{

    [HarmonyPatch]
    internal class InsideMap
    {


        public static void TeleportInside()
        {
            GameObject map = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/spawn_pos");
            if (map == null) return;
            Vector3 spawn_pos = map.transform.position;
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


        
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.openingDoorsSequence))]
        [HarmonyPostfix]
        private static void TeleportToDungeon()
        {
            GameObject outsideShip = GameObject.Find("Environment/HangarShip/ReverbTriggers/OutsideShip");
            GameObject enter = Plugin.Instantiate(outsideShip);
            enter.name = "ShipExit";
            enter.transform.SetParent(GameObject.Find("Environment/HangarShip").transform);
            enter.AddComponent<LMEntrance>();
            enter.GetComponent<Collider>().enabled = true;
            enter.GetComponent<AudioReverbTrigger>().enabled = false;
            enter.transform.position = outsideShip.transform.position;
        }
        


        private class LMEntrance : MonoBehaviour
        {

            public void OnTriggerEnter(Collider other)
            {
                if (!(other.tag == "Player"))
                {
                    return;
                }
                PlayerControllerB component = other.GetComponent<PlayerControllerB>();
                if (GameNetworkManager.Instance.localPlayerController != component)
                {
                    return;
                }
                component.ResetFallGravity();
                TeleportInside();
            }
        }

    }
}

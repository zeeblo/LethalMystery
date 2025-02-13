using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalMystery.Maps
{

    [HarmonyPatch]
    internal class InsideMap
    {

        private static Vector3 lll_pos = Vector3.zero;



        [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.TeleportPlayer))]
        [HarmonyPrefix]
        private static bool EntrancePatch()
        {
            if (Plugin.localPlayer.isInsideFactory)
            {
                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.openingDoorsSequence))]
        [HarmonyPostfix]
        private static void TeleportToDungeon()
        {
            TPDungeon();
            MakeLMDoorInteractive();
        }



        [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch(EntranceTeleport __instance)
        {
            if (CustomLvl.mapName.Value == "lll_map")
            {
                lll_pos = __instance.entrancePoint.position;
            }
        }



        public static void TPDungeon()
        {
            if (GameObject.Find("Environment/HangarShip/ShipExit") != null) return;
            GameObject outsideShip = GameObject.Find("Environment/HangarShip/ReverbTriggers/OutsideShip");
            GameObject enter = Plugin.Instantiate(outsideShip);
            enter.name = "ShipExit";
            enter.transform.SetParent(GameObject.Find("Environment/HangarShip").transform);
            enter.AddComponent<LMEntrance>();
            enter.GetComponent<Collider>().enabled = true;
            enter.GetComponent<AudioReverbTrigger>().enabled = false;
            enter.transform.position = outsideShip.transform.position;
        }


        public static void SpawnInterior()
        {
            if (CustomLvl.mapName.Value == "lll_map") return;
            Vector3 pos = new Vector3(0f, -150, 0f);
            Plugin.Instantiate(CustomLvl.CurrentInside, pos, Quaternion.identity);
        }


        private static void MakeLMDoorInteractive()
        {
            GameObject intr = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/exit_pos");
            if (intr == null) return;
            Sprite hoverSprite = UnityEngine.Object.FindObjectOfType<InteractTrigger>().hoverIcon;
            intr.tag = "InteractTrigger";
            intr.layer = LayerMask.NameToLayer("InteractableObject");
            InteractTrigger exitDoor = intr.AddComponent<InteractTrigger>();
            exitDoor.hoverTip = "EXIT : [LMB]";
            exitDoor.holdInteraction = true;
            exitDoor.hoverIcon = hoverSprite;
            exitDoor.holdingInteractEvent = new InteractEventFloat();
            exitDoor.onInteractEarlyOtherClients = new InteractEvent();
            exitDoor.onInteract = new InteractEvent();
            exitDoor.onInteractEarly = new InteractEvent();
            exitDoor.onStopInteract = new InteractEvent();
            exitDoor.onCancelAnimation = new InteractEvent();
            intr.AddComponent<EntranceTeleport>();
        }


        public static void TeleportInside()
        {
            Vector3 spawn_pos = Vector3.zero;
            if (CustomLvl.mapName.Value != "lll_map")
            {
                GameObject map = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/spawn_pos");
                spawn_pos = map.transform.position;
            }
            else if (CustomLvl.mapName.Value == "lll_map")
            {
                spawn_pos = lll_pos;

            }

            GameNetworkManager.Instance.localPlayerController.isInElevator = false;
            GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
            GameNetworkManager.Instance.localPlayerController.isInsideFactory = true;
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(spawn_pos);
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

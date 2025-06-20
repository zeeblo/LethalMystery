﻿using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using LethalMystery.Utils;
using LethalMystery.Players;
using LethalMystery.MainGame;
using System.Collections.Generic;

namespace LethalMystery.Maps
{

    [HarmonyPatch]
    internal class InsideMap
    {

        public static Vector3 entrance_pos = Vector3.zero;
        public static Transform exitDoor;
        public static Dictionary<string, List<string>> allVents = new Dictionary<string, List<string>>();
        public static List<Vector3> scrapLocations = new List<Vector3>();
        private static GameObject shipExit;



        public static void ResetVars()
        {
            if (shipExit != null)
            {
                Plugin.Destroy(shipExit);
            }
            if (CustomLvl.CurrentInside == null) return;
            GameObject minimapLayer = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)");

            if (minimapLayer != null )
            {
                Plugin.Destroy(minimapLayer.gameObject);
            }
        } 


        private class LMEntrance : MonoBehaviour
        {

            public void OnTriggerEnter(Collider other)
            {
                if (Meeting.inMeeting.Value == "true") return;
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


        [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.TeleportPlayer))]
        [HarmonyPrefix]
        private static bool EntrancePatch()
        {
            if (CustomLvl.CurrentInside == null) return true;
            Plugin.mls.LogInfo(">>> Attempted to teleport");
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
            //MakeLMDoorInteractive();
        }


        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.StopInteraction))]
        [HarmonyPostfix]
        private static void HoldPatch(InteractTrigger __instance)
        {

            if (__instance.currentCooldownValue > 0f)
            {
                ExitFacility();
            }
        }


        [HarmonyPatch(typeof(EntranceTeleport), "Update")]
        [HarmonyPostfix]
        private static void UpdatePatch(EntranceTeleport __instance)
        {

            entrance_pos = __instance.entrancePoint.position;
            exitDoor = __instance.entrancePoint;
        }


        private static void ExitFacility()
        {
            if (CustomLvl.CurrentInside == null) return;
            if (GOTools.GetObjectPlayerIsLookingAt().name.ToLower() == "exit_pos")
                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
        }


        /// <summary>
        /// Attaches to ship door and teleports player inside the facility
        /// </summary>
        public static void TPDungeon()
        {
            if (GameObject.Find("Environment/HangarShip/ShipExit") != null) return;
            if (CustomLvl.CurrentInside == null) return;
            GameObject outsideShip = GameObject.Find("Environment/HangarShip/ReverbTriggers/OutsideShip");
            GameObject enter = Plugin.Instantiate(outsideShip);
            enter.name = "ShipExit";
            enter.transform.SetParent(GameObject.Find("Environment/HangarShip").transform);
            enter.AddComponent<LMEntrance>();
            enter.GetComponent<Collider>().enabled = true;
            enter.GetComponent<AudioReverbTrigger>().enabled = false;
            enter.transform.position = outsideShip.transform.position;
            shipExit = enter;
        }


        public static void SpawnInterior()
        {
            if (CustomLvl.mapName.Value == "lll_map") return;
            if (CustomLvl.CurrentInside == null) return;
            Vector3 pos = new Vector3(0f, -150, 0f);
            Plugin.Instantiate(CustomLvl.CurrentInside, pos, Quaternion.identity);
        }


        public static void SpawnVents()
        {
            if (CustomLvl.CurrentInside == null) return;
            GameObject map_vents = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/vents");
            List<string> innerVent = new List<string>();
            allVents.Clear();
            if (map_vents == null) return;


            foreach (GameObject link in GOTools.GetAllChildren(map_vents))
            {
                if (!link.name.ToLower().Contains("link")) continue;
                foreach (GameObject link_vent in GOTools.GetAllChildren(link))
                {
                    if (!link_vent.name.ToLower().Contains("vent")) continue;

                    // Find vent model in hangarship and spawn it around the loaded map

                    Vector3 pos = new Vector3(link_vent.transform.position.x, link_vent.transform.position.y, link_vent.transform.position.z);
                    GameObject shipvent = GameObject.Find("Environment/HangarShip/VentEntrance");
                    GameObject vent = Plugin.Instantiate(shipvent, pos, link_vent.transform.localRotation);
                    vent.transform.SetParent(link_vent.transform);

                    if (link.name.StartsWith("ground"))
                    {
                        vent.transform.Find("Hinge/VentCover").GetComponent<MeshRenderer>().enabled = false;
                        vent.transform.Find("ventTunnel").GetComponent<MeshRenderer>().enabled = false;
                    }

                    vent.transform.localRotation = link_vent.transform.localRotation;
                    vent.transform.localScale = new Vector3(1f, 2f, 3f);
                    vent.layer = LayerMask.NameToLayer("ScanNode");
                    vent.AddComponent<BoxCollider>();

                    // Add custom vent component to check if user is in vent
                    //int ventIndex = StringAddons.ConvertToInt(link_vent.name.ToLower().Split("vent")[1]);
                    /*
                    string ventIndex = link_vent.name.ToLower();
                    Ability.LM_Vent ventComp = link_vent.AddComponent<Ability.LM_Vent>();
                    ventComp.thisIndex = ventIndex;
                    ventComp.parent = link.name;
                    */


                    if (Roles.CurrentRole.Type == Roles.RoleType.monster)
                    {
                        // Allow user to enter vent
                        Sprite hoverSprite = UnityEngine.Object.FindObjectOfType<InteractTrigger>().hoverIcon;
                        link_vent.tag = "InteractTrigger";
                        link_vent.layer = LayerMask.NameToLayer("InteractableObject");
                        InteractTrigger useVent = link_vent.AddComponent<InteractTrigger>();
                        useVent.hoverTip = "ENTER : [LMB]";
                        useVent.holdInteraction = true;
                        useVent.hoverIcon = hoverSprite;
                        useVent.holdingInteractEvent = new InteractEventFloat();
                        useVent.onInteractEarlyOtherClients = new InteractEvent();
                        useVent.onInteract = new InteractEvent();
                        useVent.onInteractEarly = new InteractEvent();
                        useVent.onStopInteract = new InteractEvent();
                        useVent.onCancelAnimation = new InteractEvent();
                    }


                    // Allow users to scan the vent
                    ScanNodeProperties scannode = vent.AddComponent<ScanNodeProperties>();
                    scannode.headerText = "Vent";
                    scannode.subText = "Crawl in";
                    scannode.nodeType = 1;
                    scannode.maxRange = 17;
                    scannode.minRange = 2;
                    scannode.requiresLineOfSight = true;

                    innerVent.Add(link_vent.name.ToLower());

                }
                allVents.Add(link.name.ToLower(), innerVent);
            }
        }



        public static void SpawnScrapScanPositions()
        {
            if (CustomLvl.CurrentInside == null) return;
            scrapLocations.Clear();
            GameObject scrapsParent = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/scraps");
            if (scrapsParent == null) return;

            foreach (GameObject s in GOTools.GetAllChildren(scrapsParent))
            {
                if (s.name.ToLower().Contains("scrap"))
                {
                    GOTools.AddScanNode(s, "Scrap Spawn", subText: "scraps will spawn here", maxRange: 14);
                    scrapLocations.Add(s.transform.position);
                }

                // If the parent GameObject acts as a "folder" for the actual scrap
                // nodes then iterate through those
                foreach (GameObject child in GOTools.GetAllChildren(s))
                {
                    if (child.name.ToLower().Contains("scrap"))
                    {
                        GOTools.AddScanNode(child, "Scrap Spawn", subText: "scraps will spawn here", maxRange: 14);
                        scrapLocations.Add(child.transform.position);
                    }
                }
            }
        }




        public static void SpawnGeneratorBox()
        {
            if (CustomLvl.CurrentInside == null) return;
            GameObject sabo = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/sabo");
            GameObject rawsaboBox = GameObject.Find("Environment/HangarShip/DoorGenerator");
            if (sabo == null || rawsaboBox == null) return;
            GameObject saboBox = Plugin.Instantiate(rawsaboBox, sabo.transform.position, rawsaboBox.transform.rotation);

            GOTools.AddInteractTrig(sabo, "FIX", time: 5f);

            GameObject scan_node = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/sabo/scan_node");
            if (scan_node)
            {
                GOTools.AddScanNode(scan_node, "Generator", "FIX", nodeType: 1, requiresLineOfSight: false);
            }

        }



        /// <summary>
        /// Makes the LM generated dungeon door interactable
        /// which will allow users to leave
        /// </summary>
        public static void MakeLMDoorInteractive()
        {
            if (CustomLvl.CurrentInside == null) return;
            GameObject intr = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/exit_pos");
            GameObject scan = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/exit_pos/scan_node");
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


            scan.layer = LayerMask.NameToLayer("ScanNode");
            scan.AddComponent<BoxCollider>();
            ScanNodeProperties scannode = scan.AddComponent<ScanNodeProperties>();
            scannode.headerText = "Exit";
            scannode.subText = "Return to ship";
            scannode.nodeType = 0;
            scannode.maxRange = 950;
            scannode.minRange = 2;
            scannode.requiresLineOfSight = false;

        }


        /// <summary>
        /// Instantly teleports player inside the facility when the game first starts
        /// </summary>
        public static void TeleportInside()
        {
            if (CustomLvl.CurrentInside == null)
            {
                Transform[] playerSpawnPositions = StartOfRound.Instance.playerSpawnPositions;
                Plugin.localPlayer.TeleportPlayer(playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
                return;
            }

            Vector3 spawn_pos = Vector3.zero;
            if (CustomLvl.mapName.Value != "lll_map")
            {
                GameObject map = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/spawn_pos");
                spawn_pos = map.transform.position;
            }
            else if (CustomLvl.mapName.Value == "lll_map")
            {
                spawn_pos = entrance_pos;
            }

            GameNetworkManager.Instance.localPlayerController.isInElevator = false;
            GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
            GameNetworkManager.Instance.localPlayerController.isInsideFactory = true;
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(spawn_pos);
        }


        public static void SetMinimapLayer()
        {
            if (CustomLvl.CurrentInside == null) return;
            GameObject minimapLayer = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)");
            GameObject mapRadar = new GameObject("Custom - MapRadar");
            mapRadar.transform.SetParent(minimapLayer.transform);
           RemakeMap(minimapLayer.transform, mapRadar.transform);
           ApplyRoomLayer(minimapLayer.transform);
        }


        private static void ApplyRoomLayer(Transform parent)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Contains("Custom - MapRadar")) return;
                if (StringAddons.ContainsSpecialMapID(child.name)) continue;
                child.gameObject.layer = 14;
                ApplyRoomLayer(child);
            }
        }


        private static void RemakeMap(Transform parent, Transform radarParent)
        {
            foreach (Transform child in parent)
            {
                if (StringAddons.ContainsSpecialMapID(child.name) || child.name.Contains("Custom - MapRadar")) continue;
                GameObject mapRadarObj = Plugin.Instantiate(child.gameObject);
                mapRadarObj.transform.SetParent(radarParent.transform);
                //mapRadarObj.transform.position = new Vector3(child.position.x, -150, child.position.z);
            }
            radarParent.transform.localScale = Vector3.one;
            radarParent.transform.position = new Vector3(0, -150, 0);
        }



        /// <summary>
        /// Give exit door a scan node
        /// </summary>
        public static void SetEntranceScan()
        {
            GameObject scan_node = exitDoor.gameObject;
            if (scan_node)
            {
                GOTools.AddScanNode(scan_node, "Exit", "Return to ship", nodeType: 0, requiresLineOfSight: false, maxRange: 9950);
            }
        }



    }
}

using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Players;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace LethalMystery.Utils
{
    public class GOTools
    {


        /// <summary>
        /// If their weapon is for some reason not spawned in their inventory
        /// notify them in chat on how to spawn it
        /// </summary>
        public static void CheckForWeaponInInventoryNotif()
        {
            if (Roles.CurrentRole == null) return;

            if (Roles.CurrentRole.Type == Roles.RoleType.monster || Roles.CurrentRole.Name == "sheriff")
            {
                bool weaponFound = GOTools.CheckForExistingWeapon();
                if (!weaponFound)
                {
                    Commands.DisplayChatMessage($"Press <color=#FF0000>\"{LMConfig.spawnItemBind.Value.ToUpper()}\"</color> to spawn and equip your weapon. (this takes a few seconds)");
                }
            }
        }

        public static bool CheckForExistingWeapon()
        {
            for (int i = 0; i < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; i++)
            {
                if (GameNetworkManager.Instance.localPlayerController.ItemSlots[i] != null)
                {
                    if ((GameNetworkManager.Instance.localPlayerController.ItemSlots[i].name.ToLower().Contains("shotgun")) || (GameNetworkManager.Instance.localPlayerController.ItemSlots[i].name.ToLower().Contains("knife")))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Get child objects of GameObject
        /// </summary>
        public static List<GameObject> GetAllChildren(GameObject parent)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject);
            }
            return children;
        }


        public static void ClearInventory()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            for (int i = 0; i < localPlayer.ItemSlots.Count(); i++)
            {
                if (localPlayer.ItemSlots[i] != null)
                {
                    localPlayer.DestroyItemInSlotAndSync(i);
                    HUDManager.Instance.itemSlotIcons[i].enabled = false;
                }

            }
        }


        /// <summary>
        /// Fix any left over icons that shouldn't be displayed when the round ends
        /// </summary>
        public static void CleanSlot()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            for (int i = 0; i < localPlayer.ItemSlots.Count(); i++)
            {
                HUDManager.Instance.itemSlotIcons[i].enabled = false;
            }
        }


        public static void RemoveItem(int slot)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            GrabbableObject grabbableObject = localPlayer.ItemSlots[slot];
            localPlayer.currentlyHeldObjectServer.DiscardItemOnClient();
            localPlayer.currentlyHeldObjectServer = null;
            localPlayer.ItemSlots[slot] = null;

            //Plugin.netHandler.destroyScrapReceive(grabbableObject, Plugin.localPlayer.actualClientId);
        }


        /// <summary>
        /// Convert Ulong ID to PlayerID, if there's no match
        /// then -1 indicates it's an invalid id.
        /// </summary>
        public static int UlongToPlayerID(ulong id)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.actualClientId == id)
                {
                    return (int)player.playerClientId;
                }
            }
            return -1;
        }


        /// <summary>
        /// Returns the GameObject the user is currently looking at
        /// </summary>
        public static GameObject GetObjectPlayerIsLookingAt()
        {
            if (Plugin.localPlayer == null) return Plugin.shipInstance.gameObject;

            Camera gameplayCamera = Plugin.localPlayer.gameplayCamera;
            float grabDistance = Plugin.localPlayer.grabDistance;
            bool twoHanded = Plugin.localPlayer.twoHanded;
            float sinkingValue = Plugin.localPlayer.sinkingValue;
            Transform transform = Plugin.localPlayer.transform;

            Ray interactRay = new Ray(gameplayCamera.transform.position, gameplayCamera.transform.forward);
            RaycastHit hit;
            int interactableObjectsMask = (int)Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("interactableObjectsMask").GetValue();

            if (!Physics.Raycast(interactRay, out hit, grabDistance, interactableObjectsMask) || hit.collider.gameObject.layer == 8 || !(hit.collider.tag == "PhysicsProp") || twoHanded || sinkingValue > 0.73f || Physics.Linecast(gameplayCamera.transform.position, hit.collider.transform.position + transform.up * 0.16f, 1073741824, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != null)
                {
                    //Plugin.mls.LogInfo($">>> Looking at {hit.collider.transform.gameObject.name}");
                    return hit.collider.transform.gameObject;
                }
                return Plugin.shipInstance.gameObject;
            }

            return hit.collider.transform.gameObject;
        }



        public static void RemoveGameObject(string tag, string name)
        {
            GameObject[] go = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in go)
            {
                string new_name = name.ToLower().Replace("(clone)", "");
                string go_name = obj.name.ToLower().Replace("(clone)", "");
                if (new_name == go_name)
                {
                    Plugin.Destroy(obj);
                }
            }
        }



        public static void HideEnvironment(bool value, string ignore = "")
        {
            GameObject.Find("OutOfBoundsTerrain")?.gameObject?.SetActive(!value);
            Scene currentScene = SceneManager.GetSceneAt(1);

            foreach (GameObject obj in currentScene.GetRootGameObjects())
            {
                if (obj.name == "Environment")
                {
                    foreach(GameObject child in GetAllChildren(obj))
                    {
                        if (child.name == ignore)
                        {
                            child.SetActive(value);
                            continue;
                        } 
                        child.SetActive(!value);
                    }
                }
            }
        }


        public static void HideDungeon(bool value = true)
        {
            Scene currentScene = SceneManager.GetSceneAt(1);
            foreach (GameObject obj in currentScene.GetRootGameObjects())
            {
                if (obj.name == "Systems")
                {
                    obj.transform.Find("LevelGeneration").gameObject.SetActive(!value);
                }
            }
        }


        public static void HidePlayerModel(bool value = true)
        {
            Plugin.localPlayer.thisPlayerModelArms.gameObject.SetActive(!value);
            Plugin.localPlayer.thisPlayerModelLOD1.gameObject.SetActive(!value);
            Plugin.localPlayer.thisPlayerModelLOD2.gameObject.SetActive(!value);
            Plugin.localPlayer.playerGlobalHead.gameObject.SetActive(!value);
            Plugin.localPlayer.headCostumeContainer.gameObject.SetActive(!value);
            Plugin.localPlayer.headCostumeContainerLocal.gameObject.SetActive(!value);
            Plugin.localPlayer.playerModelArmsMetarig.gameObject.SetActive(!value);
            Plugin.localPlayer.playerBadgeMesh.gameObject.SetActive(!value);
            Plugin.localPlayer.playerBetaBadgeMesh.gameObject.SetActive(!value);
            Plugin.localPlayer.localVisor.gameObject.SetActive(!value);

            if (Plugin.localPlayer.currentlyHeldObjectServer == null) return;
            if (Plugin.localPlayer.currentlyHeldObjectServer.gameObject.GetComponent<MeshRenderer>() == null) return;

            Plugin.localPlayer.currentlyHeldObjectServer.gameObject.GetComponent<MeshRenderer>().enabled = !value;

        }


        public static void TurnLightsRed(bool value = true)
        {
            GameObject lights = GameObject.Find("Environment/HangarShip/ShipElectricLights");
            foreach (GameObject l in GetAllChildren(lights))
            {
                if (l.GetComponent<Light>() != null)
                {
                    l.GetComponent<Light>().color = (value) ? Color.red : Color.white;
                }
            }
        }



    }
}

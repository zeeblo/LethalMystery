using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalMystery.Utils
{
    public class GOTools
    {
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



        /*
        public static GameObject GetHangarShipInstance()
        {
            Scene SampleScene = SceneManager.GetSceneAt(0);
            GameObject gameobj = GameObject.Find("");
            List<GameObject> allObjs = new List<GameObject>();
            foreach (GameObject obj in SampleScene.GetRootGameObjects())
            {
                if (obj.name.ToLower() == "environment")
                {
                    allObjs = GetAllChildren(obj);
                    break;
                }
            }

            foreach (GameObject envChild in allObjs)
            {
                if (envChild.name.ToLower() == "hangarship")
                {
                    gameobj = envChild;
                    break;
                }
            }
            return gameobj;
        }
        */
    }
}

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

    }
}

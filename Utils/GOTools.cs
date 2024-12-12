using UnityEngine;

namespace LethalMystery.Utils
{
    public class GOTools
    {
        /// <summary>
        /// Get child objects of GameObject
        /// </summary>
        public static List<GameObject> GetAllChildren(GameObject parent)
        {
            List <GameObject> children = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject);
            }
            return children;
        }
    }
}

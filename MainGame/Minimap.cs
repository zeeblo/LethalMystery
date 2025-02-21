using UnityEngine.EventSystems;
using UnityEngine;
using LethalMystery.UI;

namespace LethalMystery.MainGame
{
    internal class Minimap
    {

        public class MinimapClickHandler : MonoBehaviour, IPointerClickHandler
        {
            //public RectTransform minimapRect; // The UI RectTransform of the minimap
            public Camera mapCam; // The camera rendering the minimap
            public Transform player; // The player's transform

            private GameObject currentMarker;

            public void OnPointerClick(PointerEventData eventData)
            {
                Plugin.mls.LogInfo(">>> Click Clack");
                RectTransform minimapRect = MinimapUI.minimap.GetComponent<RectTransform>();
                mapCam = GameObject.Find("Systems/GameSystems/ItemSystems/MapCamera").GetComponent<Camera>();
                player = Plugin.localPlayer.transform;
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, eventData.position, eventData.pressEventCamera, out localPoint);


                Vector2 normalizedPoint = new Vector2(
                    (localPoint.x / minimapRect.sizeDelta.x) + 0.5f,
                    (localPoint.y / minimapRect.sizeDelta.y) + 0.5f
                );

                // Convert minimap coordinates to world space
                Vector3 worldPosition = MinimapToWorld(normalizedPoint);

                if (currentMarker != null)
                {
                    Destroy(currentMarker);
                }

                currentMarker = Instantiate(MinimapUI.markerDot, worldPosition, Quaternion.identity);
            }

            private Vector3 MinimapToWorld(Vector2 minimapPoint)
            {
                // Get the minimap's camera world bounds
                Vector3 center = player.position;
                float camSize = mapCam.orthographicSize;
                float aspect = mapCam.aspect;

                // Convert minimap coordinates to world coordinates
                float worldX = center.x + (minimapPoint.x - 0.5f) * camSize * 2 * aspect;
                float worldZ = center.z + (minimapPoint.y - 0.5f) * camSize * 2;

                return new Vector3(worldX, player.position.y, worldZ);
            }
        }

    }
}

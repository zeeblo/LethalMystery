using UnityEngine.EventSystems;
using UnityEngine;
using LethalMystery.UI;

namespace LethalMystery.MainGame
{
    internal class Minimap
    {
        public class MinimapWaypoint: MonoBehaviour, IPointerClickHandler
        {

            public Camera minimapCamera;
            public Transform playerTransform;
            public GameObject waypointPrefab;
            public RectTransform minimapRectTransform;

            public float minimapSize = 448f; // World size represented by the minimap
            private GameObject currentWaypoint;
            //private LayerMask Room;



            public void OnPointerClick(PointerEventData eventData)
            {
                Plugin.mls.LogInfo(">>> Clack Clack CLick CLick");

                // Check if click was on the minimap
                if (RectTransformUtility.RectangleContainsScreenPoint(minimapRectTransform, eventData.position, eventData.pressEventCamera))
                {
                    Plugin.mls.LogInfo(">>> 1");
                    // Convert screen position to local point in rectangle
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        minimapRectTransform,
                        eventData.position,
                        eventData.pressEventCamera,
                        out Vector2 localPoint);

                    // Calculate the normalized position (0-1) within the minimap
                    Vector2 normalizedPos = new Vector2(
                        (localPoint.x + minimapRectTransform.rect.width * 0.5f) / minimapRectTransform.rect.width,
                        (localPoint.y + minimapRectTransform.rect.height * 0.5f) / minimapRectTransform.rect.height
                    );

                    Plugin.mls.LogInfo(">>> 2");
                    // Convert to world position relative to minimap camera's view
                    Vector3 worldPos = CalculateWorldPosition(normalizedPos);

                    // Optional: Perform raycast to ensure the waypoint is placed on valid ground
                    /*
                    RaycastHit hit;
                    if (Physics.Raycast(new Vector3(worldPos.x, 1000f, worldPos.z), Vector3.down, out hit, 2000f, Room))
                    {
                        worldPos = hit.point;
                    }
                    */
                    Plugin.mls.LogInfo(">>> 4");
                    PlaceWaypoint(worldPos);

                    Plugin.mls.LogInfo($"Placing waypoint at: {worldPos}, Camera at: {minimapCamera.transform.position}, Click pos normalized: {normalizedPos}");
                }
            }


            private Vector3 CalculateWorldPosition(Vector2 normalizedPos)
            {
                Vector3 viewportPoint = new Vector3(normalizedPos.x, normalizedPos.y, minimapCamera.nearClipPlane);
                Vector3 worldPos = minimapCamera.ViewportToWorldPoint(viewportPoint);
                worldPos.y = playerTransform.position.y;

                return worldPos;
            }



            private void PlaceWaypoint(Vector3 worldPosition)
            {
                Plugin.mls.LogInfo(">>> 5");

                if (currentWaypoint != null)
                {
                    Plugin.mls.LogInfo(">>> 6");
                    Destroy(currentWaypoint);
                    Plugin.mls.LogInfo(">>> 7");
                }

                Plugin.mls.LogInfo(">>> 8");

                currentWaypoint = Instantiate(waypointPrefab, worldPosition, Quaternion.identity);


            }

            // For manual control of minimap zoom level
            public void SetMinimapZoom(float zoomLevel)
            {
                minimapSize = zoomLevel;
            }

            // Optional: Method to clear the waypoint
            public void ClearWaypoint()
            {
                if (currentWaypoint != null)
                {
                    Destroy(currentWaypoint);
                    currentWaypoint = null;
                }
            }
        }

    }
}

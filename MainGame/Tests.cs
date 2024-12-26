using HarmonyLib;
using LethalNetworkAPI;
using UnityEngine;
using UnityEngine.InputSystem;



namespace LethalMystery.MainGame
{

    internal class Tests
    {
        public static Vector3 modelPosition = new Vector3(80f, 20f, 20f);
        public static Vector3 groundPosition = new Vector3(80f, 18.8f, 20f);
        public static Vector3 cameraPosition = new Vector3(80f, 31.05f, 26f);
        public static Vector3 lightPosition = new Vector3(80f, 34f, 20f);
        public static GameObject? playerObj;
        public static GameObject? lght;
        public static Camera? introCamera;
        public static bool entered = false;
        public static bool entered6 = false;
        public static bool disableMovement = false;
        private static GameObject? HeaderImage;
        private static GameObject? BannerImage;
        private static GameObject? BGCanvas;




        [HarmonyPatch]
        internal class AdminCMDS
        {


        }




        [HarmonyPatch(typeof(HUDManager))]
        internal class AdminCMDS_2
        {
            [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
            [HarmonyPostfix]
            private static void Keys(HUDManager __instance)
            {
                if (Keyboard.current.digit3Key.wasPressedThisFrame)
                {
                    Commands.SpawnScrapFunc("metalsheet", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }
                if (Keyboard.current.digit4Key.wasPressedThisFrame)
                {
                    Commands.SpawnScrapFunc("ring", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }
                if (Keyboard.current.digit5Key.wasPressedThisFrame)
                {
                    Plugin.mls.LogInfo($">>> Role is: {Plugin.netHandler.GetallPlayerRoles().Name}");

                    Plugin.mls.LogInfo($">>> InMetting Value: {Plugin.inMeeting.Value}");
                    Plugin.mls.LogInfo($">>> inGracePeriod Value: {Plugin.inGracePeriod.Value}");
                    Plugin.mls.LogInfo($">>> currentGracePeriodCountdown Value: {Plugin.currentGracePeriodCountdown.Value}");
                    Plugin.mls.LogInfo($">>> currentMeetingCountdown Value: {Plugin.currentMeetingCountdown.Value}");
                    Plugin.mls.LogInfo($">>> MeetingCooldown Value: {Plugin.MeetingCooldown.Value}");
                    
                }

            }
        }
    }
}

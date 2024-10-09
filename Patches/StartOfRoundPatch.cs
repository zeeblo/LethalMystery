using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;



namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {

        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        private static void StartPatch(StartOfRound __instance)
        {

            for (int k = 0; k < __instance.unlockablesList.unlockables.Count; k++)
            {
                Plugin.mls.LogInfo(">>> Unlockable Name ES: " + __instance.unlockablesList.unlockables[k].unlockableName);
                if (__instance.unlockablesList.unlockables[k].unlockableName.Contains("Loud"))
                {
                    //__instance.SpawnUnlockable(k);

                    SpawnHorn(
                        __instance : __instance,
                        prefab : __instance.unlockablesList.unlockables[k].prefabObject,
                        pos : __instance.elevatorTransform.position,
                        unlockableIndex : k
                        );
                }
            }

        }


        private static void SpawnHorn(StartOfRound __instance, GameObject prefab, Vector3 pos, int unlockableIndex)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab, pos, Quaternion.identity, null);

            if (!gameObject.GetComponent<NetworkObject>().IsSpawned)
            {
                gameObject.GetComponent<NetworkObject>().Spawn();
            }
            if (gameObject != null)
            {
                __instance.SpawnedShipUnlockables.Add(unlockableIndex, gameObject);
            }
        }
    }
}

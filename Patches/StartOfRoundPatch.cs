using HarmonyLib;
using Unity.Netcode;
using UnityEngine;




namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {

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


        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        private static void StartPatch(StartOfRound __instance)
        {

            // Spawn the meeting horn
            for (int k = 0; k < __instance.unlockablesList.unlockables.Count; k++)
            {
                if (__instance.unlockablesList.unlockables[k].unlockableName.Contains("Loud"))
                {

                    SpawnHorn(
                        __instance : __instance,
                        prefab : __instance.unlockablesList.unlockables[k].prefabObject,
                        pos : __instance.elevatorTransform.position,
                        unlockableIndex : k
                        );
                }
            }
        }


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SetPlanetsWeather))]
        [HarmonyPrefix]
        private static bool NoBadWeather(ref SelectableLevel[] ___levels)
        {
            for (int i = 0; i < ___levels.Length; i++)
            {
                ___levels[i].currentWeather = LevelWeatherType.None;
                if (___levels[i].overrideWeather)
                {
                    ___levels[i].currentWeather = ___levels[i].overrideWeatherType;
                }
            }
            return false;
        }



    }
}

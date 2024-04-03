using HarmonyLib;
using LethalMystery.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;



namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        private static AssignmentUI? _assignmentUI;

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
        [HarmonyPrefix]
        private static void StartPatch()
        {
            _assignmentUI = new GameObject("UI").AddComponent<AssignmentUI>();
            /*
            Scene targetScene = SceneManager.GetSceneByName("DontDestroyOnLoad");

            foreach (GameObject parentObject in targetScene.GetRootGameObjects())
            {
                if (parentObject.name == "UI")
                {
                    if (_assignmentUI is not null)
                        _assignmentUI.transform.SetParent(parentObject.transform);
                    break;
                }
            }
            */

            // _assignmentUI = new GameObject("UI").AddComponent<AssignmentUI>();
            //((Component)_assignmentUI).transform.SetParent(((Component)this).transform);

            AssignmentUI.SetAssignment("Monster");
        }

    }
}

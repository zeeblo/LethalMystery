using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatch
    {

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Update))]
        [HarmonyPrefix]
        private static void UpdatePatch(GrabbableObject __instance)
        {
        }


    }
}

using LethalCompanyInputUtils.Api;
using LethalMystery.MainGame;
using LethalMystery.Players.Abilities;
using LethalMystery.UI;
using LethalMystery.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalMystery.Players
{
    public class Controls
    {
        private static bool spawningWeapon = false;
        public static bool cleaningBody = false;


        public class PlayerBinds : LcInputActions
        {
            [InputAction("<Keyboard>/v", Name = "Show Vote")]
            public InputAction showVote { get; set; }

            [InputAction("<Keyboard>/m", Name = "Show Map")]
            public InputAction showMap { get; set; }

            [InputAction("<Keyboard>/t", Name = "Spawn Item")]
            public InputAction spawnItem { get; set; }

            [InputAction("<Keyboard>/f", Name = "Self Clean", ActionType = InputActionType.Value)]
            public InputAction selfclean { get; set; }

        }


        public static void InitControls()
        {

            Plugin.actionInstance.showVote.performed += ShowVote_performed;
            Plugin.actionInstance.showMap.performed += ShowMap_performed;
            Plugin.actionInstance.spawnItem.performed += SpawnItem_performed;
            Plugin.actionInstance.selfclean.performed += Selfclean_performed;
            Plugin.actionInstance.selfclean.canceled += Selfclean_canceled;
        }



        public static void ResetVars()
        {
            cleaningBody = false;
            spawningWeapon = false;
        }

        public static void UnlockCursor(bool value)
        {
            Cursor.lockState = (value) ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.SetCursor(HUDManager.Instance.defaultCursorTex, new Vector2(0f, 0f), CursorMode.Auto);
            if (!StartOfRound.Instance.localPlayerUsingController)
            {
                Cursor.visible = value;
            }
        }





        private static void ShowVote_performed(InputAction.CallbackContext obj)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (Plugin.localPlayer == null) return;
            if (Plugin.localPlayer.quickMenuManager.isMenuOpen || Plugin.terminal.terminalInUse || Plugin.localPlayer.isTypingChat) return;
            if (VotingUI.votingGUI == null) return;
            if (StringAddons.ConvertToBool(Meeting.inMeeting.Value) == false || StringAddons.ConvertToBool(EjectPlayers.currentlyEjectingPlayer.Value) == true) return;

            ShowVoteUI();
        }

        public static void ShowVoteUI()
        {
            VotingUI.votingGUI.SetActive(!VotingUI.votingGUI.activeSelf);
            UnlockCursor(VotingUI.votingGUI.activeSelf);
            Plugin.mls.LogInfo(">>> Opened Vote Menu");
        }


        private static void ShowMap_performed(InputAction.CallbackContext obj)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (Plugin.localPlayer == null) return;
            if (Plugin.localPlayer.quickMenuManager.isMenuOpen || Plugin.terminal.terminalInUse || Plugin.localPlayer.isTypingChat) return;

            if (MinimapUI.minimap == null)
            {
                MinimapUI.CreateMinimap();
                UnlockCursor(true);
                return;
            }

            MinimapUI.border.SetActive(!MinimapUI.border.activeSelf);
            UnlockCursor(MinimapUI.border.activeSelf);
        }

        private static void SpawnItem_performed(InputAction.CallbackContext context)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (Plugin.localPlayer == null) return;
            if (Plugin.localPlayer.quickMenuManager.isMenuOpen || Plugin.terminal.terminalInUse || Plugin.localPlayer.isTypingChat) return;
            if (Plugin.localPlayer.isPlayerDead) return;
            if (Roles.CurrentRole == null) return;
            if (Roles.CurrentRole.Type == Roles.RoleType.employee ) return;

            StartOfRound.Instance.StartCoroutine(SpawnWeapon());
        }



        private static void Selfclean_performed(InputAction.CallbackContext context)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (Plugin.localPlayer == null) return;
            if (Plugin.localPlayer.quickMenuManager.isMenuOpen || Plugin.terminal.terminalInUse || Plugin.localPlayer.isTypingChat) return;
            if (Plugin.localPlayer.isPlayerDead) return;
            if (Roles.CurrentRole == null) return;
            if (Roles.CurrentRole.Type != Roles.RoleType.monster) return;

            Plugin.mls.LogInfo(">> Cleaning Blood");
            cleaningBody = true;
            HUDManager.Instance.holdInteractionCanvasGroup.alpha = 1;
        }

        private static void Selfclean_canceled(InputAction.CallbackContext context)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (Plugin.localPlayer == null) return;
            if (Plugin.localPlayer.quickMenuManager.isMenuOpen || Plugin.terminal.terminalInUse || Plugin.localPlayer.isTypingChat) return;
            if (Plugin.localPlayer.isPlayerDead) return;
            if (Roles.CurrentRole == null) return;
            if (Roles.CurrentRole.Type != Roles.RoleType.monster) return;

            Plugin.mls.LogInfo(">> Stopped cleaning");
            StopCleaning();
        }


        public static void StopCleaning()
        {
            cleaningBody = false;
            CleanSuit.cleaningBloodAmt = 0f;
            HUDManager.Instance.holdFillAmount = 0f;
            HUDManager.Instance.holdInteractionCanvasGroup.alpha = 0;
        }


        private static void Shapeshift_performed(InputAction.CallbackContext context)
        {
            Plugin.mls.LogInfo(">> Shapeshifted!!");
        }



        private static IEnumerator SpawnWeapon()
        {
            if (spawningWeapon == false && Roles.CurrentRole != null)
            {
                bool weaponInInventory = GOTools.CheckForExistingWeapon();

                if (weaponInInventory)
                {
                    HUDManager.Instance.DisplayTip("Weapon Detected!", "You already have a weapon in your inventory.", isWarning: true);
                    yield break;
                }
                    

                spawningWeapon = true;
                AutoGiveItem.ResetVariables();
                Plugin.netHandler.SpawnWeaponReceive($"{Roles.CurrentRole.Type}/{Roles.CurrentRole.Name}", Plugin.localPlayer.playerClientId);

                yield return new WaitForSeconds(5f); // butler takes roughly 2.5s or less while nutcracker takes a bit longer
                AutoGiveItem.doneSpawningWeapons = true;

                yield return new WaitForSeconds(8);
                AutoGiveItem.doneSpawningWeapons = false;
                spawningWeapon = false;
            }
        }


    }
}

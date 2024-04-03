using LethalMystery.Utils;
using UnityEngine;

namespace LethalMystery.Players
{
    public class Roles
    {


        public string? TopText;
        public string? BottomText;
        private AssignmentUI? _assignmentUI;



        public void displayRole(string role)
        {
            Debug.Log(">>>>>>>>>EEEEEEEEEEEEEEEEE");
            if (_assignmentUI is not null)
            {
                //TopText = RoleAttrs[role]["topText"];
                //BottomText = RoleAttrs[role]["bottomText"];

                if (Plugin.Instance is not null)
                    if (Plugin.Instance.logger is not null)
                //Plugin.Instance?.logger.LogInfo($"TopText: {TopText}\nBottomText: {BottomText} ");
                Debug.Log(">>>>>>>>>EEEEEEEEEEEEEEEEE");

                //_assignmentUI.SetAssignment(role);
                
            }
        }


    }
}
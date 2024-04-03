using LethalMystery.Utils;
using UnityEngine;

namespace LethalMystery.Players
{
    public class Roles
    {


        private static readonly Dictionary<string, Dictionary<string, string>> RoleAttrs = new Dictionary<string, Dictionary<string, string>>
        {
            {"Employee", new Dictionary<string, string>
                {
                    {"topText", "Employee"},
                    {"bottomText", "Do all your tasks"}
                }
            },
            {"Monster", new Dictionary<string, string>
                {
                    {"topText", "Monster"},
                    {"bottomText", "Eliminate all the crew"}
                }
            },
            {"Shapeshifter", new Dictionary<string, string>
                {
                    {"topText", "Shapeshifter"},
                    {"bottomText", "Eliminate all the crew and press x to shapeshift"}
                }
            },
            {"Sherif", new Dictionary<string, string>
                {
                    {"topText", "Shapeshifter"},
                    {"bottomText", "Eliminate all the crew and press x to shapeshift"}
                }
            }
        };

        public static string? TopText;
        public static string? BottomText;

        public static void AssignRole(string role)
        {
            Debug.Log(">>>>>>>>>EEEEEEEEEEEEEEEEE");

            TopText = RoleAttrs[role]["topText"];
            BottomText = RoleAttrs[role]["bottomText"];

            AssignmentUI.SetAssignment(RoleAttrs[role]);
        }


    }
}
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
                    {"role", "Employee"},
                    {"desc", "Do all your tasks"},
                    {"type", "employee"}
                }
            },
            {"Sherif", new Dictionary<string, string>
                {
                    {"role", "Shapeshifter"},
                    {"desc", "Eliminate all the crew and press x to shapeshift"},
                    {"type", "employee"}
                }
            },
            {"Monster", new Dictionary<string, string>
                {
                    {"role", "Monster"},
                    {"desc", "Eliminate all the crew"},
                    {"type", "monster"}

                }
            },
            {"Shapeshifter", new Dictionary<string, string>
                {
                    {"role", "Shapeshifter"},
                    {"desc", "Eliminate all the crew and press x to shapeshift"},
                    {"type", "monster"}
                }
            }
        };

        public static string? TopText;
        public static string? BottomText;

        public static void AssignRole(string role)
        {
            TopText = RoleAttrs[role]["role"];
            BottomText = RoleAttrs[role]["desc"];
            AssignmentUI.SetAssignment(RoleAttrs[role]);
            ShowRole(role);
        }


        public static void ShowRole(string role)
        {
            DialogueSegment[] array = (DialogueSegment[])(object)new DialogueSegment[1]
            {
                    new DialogueSegment
                    {
                        speakerText = RoleAttrs[role]["role"],
                        bodyText = RoleAttrs[role]["desc"],
                        waitTime = 14f
                    }
            };
            HUDManager.Instance.ReadDialogue(array);
        }


    }
}
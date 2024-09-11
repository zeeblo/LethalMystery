using UnityEngine;

namespace LethalMystery.Players
{
    public class Roles
    {


        public static readonly Dictionary<string, Dictionary<string, string>> RoleAttrs = new Dictionary<string, Dictionary<string, string>>
        {
            {"Employee", new Dictionary<string, string>
                {
                    {"role", "Employee"},
                    {"desc", "Bring back items to the ship to meet the quota."},
                    {"type", "employee"}
                }
            },
            {"Sherif", new Dictionary<string, string>
                {
                    {"role", "Sherif"},
                    {"desc", "Kill the monster(s). Guessing wrong will get you killed."},
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
            },
            {"Necromancer", new Dictionary<string, string>
                {
                    {"role", "Necromancer"},
                    {"desc", "Spawn monsters to eliminate all the crew. Press x to spawn"},
                    {"type", "monster"}
                }
            }
        };

        public static string[] availableRoles = { "Employee", "Monster"};
        public static string? TopText;
        public static string? BottomText;
        public static string CurrentRole = "";



        public static void AssignRole()
        {
            System.Random randomNum = new System.Random();
            int index = randomNum.Next(0, availableRoles.Length);
            string role = availableRoles[index];

            TopText = RoleAttrs[role]["role"];
            BottomText = RoleAttrs[role]["desc"];
            CurrentRole = role;
            ShowRole(role);
        }


        public static void ShowRole(string role)
        {
            DialogueSegment[] array = new DialogueSegment[]
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
using Facepunch.Extend;
using RustAsia.Extensions;
using RustAsia.System;
using System;
using UnityEngine;

namespace RustAsia.Admin
{
    public static class Commands
    {
        internal static void InsertCommands()
        {
            foreach (var cmd in NewCommands)
            {
                ConsoleSystem.Index.Server.Dict.Add(cmd.FullName, cmd);
            }
        }

        public static ConsoleSystem.Command[] NewCommands = new ConsoleSystem.Command[]
        {
            new ConsoleSystem.Command
            {
                Name = "freebuild",
                Parent = "global",
                FullName = "global.freebuild",
                ServerAdmin = true,
                Variable = true,
                GetOveride = (() => Variables.FreeBuild.ToString()),
                SetOveride = delegate (string str)
                {
                    Variables.FreeBuild = str.ToBool();
                }
            },
            new ConsoleSystem.Command
            {
                Name = "invisible",
                Parent = "global",
                FullName = "global.invisible",
                ServerAdmin = true,
                Variable = false,
                Call = delegate (ConsoleSystem.Arg arg)
                {
                    var basePlayer = arg.Player();
                    if (basePlayer != null)
                    {
                        if (!basePlayer.IsInvisible)
                        {
                            BasePlayerExt.Disappear(basePlayer);
                        }
                        else
                        {
                            BasePlayerExt.Reappear(basePlayer);
                        }
                        arg.ReplyWith(string.Format("You are now: {0}", basePlayer.IsInvisible ? "Invisible" : "Visible"));
                    }
                }
            },
            new ConsoleSystem.Command
            {
                Name = "esp",
                Parent = "global",
                FullName = "global.esp",
                ServerAdmin = true,
                Variable = false,
                Call = delegate (ConsoleSystem.Arg arg)
                {
                    BasePlayer basePlayer = arg.Player();
                    basePlayer.ESPOn = !basePlayer.ESPOn;
                    basePlayer.ChatMessage(string.Format("ESP {0}", basePlayer.ESPOn ? "on" : "off"));
                }
            },
            new ConsoleSystem.Command
            {
                Name = "admin_no_target",
                Parent = "global",
                FullName = "global.admin_no_target",
                ServerAdmin = true,
                Variable = true,
                GetOveride = (() => Variables.AdminNoTarget.ToString()),
                SetOveride = delegate (string str)
                {
                    Variables.AdminNoTarget = str.ToBool();
                }
            },
            new ConsoleSystem.Command
            {
                Name = "default_building_grade",
                Parent = "global",
                FullName = "global.default_building_grade",
                ServerAdmin = true,
                Variable = false,
                Call = delegate (ConsoleSystem.Arg arg)
                {
                    BasePlayer ply = arg.Player();
                    if (ply != null)
                    {
                        if (arg.Args.Length != 0)
                        {
                            object p = Enum.Parse(typeof(BuildingGrade.Enum), arg.Args[0]);
                            if (p != null)
                            {
                                ply.FreeBuildType = (BuildingGrade.Enum)p;
                                arg.ReplyWith(ply.FreeBuildType.ToString());
                                return;
                            }
                            arg.ReplyWith(string.Format("Cant find building grade {0}, must be {1}", arg.Args[0], string.Join(", ", new object[]
                            {
                                    Enum.GetValues(typeof(BuildingGrade.Enum))
                            })));
                            return;
                        }
                        else
                        {
                            arg.ReplyWith(ply.FreeBuildType.ToString());
                        }
                    }
                }
            },
            new ConsoleSystem.Command
            {
                Name = "lootable_admins",
                Parent = "global",
                FullName = "global.lootable_admins",
                ServerAdmin = true,
                Variable = true,
                GetOveride = (() => Variables.LootableAdmins.ToString()),
                SetOveride = delegate (string str)
                {
                    Variables.LootableAdmins = str.ToBool();
                }
            }
        };
    }
}

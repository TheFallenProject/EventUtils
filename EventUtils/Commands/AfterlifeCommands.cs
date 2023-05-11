using CommandSystem;
using Exiled.Permissions.Extensions;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EventUtils.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class BanTeamRespawning : ICommand
    {
        public string Command { get; } = "ev_togglerespawn";
        public string[] Aliases { get; } = { "ev_tr" };
        public string Description { get; } = "Toggles team respawning. Requires <b>ev.afterlife</b> permission.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ev.afterlife"))
            {
                response = "Lacking <b>ev.afterlife</b> permission. Contact server owner/senior admin if you believe this is a mistake.";
                return false;
            }

            Plugin.ShouldIBlockRespawns = !Plugin.ShouldIBlockRespawns;
            response = $"Success! {(Plugin.ShouldIBlockRespawns ? "Now no respawns will take place!" : "Respawning is now enabled!")}";
            return true;
        }
    }

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class DeathRole : ICommand
    {
        public string Command { get; } = "ev_deathrole";
        public string[] Aliases { get; } = { "ev_dr" };
        public string Description { get; } = $"Allows you to set a death role, that dead (and newly-joined) people will get. Spawnpoint is set at your <b>current</b> position (when command is invoked).";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ev.afterlife"))
            {
                response = "Lacking <b>ev.afterlife</b> permission. Contact server owner/senior admin if you believe this is a mistake.";
                return false;
            }

            string UID = sender.LogName.Split('(', ')').Where((item, index) => index % 2 != 0).ToList().Last();

            if (arguments.Count > 2 || arguments.Count < 1)
            {
                response = "Syntax:\n" +
                    $"{Command} enable [role id] - Takes your current position as a death-role spawnpoint and enables it.\n" +
                    $"{Command} disable - Disables death-role";
                return true;
            }
            else
            {
                switch (arguments.At(0).ToLower())
                {
                    case "enable":
                        if (arguments.Count != 2)
                        {
                            response = $"Invalid amount of arguments! Need 2, got {arguments.Count}. Run command without any arguments for help.";
                            return false;
                        }
                        RoleTypeId role;
                        try
                        {
                            role = (RoleTypeId)sbyte.Parse(arguments.At(1));
                        }
                        catch
                        {
                            response = "Conversion failed! Are you sure this is a valid roletype? Use rolelist in (~) console to see avaliable roles.";
                            return false;
                        }
                        Vector3 pos = Exiled.API.Features.Player.List.First(pl => pl.UserId == UID).Position;
                        DeathRoleManager.EnableDeathRole(pos, role);

                        response = $"DeathRole successfully enabled! Now spawning dead (and newly-joined) players at {pos} as {role}";
                        return true;

                        break;
                    case "disable":
                        DeathRoleManager.DisableDeathRole();
                        response = $"DeathRole successfully disabled!";
                        return true;
                        break;
                    default:
                        response = "Unknown subcommand! Run command without any arguments for help.";
                        return false;
                        break;
                }
            }
        }
    }
}

using Exiled.API.Features.Roles;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EventUtils
{
    internal class DeathRoleManager
    {
        private static Vector3 deathRoleSpawnpoint;

        private static RoleTypeId role;

        private static bool shouldRespawn = false;

        private static void InvokePlayerRespawn(Exiled.API.Features.Player pl, bool AnnounceRespawnReason = false)
        {
            if (shouldRespawn)
            {
                if (AnnounceRespawnReason)
                    pl.Broadcast(10, "Вы были возрождены, так как ивентер включил <color=yellow>респавн</color> во время <color=yellow>ивента</color>");

                Timing.CallDelayed(1f, () => { pl.Role.Set(role); pl.Position = deathRoleSpawnpoint; });
            }
        }

        public static void Reset()
        {
            shouldRespawn = false;
        }

        public static void EnableDeathRole(Vector3 spawnpointPos, RoleTypeId role)
        {
            DeathRoleManager.role = role;
            deathRoleSpawnpoint = spawnpointPos;
            shouldRespawn = true;
        }

        public static void DisableDeathRole()
        {
            shouldRespawn = false;
        }

        public static void OnPlayerJoin(Exiled.Events.EventArgs.Player.VerifiedEventArgs ev)
        {
            InvokePlayerRespawn(ev.Player, true);
        }

        public static void OnPlayerDeath(Exiled.Events.EventArgs.Player.DiedEventArgs ev)
        {
            InvokePlayerRespawn(ev.Player);
        }
    }
}

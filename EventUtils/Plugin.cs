using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventUtils
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "TFP-EventUtils";

        public override string Author => "Treeshold (aka Darcy Gaming) | guys help lambert ate my sock";

        public static bool ShouldIBlockRespawns = false;

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Server.RespawningTeam += Server_RespawningTeam;
            Exiled.Events.Handlers.Server.WaitingForPlayers += Server_WaitingForPlayers;
            Exiled.Events.Handlers.Player.Verified += DeathRoleManager.OnPlayerJoin;
            Exiled.Events.Handlers.Player.Died += DeathRoleManager.OnPlayerDeath;
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RespawningTeam -= Server_RespawningTeam;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= Server_WaitingForPlayers;
            Exiled.Events.Handlers.Player.Verified -= DeathRoleManager.OnPlayerJoin;
            Exiled.Events.Handlers.Player.Died -= DeathRoleManager.OnPlayerDeath;

            Server_WaitingForPlayers();
        }

        public override void OnReloaded()
        {
            OnDisabled();
            OnEnabled();
        }

        /// <summary>
        /// THIS MIGHT NOT WORK IF SERVER USES SOME OTHER PLUGINS THAT RELY ON THIS EVENT.
        /// </summary>
        /// <param name="ev"></param>
        private void Server_RespawningTeam(Exiled.Events.EventArgs.Server.RespawningTeamEventArgs ev)
        {
            if (ShouldIBlockRespawns)
                ev.IsAllowed = false;
        }

        private void Server_WaitingForPlayers()
        {
            VotingManager.currentVotingStatus = OpenedVotingStatus.None;
            DeathRoleManager.Reset();
            ShouldIBlockRespawns = false;
        }
    }
}

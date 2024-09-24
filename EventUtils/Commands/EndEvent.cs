using CommandSystem;
using Exiled.Permissions.Extensions;
using MEC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFP_APIManager.Features;

namespace EventUtils.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class EndEvent : ICommand
    {
        public string Command { get; } = "ev_end";
        public string[] Aliases { get; } = { };
        public string Description { get; } = "Заканчивает ивент.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ev.report"))
            {
                response = $"Lacking <b>ev.report</b> permission. Contact server owner/senior admin if you believe this is a mistake.";
                return false;
            }

            if (Managers.ActiveEventManager.ActiveEventId is null)
            {
                response = $"Ивент не был отписан!";
                return false;
            }

            Timing.RunCoroutine(WSEndEventCoroutine());
            response = "Начали танцы с бубном, ждите бродкаст.";
            return true;
        }

        private IEnumerator<float> WSEndEventCoroutine()
        {
            var WSTask = Task.Run(() =>
            {
                Websockets.SendMessage($"[RelayEventEnd] Data: {Managers.ActiveEventManager.ActiveEventId}");
            });
            yield return Timing.WaitUntilTrue(() => { return WSTask.IsCompleted; });
        }
    }
}

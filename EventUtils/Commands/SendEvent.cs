using CommandSystem;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Exiled.Permissions.Extensions;
using TFP_APIManager.Features;
using Exiled.API.Features;

namespace EventUtils.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class SendEvent : ICommand
    {
        public string Command { get; } = "ev_send";
        public string[] Aliases { get; } = { };
        public string Description { get; } = "Автоматически отписывает ивент.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ev.report"))
            {
                response = $"Lacking <b>ev.report</b> permission. Contact server owner/senior admin if you believe this is a mistake.";
                return false;
            }

            var pl = Exiled.API.Features.Player.Get(sender);

            if (Managers.ActiveEventManager.ActiveEventId is not null)
            {
                response = $"Ивент уже отписан!";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = $"Синтаксис: ev_send [Имя ивента]";
                return true;
            }
            else if (arguments.Count < 1)
            {
                response = $"Синтаксис: ev_send [Имя ивента]";
                return false;
            }

            string eventer = "NA";
            if (sender.LogName.ToLower() != "dedicated server")
            {
                eventer = pl.Nickname;
            }
            else
            {
                eventer = Player.Get(sender).Nickname;
            }

            Timing.RunCoroutine(WSReportEventCoroutine(eventer, string.Join(" ", arguments)));
            response = $"Начали танцы с бубном, ждите бродкаст.";
            return true;
        }

        private IEnumerator<float> WSReportEventCoroutine(string eventer, string EventName)
        {
            var WSTask = Task.Run(() =>
            {
                Websockets.SendMessage($"[RelayEventStart] Data: {JsonConvert.SerializeObject(new { Eventer = eventer, Name = EventName })}");
            });
            yield return Timing.WaitUntilTrue(() => { return WSTask.IsCompleted; });
        }
    }
}

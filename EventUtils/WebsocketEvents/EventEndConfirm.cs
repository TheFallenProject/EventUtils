using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using TFP_APIManager.Features;

namespace EventUtils.WebsocketEvents
{
    internal class EventEndConfirm : IWebsocketEvent
    {
        public string Type { get; } = "EventEndConfirm";

        public async Task Invoke(WebSocket websocket, string Subtype, string Content)
        {
            Exiled.API.Features.Log.Info($"Event started! Id (internal): {Content}");
            foreach (var pl in Exiled.API.Features.Player.List)
            {
                if (pl.CheckPermission("ev.report"))
                {
                    pl.Broadcast(10, $"<color=yellow>Ивент отписан как завершённый.");
                }
            }

            Managers.ActiveEventManager.ActiveEventId = null;
        }
    }
}

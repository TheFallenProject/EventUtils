using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using TFP_APIManager.Features;
using Exiled.Permissions.Extensions;

namespace EventUtils.WebsocketEvents
{
    internal class EventId : IWebsocketEvent
    {
        public string Type { get; } = "EventId";

        public async Task Invoke(WebSocket websocket, string Subtype, string Content)
        {
            Exiled.API.Features.Log.Info($"Event started! Id (internal): {Content}");
            foreach (var pl in Exiled.API.Features.Player.List)
            {
                if (pl.CheckPermission("ev.report"))
                {
                    pl.Broadcast(10, $"<color=yellow>Ивент отписан. Можно начинать проводить.</color>\nEventID: {Content}");
                }
            }

            Managers.ActiveEventManager.ActiveEventId = Content;
        }
    }
}

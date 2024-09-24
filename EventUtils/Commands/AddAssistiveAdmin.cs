using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace EventUtils.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class AddAssistiveAdmin : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckPermission("ev.report"))
        {
            response = $"Lacking <b>ev.report</b> permission. Contact server owner/senior admin if you believe this is a mistake.";
            return false;
        }
        
        if (arguments.Count == 0)
        {
            response = $"Синтаксис: ev_add [ID]";
            return true;
        }
        else if (arguments.Count < 1)
        {
            response = $"Синтаксис: ev_add [ID]";
            return false;
        }

        if (TFP_APIManager.Features.Websockets.IsConnected == false)
        {
            response = "WS unavailable!";
            return false;
        }
        
        TFP_APIManager.Features.Websockets.SendMessage($"[AddAssisitveEventer] {Managers.ActiveEventManager.ActiveEventId}: {Player.Get(int.Parse(arguments.At(0))).Nickname}");
        
        response = "Отправлено!";
        return true;
    }

    public string Command { get; } = "ev_add";
    public string[] Aliases { get; } = [];
    public string Description { get; } = "Добавляет администратора в помощники.";
}
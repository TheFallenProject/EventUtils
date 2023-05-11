using CommandSystem;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventUtils.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class YesNoVoting : ICommand
    {
        public string Command { get; } = "ev_yesNoVote";
        public string[] Aliases { get; } = { "ev_sv" };
        public string Description { get; } = "Use this command to begin a Yes/No voting.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ev.voting"))
            {
                response = "Lacking <b>ev.voting</b> permission. Contact server owner/senior admin if you believe this is a mistake.";
                return false;
            }

            if (arguments.Count != 1)
            {
                arguments = new ArraySegment<string>(new string[] { "help" });
            }
                
            switch (arguments.At(0).ToLower())
            {
                case "start":
                    VotingManager.YesNoVoting.ResetVoting();
                    VotingManager.currentVotingStatus = OpenedVotingStatus.YesNoVoting;

                    Exiled.API.Features.Map.Broadcast(10, "<color=yellow>Голосование открыто!</b> Оставьте свой голос при помощи команды .vote [1/0] в консоли на (~).", Broadcast.BroadcastFlags.Normal, true);
                    response = "Now accepting votes!";
                    break;
                case "end":
                    if (VotingManager.currentVotingStatus != OpenedVotingStatus.YesNoVoting)
                    {
                        response = $"Umm, did you open yes/no voting first? Current status mismatch! Expected {OpenedVotingStatus.YesNoVoting}, got {VotingManager.currentVotingStatus}.";
                        return false;
                    }

                    var result = VotingManager.YesNoVoting.GetResults();
                    float fractionFor;
                    try
                    {
                        fractionFor = VotingManager.YesNoVoting.votesTowards / (VotingManager.YesNoVoting.votesTowards + VotingManager.YesNoVoting.votesAgainst);
                    }
                    catch (DivideByZeroException) // <-- in case no one votes
                    {
                        fractionFor = 0f;
                    }

                    string playersRussianized = "игроков";
                    string verbRussianised = "проголосовало";
                    int n = Exiled.API.Features.Player.List.Count();
                    if (n % 10 == 1)
                    {
                        playersRussianized = "игрок";
                        verbRussianised = "проголосовал";
                    }
                    if (n % 10 >= 2 && n % 10 <= 4)
                    {
                        playersRussianized = "игрока";
                        verbRussianised = "проголосовали";
                    }

                    Exiled.API.Features.Map.Broadcast(10, $"<color=yellow>Голосование окончено!</color> Результат - <color=yellow>{result}</color>. <color=grey>({(fractionFor * 100).ToString("0.00")}% за, {verbRussianised} " +
                        $"{(VotingManager.YesNoVoting.votesTowards + VotingManager.YesNoVoting.votesAgainst)} из {Exiled.API.Features.Player.List.Count()} {playersRussianized}.)</color>", Broadcast.BroadcastFlags.Normal, true);
                    response = $"Voting has ended, results: {result}.";

                    VotingManager.currentVotingStatus = OpenedVotingStatus.None; // either way, we will reset voting on the next voting start sequence.
                    break;
                case "help": default:
                    response = $"Syntax: {Command} [start/end]. All arguments are case-insensitive.";
                    break;
            }

            return true;
        }
    }

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class MultipleChoiceVoting : ICommand
    {
        public string Command { get; } = "ev_multipleChoiceVoting";
        public string[] Aliases { get; } = { "ev_mv" };
        public string Description { get; } = "Use this command to begin a Multiple Choice voting.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ev.voting"))
            {
                response = "Lacking <b>ev.voting</b> permission. Contact server owner/senior admin if you believe this is a mistake.";
                return false;
            }

            if (arguments.Count < 1)
            {
                arguments = new ArraySegment<string>(new string[] { "help" });
            }

            switch (arguments.At(0).ToLower())
            {
                case "start":
                    List<string> preCategoryClearingList = new List<string>(arguments);
                    preCategoryClearingList.RemoveAt(0);

                    string caregoriesDirtyString = string.Join(" ", preCategoryClearingList);
                    string[] categories = caregoriesDirtyString.Split(',');
                    
                    for (int i = 0; i < categories.Count(); i++)
                    {
                        categories[i] = categories[i].Trim();
                    }

                    if (categories.Count() < 2)
                    {
                        response = "Вы должны указать как минимум 2 категории!";
                        return false;
                    }
                    

                    VotingManager.MultipleChoiceVoting.InitCategories(categories); //categories inited
                    VotingManager.currentVotingStatus = OpenedVotingStatus.CategoryVoting;

                    Exiled.API.Features.Map.Broadcast(10, "<color=yellow>Голосование за категорию начато!</color> Напишите <color=yellow>.vote в консоль на (~)</color> чтобы увидеть категории и " +
                        ".vote [номер категории] чтобы отдать голос!", Broadcast.BroadcastFlags.Normal, true);

                    response = "Voting started";
                    return true;
                    break;
                case "end":
                    if (VotingManager.currentVotingStatus != OpenedVotingStatus.CategoryVoting)
                    {
                        response = $"Umm, did you open multiple category voting first? Current status mismatch! Expected {OpenedVotingStatus.CategoryVoting}, got {VotingManager.currentVotingStatus}.";
                        return false;
                    }

                    var result = VotingManager.MultipleChoiceVoting.getResults();
                    var playerRelatedResults = VotingManager.MultipleChoiceVoting.GetVotedDetails();

                    string playersRussianized = "игроков";
                    string verbRussianised = "проголосовало";
                    int n = Exiled.API.Features.Player.List.Count();
                    if (n % 10 == 1) 
                    { 
                        playersRussianized = "игрок";
                        verbRussianised = "проголосовал";
                    }
                    if (n % 10 >= 2 && n % 10 <= 4)
                    { 
                        playersRussianized = "игрока";
                        verbRussianised = "проголосовали";
                    }

                    Exiled.API.Features.Map.Broadcast(10, $"<color=yellow>Голосование окончено!</color> Победившая категория - <color=yellow>{result}</color>. <color=grey>({(playerRelatedResults.fractionVoted * 100).ToString("0.00")}% за, {verbRussianised} " +
                        $"{(playerRelatedResults.playersVoted)} из {Exiled.API.Features.Player.List.Count()} {playersRussianized}.)</color>", Broadcast.BroadcastFlags.Normal, true);
                    response = $"Voting has ended, winning category: {result}.";

                    VotingManager.currentVotingStatus = OpenedVotingStatus.None;
                    return true;
                    break;
                case "help":
                default:
                    response = $"Syntax: \n" +
                        $"{Command} start [category 1], [category 2], <category 3>, <category 4>, <category 5>. All arguments are case-insensitive.\n" +
                        $"Example: {Command} start This is category 1, This is category 2, This is the third category\n\n" +
                        $"{Command} end";
                    return true;
                    break;
            }

            response = "Unhandled Response.";
            return false;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    internal class ClientVote : ICommand
    {
        public string Command { get; } = "голос"; //scuffed translation, IK. It's the shortest one I could think about.
        public string[] Aliases { get; } = { "vote" };
        public string Description { get; } = "Проголосовать, если голосование открыто.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            string UID = sender.LogName.Split('(', ')').Where((item, index) => index % 2 != 0).ToList().Last();

            switch (VotingManager.currentVotingStatus)
            {
                case OpenedVotingStatus.None:
                    response = "Голосование не открыто.";
                    return false;
                case OpenedVotingStatus.YesNoVoting:
                    try
                    {
                        if (arguments.Count != 1)
                        {
                            response = "Вы должы указать 1 (За) или 0 (Против).";
                            return false;
                        }

                        switch (arguments.At(0))
                        {
                            case "0":
                                VotingManager.YesNoVoting.CastAVote(UID, false);
                                break;
                            case "1":
                                VotingManager.YesNoVoting.CastAVote(UID, true);
                                break;
                            default:
                                response = "Вы должы указать 1 (За) или 0 (Против).";
                                return false;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        response = $"Не удалось отдать ваш голос. Ошибка: {ex.Message}";
                        return false;
                    }
                    response = "Успешно! Ваш голос учтён!";
                    return true;
                case OpenedVotingStatus.CategoryVoting:
                    try
                    {
                        if (arguments.Count != 1)
                        {
                            var categories = VotingManager.MultipleChoiceVoting.getCategories();

                            string categoryClearedString = "";
                            foreach (var cat in categories)
                            {
                                categoryClearedString += $"\n[{cat.Key}] {cat.Value}";
                            }

                            response = $"Синтаксис: .{Command} [номер категории].\n" +
                                $"Категории (в виде \"[номер] имя\"):" +
                                $"{categoryClearedString}";
                            return false;
                        }

                        VotingManager.MultipleChoiceVoting.castAVote(UID, int.Parse(arguments.At(0)));
                        response = $"Ваш голос учтён!";
                        return true;
                    }
                    catch (Exception ex)
                    {
                        response = $"Не удалось отдать ваш голос. Ошибка: {ex.Message}";
                        return false;
                    }
                default:
                    response = "Неожиданное состояние currentVotingStatus";
                    return false;
            }

            response = "InDev";
            return true;
        }
    }
}

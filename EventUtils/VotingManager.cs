using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventUtils
{
    internal enum YesNoVotingResults
    {
        /// <summary>
        /// Voting passed
        /// </summary>
        Accepted,
        
        /// <summary>
        /// Voting tied
        /// </summary>
        Tied, 
        
        /// <summary>
        /// Voting declined
        /// </summary>
        Declined
    }

    internal enum OpenedVotingStatus
    {
        /// <summary>
        /// Yes/No voting is active
        /// </summary>
        YesNoVoting, 

        /// <summary>
        /// Category voting is active
        /// </summary>
        CategoryVoting, 

        /// <summary>
        /// No voting is active
        /// </summary>
        None
    }

    internal static class VotingManager
    {
        public static OpenedVotingStatus currentVotingStatus = OpenedVotingStatus.None;

        public static class YesNoVoting 
        {
            public static ulong votesTowards { get; private set; }

            public static ulong votesAgainst { get; private set; }

            private static Dictionary<string, bool> alreadyVoted = new Dictionary<string, bool>();

            public static string CastAVote(string UID, bool isTowards)
            {
                if (VotingManager.currentVotingStatus != OpenedVotingStatus.YesNoVoting)
                    throw new Exception("Yes/No voting is not opened");

                if (alreadyVoted.ContainsKey(UID))
                {
                    bool _cachedVote = alreadyVoted[UID];

                    if (_cachedVote == isTowards)
                        throw new Exception("Player attempted to vote twice");

                    if (isTowards)
                    {
                        votesAgainst--;
                        votesTowards++;
                    }
                    else
                    {
                        votesAgainst++;
                        votesTowards--;
                    }

                    alreadyVoted[UID] = isTowards;

                    return "vote changed";
                }
                else
                {
                    alreadyVoted.Add(UID, isTowards);

                    if (isTowards)
                        votesTowards++;
                    else
                        votesAgainst++;

                    return "vote casted";
                }
            }

            public static YesNoVotingResults GetResults()
            {
                currentVotingStatus = OpenedVotingStatus.None;

                if (votesTowards > votesAgainst)
                    return YesNoVotingResults.Accepted;
                else if (votesTowards == votesAgainst)
                    return YesNoVotingResults.Tied;
                else
                    return YesNoVotingResults.Declined;
            }

            public static void ResetVoting()
            {
                votesTowards = 0;
                votesAgainst = 0;
                alreadyVoted.Clear();
            } 
        }

        public static class MultipleChoiceVoting
        {
            public class VotingPlayerCountDetails
            {
                public float fractionVoted;

                public int playersVoted;

                public int winningCategoryVotes;
            }

            /// <summary>
            /// Category name, current votes
            /// </summary>
            public static Dictionary<string, int> voteCategories = new Dictionary<string, int>();

            /// <summary>
            /// UserID, Category INDEX
            /// </summary>
            public static Dictionary<string, int> castedVotes = new Dictionary<string, int>();

            /// <summary>
            /// Category NUMBER, Category name
            /// </summary>
            /// <returns></returns>
            public static Dictionary<int, string> getCategories()
            {
                var dict = new Dictionary<int, string>();

                for (int i = 0; i < voteCategories.Count; i++)
                {
                    dict.Add(i + 1, voteCategories.ElementAt(i).Key);
                }

                return dict;
            }

            public static void InitCategories(IEnumerable<string> categories) // <-- you gotta somehow parse the categories in RA command args (we won't get a raw query)
            {
                voteCategories.Clear();
                castedVotes.Clear();
                foreach (var cat in categories)
                {
                    voteCategories.Add(cat, 0);
                }
            }

            public static VotingPlayerCountDetails GetVotedDetails()
            {
                int totalVotes = 0;
                int winningVotes = 0;
                int playerCount = Exiled.API.Features.Player.List.Count();

                KeyValuePair<string, int> winningPair = new KeyValuePair<string, int>("None!", 0);
                foreach (var cat in voteCategories)
                {
                    if (cat.Value > winningPair.Value)
                    {
                        winningPair = cat;
                    }

                    totalVotes += cat.Value;
                }

                winningVotes = winningPair.Value;
                var resp = new VotingPlayerCountDetails()
                {
                    playersVoted = totalVotes,
                    winningCategoryVotes = winningVotes
                };
                try
                {   //                       always   <  than that
                    resp.fractionVoted = totalVotes / playerCount;
                }
                catch (DivideByZeroException)
                {
                    resp.fractionVoted = 0f; //prob noone voted
                }

                return resp;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="UID"></param>
            /// <param name="selection">index + 1</param>
            public static string castAVote(string UID, int selection)
            {
                if (VotingManager.currentVotingStatus != OpenedVotingStatus.CategoryVoting)
                    throw new Exception("Multiple category voting is not opened");

                if (selection > voteCategories.Count || selection < 0)
                    throw new Exception("Selection is out of bounds of the category dictionary");

                if (castedVotes.ContainsKey(UID))
                {
                    int pastCategoryIndex = castedVotes[UID];
                    int currentCategoryIndex = selection - 1;


                    if (pastCategoryIndex == selection - 1)
                        throw new Exception("Player attempted to vote twice");

                    var pastPair = voteCategories.ElementAt(pastCategoryIndex);
                    var currentPair = voteCategories.ElementAt(currentCategoryIndex);
                    voteCategories[pastPair.Key]--;
                    voteCategories[currentPair.Key]++;

                    castedVotes[UID] = currentCategoryIndex;

                    return "vote changed";
                }
                else
                {
                    int currentCategoryIndex = selection - 1;
                    var currentPair = voteCategories.ElementAt(currentCategoryIndex);
                    voteCategories[currentPair.Key]++;

                    castedVotes.Add(UID, currentCategoryIndex);

                    return "vote casted";
                }
            }

            /// <summary>
            /// Returns the name of the winning category. If there are 2 winning categories, one with lowest index will be picked.
            /// </summary>
            /// <returns></returns>
            public static string getResults()
            {
                KeyValuePair<string, int> winningPair = new KeyValuePair<string, int>("None!", 0);

                foreach (var cat in voteCategories)
                {
                    if (cat.Value > winningPair.Value)
                    {
                        winningPair = cat;
                    }
                }


                currentVotingStatus = OpenedVotingStatus.None;
                return winningPair.Key;
            }
        }
    }
}

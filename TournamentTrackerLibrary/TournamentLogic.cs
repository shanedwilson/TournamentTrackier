using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TournamentTrackerLibrary
{
    public static class TournamentLogic
    {
        public static void CreateRounds(TournamentModel model)
        {
            List<TeamModel> randomizedTeams = RandomizeTeamOrder(model.Teams);
            int rounds = FindNumberOfRounds(randomizedTeams.Count);
            int byes = NumberOfByes(rounds, randomizedTeams.Count);

            model.Rounds.Add(CreateFirstRound(byes, randomizedTeams));

            CreateOtherRounds(model, rounds);
        }

        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchUpModel> previousRound = model.Rounds[0];
            List<MatchUpModel> currRound = new List<MatchUpModel>();
            MatchUpModel currMatchup = new MatchUpModel();

            while(round <= rounds)
            {
                foreach(MatchUpModel match in previousRound)
                {
                    currMatchup.Entries.Add(new MatchUpEntryModel { ParentMatchup = match });

                    if (currMatchup.Entries.Count > 1)
                    {
                        currMatchup.MatchupRound = round;
                        currRound.Add(currMatchup);
                        currMatchup = new MatchUpModel();
                    }
                }
                model.Rounds.Add(currRound);
                previousRound = currRound;
                currRound = new List<MatchUpModel>();
                round += 1;
            }
        }

        private static List<MatchUpModel>CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchUpModel> output = new List<MatchUpModel>();
            MatchUpModel curr = new MatchUpModel();

            foreach(TeamModel team in teams)
            {
                curr.Entries.Add(new MatchUpEntryModel { TeamCompeting = team });

                if (byes > 0 || curr.Entries.Count > 1)
                {
                    curr.MatchupRound = 1;
                    output.Add(curr);
                    curr = new MatchUpModel();
                    
                    if(byes > 0)
                    {
                        byes -= 1;
                    };
                }
            }
            return output;
        }

        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            int output = 0;
            int totalTeams = 1;

            for (int i = 1; i <= rounds; i++)
            {
                totalTeams *= 2;
            }
            output = totalTeams - numberOfTeams;

            return output;
        }

        private static int FindNumberOfRounds(int teamCount)
        {
            int output = 1;
            int val = 2;

            while (val < teamCount)
            {
                output += 1;
                val *= 2;
            }

            return output;
        }

        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
           return teams.OrderBy(team => Guid.NewGuid()).ToList();
        }
    }
}

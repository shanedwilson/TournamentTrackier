using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary;
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

            UpdateTournamentResults(model);
        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            List<MatchUpModel> toScore = new List<MatchUpModel>();

            foreach(List<MatchUpModel> round in model.Rounds)
            {
                foreach(MatchUpModel rm in round)
                {
                    if (rm.Winner == null && (rm.Entries.Any(x => x.Score != 0) || rm.Entries.Count == 1))
                    {
                        toScore.Add(rm);
                    }
                }
            }

            MarkWinnerInMatchup(toScore);

            AdvanceWinners(toScore, model);

            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
        }

        private static void AdvanceWinners(List<MatchUpModel> models, TournamentModel tournament)
        {
            foreach (MatchUpModel m in models)
            {
                foreach (List<MatchUpModel> round in tournament.Rounds)
                {
                    foreach (MatchUpModel rm in round)
                    {
                        foreach (MatchUpEntryModel me in rm.Entries)
                        {
                            if (me.ParentMatchup != null)
                            {
                                if (me.ParentMatchup.Id == m.Id)
                                {
                                    me.TeamCompeting = m.Winner;
                                    GlobalConfig.Connection.UpdateMatchup(rm);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void MarkWinnerInMatchup(List<MatchUpModel> models)
        {
            string greaterWins = ConfigurationManager.AppSettings["greaterWins"];

            foreach (MatchUpModel m in models)
            {
                if(m.Entries.Count ==1)
                {
                    m.Winner = m.Entries[0].TeamCompeting;
                    continue;
                }

                if (greaterWins == "0")
                {
                    if (m.Entries[0].Score < m.Entries[1].Score) 
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if (m.Entries[1].Score < m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties in this application.");
                    }
                }
                else
                {
                    if (m.Entries[0].Score > m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if (m.Entries[1].Score > m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties in this application.");
                    }
                } 
            }
        }


        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchUpModel> previousRound = model.Rounds[0];
            List<MatchUpModel> currRound = new List<MatchUpModel>();
            MatchUpModel currMatchup = new MatchUpModel();

            while (round <= rounds)
            {
                foreach (MatchUpModel match in previousRound)
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

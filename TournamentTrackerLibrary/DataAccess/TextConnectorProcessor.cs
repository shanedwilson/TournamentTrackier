using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TournamentTrackerLibrary.DataAccess.TextHelpers
{
    public static class TextConnectorProcessor
    {
        public static string FullFilePath(this string fileName)
        {
            return $"{ ConfigurationManager.AppSettings["filepath"] }\\{fileName}";
        }

        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }

        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new List<PrizeModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PrizeModel p = new PrizeModel();
                p.Id = int.Parse(cols[0]);
                p.PlaceNumber = int.Parse(cols[1]);
                p.PlaceName = cols[2];
                p.PrizeAmount = decimal.Parse(cols[3]);
                p.PrizePercentage = double.Parse(cols[4]);
                output.Add(p);
            }
            return output;
        }

        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            List<PersonModel> output = new List<PersonModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PersonModel p = new PersonModel();
                p.Id = int.Parse(cols[0]);
                p.FirstName = cols[1];
                p.LastName = cols[2];
                p.EmailAddress = cols[3];
                p.CellPhoneNumber = cols[4];
                output.Add(p);
            }
            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines, string peopleFileName)
        {
            List<TeamModel> output = new List<TeamModel>();
            List<PersonModel> people = peopleFileName.FullFilePath().LoadFile().ConvertToPersonModels();


            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TeamModel t = new TeamModel();
                t.Id = int.Parse(cols[0]);
                t.TeamName = cols[1];

                string[] personIds = cols[2].Split('|');

                foreach(string id in personIds)
                {
                    t.TeamMembers.Add(people.Where(x => x.Id == int.Parse(id)).First());
                }
                output.Add(t);
            }
            return output;
        }

        public static List<TournamentModel> ConvertToTournamentModels(
            this List<string> lines,
            string teamFileName,
            string peopleFileName,
            string prizeFileName)
        {
            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> teams = teamFileName.FullFilePath().LoadFile().ConvertToTeamModels(peopleFileName);
            List<PrizeModel> prizes = prizeFileName.FullFilePath().LoadFile().ConvertToPrizeModels();
            List<MatchUpModel> matchUps = GlobalConfig.MatchUpFile.FullFilePath().LoadFile().ConvertToMatchUpModels();


            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TournamentModel tm = new TournamentModel();
                tm.Id = int.Parse(cols[0]);
                tm.TournamentName = cols[1];
                tm.EntryFee = decimal.Parse(cols[2]);

                string[] teamIds = cols[3].Split('|');

                foreach (string id in teamIds)
                {
                    tm.Teams.Add(teams.Where(team => team.Id == int.Parse(id)).First());
                }

                string[] prizeIds = cols[4].Split('|');

                foreach(string id in prizeIds)
                {
                    tm.Prizes.Add(prizes.Where(prize => prize.Id == int.Parse(id)).First());
                }

                string[] rounds = cols[5].Split('|');

                foreach(string round in rounds)
                {
                    string[] msText = round.Split('^');
                    List<MatchUpModel> ms = new List<MatchUpModel>();

                    foreach (string matchUpModelTextId in msText)
                    {
                        ms.Add(matchUps.Where(x => x.Id == int.Parse(matchUpModelTextId)).First());
                    }
                    tm.Rounds.Add(ms);
                }
                output.Add(tm);
            }
            return output;
        }

        public static void SaveToPrizeFile(this List<PrizeModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (PrizeModel p in models)
            {
                lines.Add($"{ p.Id },{ p.PlaceNumber },{ p.PlaceName },{ p.PrizeAmount },{ p.PrizePercentage }");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        public static void SaveToPeopleFile(this List<PersonModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (PersonModel p in models)
            {
                lines.Add($"{ p.Id },{ p.FirstName },{ p.LastName },{ p.EmailAddress },{ p.CellPhoneNumber }");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }


        public static void SaveToTeamFile(this List<TeamModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (TeamModel t in models)
            {
                lines.Add($"{ t.Id },{ t.TeamName },{ConvertPeopleListToString(t.TeamMembers) }");
            }
            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        public static void SaveRoundsToFile(this TournamentModel model)
        {
            foreach(List<MatchUpModel> round in model.Rounds)
            {
                foreach(MatchUpModel matchup in round)
                {
                    matchup.SaveMatchUpToFile();
                }

            }
        }

        private static List<MatchUpEntryModel> ConvertStringToMatchUpEntryModels(string input)
        {
            string[] ids = input.Split('|');
            List<MatchUpEntryModel> output = new List<MatchUpEntryModel>();
            List<MatchUpEntryModel> entries = GlobalConfig.MatchUpEntryFile.FullFilePath()
                .LoadFile()
                .ConvertToMatchUpEntryModels();

            foreach(string id in ids)
            {
                output.Add(entries.Where(entry => entry.Id == int.Parse(id)).First());
            }
            return output;
        }

        public static List<MatchUpEntryModel> ConvertToMatchUpEntryModels(this List<string> lines)
        {
            List<MatchUpEntryModel> output = new List<MatchUpEntryModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchUpEntryModel me = new MatchUpEntryModel();
                me.Id = int.Parse(cols[0]);
                if(cols[1].Length == 0)
                {
                    me.TeamCompeting = null;
                }
                else
                {
                    me.TeamCompeting = LookUpTeamById(int.Parse(cols[1]));
                }
                me.Score = double.Parse(cols[2]);

                int parentId = 0;
                if (int.TryParse(cols[3], out parentId))
                {
                    me.ParentMatchUp = LookUpMatchUpById(parentId);

                }
                else
                {
                    me.ParentMatchUp = null;
                }
                output.Add(me);
            }
            return output;
        }

        private static TeamModel LookUpTeamById(int id)
        {
            List<TeamModel> teams = GlobalConfig.TeamsFile.FullFilePath().LoadFile().ConvertToTeamModels(GlobalConfig.PeopleFile);

            return teams.Where(team => team.Id == id).First();
        }

        private static MatchUpModel LookUpMatchUpById(int id)
        {
            List<MatchUpModel> matchUps = GlobalConfig.MatchUpFile.FullFilePath().LoadFile().ConvertToMatchUpModels();

            return matchUps.Where(matchup => matchup.Id == id).First();
        }

        public static List<MatchUpModel> ConvertToMatchUpModels(this List<string> lines)
        {
            List<MatchUpModel> output = new List<MatchUpModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchUpModel mu = new MatchUpModel();
                mu.Id = int.Parse(cols[0]);
                mu.Entries = ConvertStringToMatchUpEntryModels(cols[1]);
                mu.Winner = LookUpTeamById(int.Parse(cols[2]));
                mu.MatchUpRound = int.Parse(cols[3]);
                output.Add(mu);
            }
            return output;
        }

        public static void SaveMatchUpToFile(this MatchUpModel matchUp)
        {
            List<MatchUpModel> matchUps = GlobalConfig.MatchUpFile.FullFilePath().LoadFile().ConvertToMatchUpModels();

            int currentId = 1;

            if (matchUps.Count > 0)
            {
                currentId = matchUps.OrderByDescending(x => x.Id).First().Id + 1;
            }

            matchUp.Id = currentId;

            matchUps.Add(matchUp);

            List<string> lines = new List<string>();

            foreach (MatchUpModel m in matchUps)
            {
                string winner = "";
                if (m.Winner != null)
                {
                    winner = m.Winner.Id.ToString();
                }
                lines.Add($"{ m.Id },{ "" },{ winner },{ m.MatchUpRound }");
            }
            File.WriteAllLines(GlobalConfig.MatchUpFile.FullFilePath(), lines);

            foreach (MatchUpEntryModel entry in matchUp.Entries)
            {
                entry.SaveEntryToFile(GlobalConfig.MatchUpEntryFile);
            }

            lines = new List<string>();

            foreach(MatchUpModel m in matchUps)
            {
                string winner = "";
                if(m.Winner != null)
                {
                    winner = m.Winner.Id.ToString();
                }
                lines.Add($"{ m.Id },{ ConvertMatchUpEntryListToString(m.Entries)},{ winner },{ m.MatchUpRound }");
            }
            File.WriteAllLines(GlobalConfig.MatchUpFile.FullFilePath(), lines);
        }

        private static string ConvertMatchUpEntryListToString(List<MatchUpEntryModel> entries)
        {
            string output = "";

            if (entries.Count == 0)
            {
                return "";
            }

            foreach (MatchUpEntryModel e in entries)
            {
                output += $"{ e.Id }|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        public static void SaveEntryToFile(this MatchUpEntryModel entry, string matchUpEntryFile)
        {
            List<MatchUpEntryModel> entries = GlobalConfig.MatchUpEntryFile.FullFilePath().LoadFile().ConvertToMatchUpEntryModels();

            int currentId = 1;
            if(entries.Count > 0)
            {
                currentId = entries.OrderByDescending(x => x.Id).First().Id + 1;
            }

            entry.Id = currentId;
            entries.Add(entry);

            List<string> lines = new List<string>();

            foreach (MatchUpEntryModel e in entries)
            {
                string parent = "";
                if(e.ParentMatchUp != null)
                {
                    parent = e.ParentMatchUp.ToString();
                }
                string teamCompeting = "";
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{ e.Id },{ teamCompeting },{ e.Score},{ parent }");
            }
            File.WriteAllLines(GlobalConfig.MatchUpEntryFile.FullFilePath(), lines);
        }

        public static void SaveToTournamentFile(this List<TournamentModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (TournamentModel tm in models)
            {
                lines.Add($@"{ tm.Id },
                    { tm.TournamentName },
                    {tm.EntryFee},
                    {ConvertTeamListToString(tm.Teams) },
                    {ConvertPrizeListToString(tm.Prizes)},
                    {ConvertRoundListToString(tm.Rounds)}");
            }
            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        private static string ConvertPeopleListToString(List<PersonModel> people)
        {
            string output = "";

            if(people.Count == 0)
            {
                return "";
            }

            foreach(PersonModel p in people)
            {
                output += $"{ p.Id }|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertTeamListToString(List<TeamModel> teams)
        {
            string output = "";

            if (teams.Count == 0)
            {
                return "";
            }

            foreach (TeamModel tm in teams)
            {
                output += $"{ tm.Id }|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertPrizeListToString(List<PrizeModel> prizes)
        {
            string output = "";

            if (prizes.Count == 0)
            {
                return "";
            }

            foreach (PrizeModel px in prizes)
            {
                output += $"{ px.Id }|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertRoundListToString(List<List<MatchUpModel>> rounds)
        {
            string output = "";

            if (rounds.Count == 0)
            {
                return "";
            }

            foreach (List<MatchUpModel> r in rounds)
            {
                output += $"{ ConvertMatchUpListToString(r) }|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertMatchUpListToString(List<MatchUpModel> matchups)
        {
            string output = "";

            if (matchups.Count == 0)
            {
                return "";
            }

            foreach (MatchUpModel mu in matchups)
            {
                output += $"{ mu.Id }^";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary.Models
{
    public class MatchUpModel
    {
        public int Id { get; set; }
        public List<MatchUpEntryModel> Entries { get; set; } = new List<MatchUpEntryModel>();
        public  int WinnerId {get; set; }
        public TeamModel Winner { get; set; }
        public int MatchupRound { get; set; }
    }
}

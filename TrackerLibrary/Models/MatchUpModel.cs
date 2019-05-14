using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
{
    public class MatchUpModel
    {
        public List<MatchUpEntryModel> Entries { get; set; } = new List<MatchUpEntryModel>();
        public TeamModel Winner { get; set; }
        public int MatchUpRound { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary.Models
{
    public class MatchUpEntryModel
    {
        public int Id { get; set; }
        public int TeamCompetingId { get; set; }
        public TeamModel TeamCompeting { get; set; }
        public Double Score { get; set; }
        public int ParentMatchupId { get; set; }
        public MatchUpModel ParentMatchup { get; set; }
    }
}

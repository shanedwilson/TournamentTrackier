using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary.Models
{
    public class MatchUpEntryModel
    {
        public TeamModel TeamCompeting { get; set; }
        public Double Score { get; set; }
        public MatchUpModel ParentMatchUp { get; set; }
    }
}

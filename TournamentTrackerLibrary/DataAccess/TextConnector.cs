using System;
using System.Collections.Generic;
using System.Text;
using TrackerLibrary.Models;
using TournamentTrackerLibrary.DataAccess.TextHelpers;

namespace TrackerLibrary.DataAccess
{
    public class TextConnector : IDataConnection
    {
        public PrizeModel CreatePrize(PrizeModel model)
        {
            //todo: wire up createprize for text files
            model.Id = 1;

            return model;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary;

namespace TournamentTrackerLibrary
{
    public interface IDataConnection
    {
        PrizeModel CreatePrize(PrizeModel model);
    }
}

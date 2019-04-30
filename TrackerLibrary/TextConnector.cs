using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
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

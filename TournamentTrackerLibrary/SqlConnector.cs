using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
{
    public class SqlConnector : IDataConnection
    {
        public PrizeModel CreatePrize(PrizeModel model)
        {
            ///todo: make the create prize method actually save to the database
            model.Id = 1;

            return model;
        }
    }
}

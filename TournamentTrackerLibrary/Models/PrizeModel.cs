using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
{
    public class PrizeModel
    {
        public int Id { get; set; }
        public int PlaceNumber { get; set; }
        public string PlaceName { get; set; }
        public decimal PrizeAmount { get; set; }
        public double PrizePercentage { get; set; }

        public PrizeModel()
        {

        }

        public PrizeModel(string placeName, string placeNumber, string prizeAmount, string prizePercentage)
        {
            int placeNumberValue = 0;
            decimal prizeAmountValue = 0;
            double prizePercentageValue = 0;

            int.TryParse(placeNumber, out placeNumberValue);
            decimal.TryParse(prizeAmount, out prizeAmountValue);
            double.TryParse(prizePercentage, out prizePercentageValue);

            PlaceName = placeName;
            PlaceNumber = placeNumberValue;
            PrizeAmount = prizeAmountValue;
            PrizePercentage = prizePercentageValue;
        }
    }


}

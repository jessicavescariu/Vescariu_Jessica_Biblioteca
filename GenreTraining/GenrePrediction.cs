
using Microsoft.ML.Data;

namespace GenreTraining
{
    public class GenrePrediction
    {
        [ColumnName("PredictedLabel")]
        public string Genre { get; set; } = "";

        public float[] Score { get; set; } = [];
    }
}

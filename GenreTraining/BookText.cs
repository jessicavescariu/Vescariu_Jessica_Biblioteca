
using Microsoft.ML.Data;

namespace GenreTraining
{
    public class BookText
    {
        [LoadColumn(0)]
        public string Description { get; set; } = "";

        [LoadColumn(1)]
        public string Genre { get; set; } = "";
    }
}

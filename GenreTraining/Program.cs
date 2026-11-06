
using GenreTraining;
using Microsoft.ML;

var ml = new MLContext(seed: 1);

string csvPath = Path.Combine(
    AppContext.BaseDirectory,
    "book-descriptions.csv"
);

IDataView data = ml.Data.LoadFromTextFile<BookText>(
    csvPath,
    hasHeader: true,
    separatorChar: ',',
    allowQuoting: true
);

var split = ml.Data.TrainTestSplit(
    data,
    testFraction: 0.2
);


var pipeline = ml.Transforms.Conversion
    .MapValueToKey("Label", nameof(BookText.Genre))
    .Append(ml.Transforms.Text.FeaturizeText(
        "Features",
        nameof(BookText.Description)))
    .Append(ml.MulticlassClassification.Trainers
        .SdcaMaximumEntropy("Label", "Features"))
    .Append(ml.Transforms.Conversion
        .MapKeyToValue("PredictedLabel"));


var model = pipeline.Fit(split.TrainSet);

var predictions = model.Transform(split.TestSet);

var metrics = ml.MulticlassClassification.Evaluate(predictions);

Console.WriteLine($"MicroAccuracy: {metrics.MicroAccuracy:P1}");
Console.WriteLine($"MacroAccuracy: {metrics.MacroAccuracy:P1}");

Console.WriteLine(
    metrics.ConfusionMatrix.GetFormattedConfusionTable()
);

string modelPath = Path.Combine(
    AppContext.BaseDirectory,
    "GenreClassifier.zip"
);

ml.Model.Save(model, data.Schema, modelPath);

Console.WriteLine("Model salvat în: " + modelPath);

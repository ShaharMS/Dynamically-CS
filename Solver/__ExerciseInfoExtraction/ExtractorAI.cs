using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst;
using Catalyst.Models;
using Dynamically.Backend;
using Microsoft.Extensions.Logging;
using Mosaik.Core;
using Version = Mosaik.Core.Version;

namespace Dynamically.Solver.ExerciseInfoExtraction;

public class ExtractorAI
{
    public static ExtractorAI Instance { get; } = new ExtractorAI();

    public string? CurrentText = null;

    public List<(string EntityType, string EnitityName)> RecognizedEntities = new();

    static ExtractorAI()
    {
        Storage.Current = new DiskStorage("catalyst-models");
        // Readable languages. currently supports English, Hebrew, and Arabic.
        English.Register(); Hebrew.Register(); Arabic.Register();
    }

    public ExtractorAI(string currentText) { CurrentText = currentText; }
    public ExtractorAI() { }

    public ExtractorAI AssignText(string currentText) {
        CurrentText = currentText;
        return this;
    }

    public async Task<ExtractorAI> RecognizeEntities()
    {
        // use catalyst to extract entities from the text. entities are extracted from the text as a pair of type & name.
        Log.Write("Starting recognition: retrieving enlish, hebrew & arabic pack");
        var naturalLanguageProcessor = await Pipeline.ForManyAsync(new[] { Language.English});
        Log.Write("Done!");
        Log.Write("recognition: adding entities");
        naturalLanguageProcessor.Add(await AveragePerceptronEntityRecognizer.FromStoreAsync(Language.English, version: Version.Latest, tag: "WikiNER"));
        Log.Write("recognition: English");
        Log.Write("Done!");
        Log.Write("recognition: printing tokens");
        var recognized = naturalLanguageProcessor.ProcessSingle(new Document(CurrentText, Language.English));
        Log.WriteAsTree(recognized);
        Log.Write(recognized.ToJson().PrettifyJson());
        return this;
    }
}

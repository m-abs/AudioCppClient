using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>kitten_tts2</c> spec, clone task (model default <c>kitten_tts2_20261021_q8_0</c>).</summary>
public sealed record KittenTts2CloneRequest : CloneRequest
{
    /// <summary>Prepared voice name available in the loaded package. A supplied reference clip takes precedence.</summary>
    public new KittenTts2VoiceId? VoiceId { get; init; }

    /// <summary>Upstream sampling preset.</summary>
    public KittenTts2Preset? Preset { get; init; } = KittenTts2Preset.Stable;

    /// <summary>Sampling temperature; zero selects greedy decoding. Otherwise defaults to the preset.</summary>
    public double? Temperature { get; init; }

    /// <summary>Top-k sampling cutoff; defaults to the preset.</summary>
    public long? TopK { get; init; }

    /// <summary>Nucleus sampling cutoff; defaults to the preset. Must be in (0, 1].</summary>
    public double? TopP { get; init; }

    /// <summary>Minimum relative probability; defaults to the preset.</summary>
    public double? MinP { get; init; }

    /// <summary>Penalty over the last 50 generated tokens; silence and ending tokens are exempt.</summary>
    public double? RepetitionPenalty { get; init; } = 1.1;

    /// <summary>Speech-token budget per text chunk. Omitted budgets scale with text length, with a minimum of 200.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Seed for sampling and waveform decoding.</summary>
    public string? Seed { get; init; }

    /// <summary>Maximum Unicode codepoints per framework text chunk.</summary>
    public long? TextChunkSize { get; init; } = 380;

    /// <summary>Framework long-form text chunking mode.</summary>
    public KittenTts2TextChunkMode? TextChunkMode { get; init; } = KittenTts2TextChunkMode.TagAware;

    /// <summary>Transcript of the reference audio; required when cloning from a 1–30 second clip.</summary>
    public string? ReferenceText { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (VoiceId != null)
        {
            throw new ArgumentException("kitten_tts2 clone does not accept voice_id (capability built_in_voices is false)");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("kitten_tts2 clone temperature must be at least 0", "Temperature");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("kitten_tts2 clone top_k must be at least 0", "TopK");
        }

        if (TopP != null && (TopP.Value > 1))
        {
            throw new ArgumentException("kitten_tts2 clone top_p must be at most 1", "TopP");
        }

        if (MinP != null && (MinP.Value < 0 || MinP.Value > 1))
        {
            throw new ArgumentException("kitten_tts2 clone min_p must be between 0 and 1", "MinP");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 1))
        {
            throw new ArgumentException("kitten_tts2 clone repetition_penalty must be at least 1", "RepetitionPenalty");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1 || MaxTokens.Value > 8192))
        {
            throw new ArgumentException("kitten_tts2 clone max_tokens must be between 1 and 8192", "MaxTokens");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 32 || TextChunkSize.Value > 1000))
        {
            throw new ArgumentException("kitten_tts2 clone text_chunk_size must be between 32 and 1000", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (VoiceId != null && VoiceId != KittenTts2VoiceId.Bruno)
        {
            options["voice_id"] = VoiceId switch
            {
            KittenTts2VoiceId.Bella => "Bella",
            KittenTts2VoiceId.Jasper => "Jasper",
            KittenTts2VoiceId.Luna => "Luna",
            KittenTts2VoiceId.Bruno => "Bruno",
            KittenTts2VoiceId.Rosie => "Rosie",
            KittenTts2VoiceId.Hugo => "Hugo",
            KittenTts2VoiceId.Kiki => "Kiki",
            KittenTts2VoiceId.Leo => "Leo",
            KittenTts2VoiceId.Matthew => "Matthew",
            KittenTts2VoiceId.Elliot => "Elliot",
            KittenTts2VoiceId.Willow => "Willow",
            KittenTts2VoiceId.Dolores => "Dolores",
            KittenTts2VoiceId.Victor => "Victor",
            KittenTts2VoiceId.Dante => "Dante",
            KittenTts2VoiceId.Alfred => "Alfred",
            KittenTts2VoiceId.Saoirse => "Saoirse",
            KittenTts2VoiceId.Claire => "Claire",
            KittenTts2VoiceId.Raven => "Raven",
            KittenTts2VoiceId.Marcus => "Marcus",
            KittenTts2VoiceId.Herbert => "Herbert",
            KittenTts2VoiceId.Diana => "Diana",
            KittenTts2VoiceId.Laurence => "Laurence",
            KittenTts2VoiceId.Maeve => "Maeve",
            KittenTts2VoiceId.Walter => "Walter",
            KittenTts2VoiceId.Edith => "Edith",
            KittenTts2VoiceId.Miles => "Miles",
            KittenTts2VoiceId.Grace => "Grace",
            KittenTts2VoiceId.Reginald => "Reginald",
            KittenTts2VoiceId.Iris => "Iris",
            KittenTts2VoiceId.Frank => "Frank",
            KittenTts2VoiceId.Serena => "Serena",
            KittenTts2VoiceId.Julian => "Julian",
            KittenTts2VoiceId.Eleanor => "Eleanor",
            KittenTts2VoiceId.Otis => "Otis",
            KittenTts2VoiceId.Vincent => "Vincent",
            KittenTts2VoiceId.Martha => "Martha",
            KittenTts2VoiceId.Sable => "Sable",
            KittenTts2VoiceId.Victoria => "Victoria",
            KittenTts2VoiceId.PreparedBruno => "PreparedBruno",
            KittenTts2VoiceId.Arabic => "Arabic",
            KittenTts2VoiceId.Chinese => "Chinese",
            KittenTts2VoiceId.French => "French",
            KittenTts2VoiceId.German => "German",
            KittenTts2VoiceId.Hindi => "Hindi",
            KittenTts2VoiceId.Italian => "Italian",
            KittenTts2VoiceId.Portuguese => "Portuguese",
            KittenTts2VoiceId.Russian => "Russian",
            KittenTts2VoiceId.Spanish => "Spanish",
            KittenTts2VoiceId.AirportAnnouncer => "AirportAnnouncer",
            KittenTts2VoiceId.ArabicChildSchool => "ArabicChildSchool",
            KittenTts2VoiceId.ArabicFalconer => "ArabicFalconer",
            KittenTts2VoiceId.ArabicMother => "ArabicMother",
            KittenTts2VoiceId.ArabicNews => "ArabicNews",
            KittenTts2VoiceId.AudiobookNarratorFemale => "AudiobookNarratorFemale",
            KittenTts2VoiceId.BoredTeenager => "BoredTeenager",
            KittenTts2VoiceId.CalmHostageNegotiator => "CalmHostageNegotiator",
            KittenTts2VoiceId.ChineseKidSpringFestival => "ChineseKidSpringFestival",
            KittenTts2VoiceId.ChineseLivestreamSeller => "ChineseLivestreamSeller",
            KittenTts2VoiceId.ChineseNewsCctv => "ChineseNewsCctv",
            KittenTts2VoiceId.ChineseXianxiaMaster => "ChineseXianxiaMaster",
            KittenTts2VoiceId.DutchBaker => "DutchBaker",
            KittenTts2VoiceId.DutchGrandpaStory => "DutchGrandpaStory",
            KittenTts2VoiceId.DutchWeatherPresenter => "DutchWeatherPresenter",
            KittenTts2VoiceId.FairytaleNarratorWhimsical => "FairytaleNarratorWhimsical",
            KittenTts2VoiceId.FrenchDocumentary => "FrenchDocumentary",
            KittenTts2VoiceId.FrenchGrandmereRecette => "FrenchGrandmereRecette",
            KittenTts2VoiceId.FrenchTeacher => "FrenchTeacher",
            KittenTts2VoiceId.FrenchWaiter => "FrenchWaiter",
            KittenTts2VoiceId.GermanKidBirthday => "GermanKidBirthday",
            KittenTts2VoiceId.GermanKneipe => "GermanKneipe",
            KittenTts2VoiceId.GermanProfessor => "GermanProfessor",
            KittenTts2VoiceId.GermanTrainAnnouncement => "GermanTrainAnnouncement",
            KittenTts2VoiceId.GrandmaBedtimeStory => "GrandmaBedtimeStory",
            KittenTts2VoiceId.GriefEulogy => "GriefEulogy",
            KittenTts2VoiceId.HindiBollywoodDrama => "HindiBollywoodDrama",
            KittenTts2VoiceId.HindiCollegeFriends => "HindiCollegeFriends",
            KittenTts2VoiceId.HindiDadiKahani => "HindiDadiKahani",
            KittenTts2VoiceId.HindiYogaTeacher => "HindiYogaTeacher",
            KittenTts2VoiceId.IndonesianNewsAnchor => "IndonesianNewsAnchor",
            KittenTts2VoiceId.IndonesianTourGuide => "IndonesianTourGuide",
            KittenTts2VoiceId.ItalianChildSchool => "ItalianChildSchool",
            KittenTts2VoiceId.ItalianFashionDesigner => "ItalianFashionDesigner",
            KittenTts2VoiceId.ItalianFootballFan => "ItalianFootballFan",
            KittenTts2VoiceId.ItalianGondolier => "ItalianGondolier",
            KittenTts2VoiceId.ItalianNonnaKitchen => "ItalianNonnaKitchen",
            KittenTts2VoiceId.ItalianOperaTenor => "ItalianOperaTenor",
            KittenTts2VoiceId.JapaneseAnimeHeroine => "JapaneseAnimeHeroine",
            KittenTts2VoiceId.JapaneseGrandma => "JapaneseGrandma",
            KittenTts2VoiceId.JapaneseSalarymanApology => "JapaneseSalarymanApology",
            KittenTts2VoiceId.JapaneseTrainConductor => "JapaneseTrainConductor",
            KittenTts2VoiceId.KoreanGrandpaAdvice => "KoreanGrandpaAdvice",
            KittenTts2VoiceId.KoreanKdramaConfession => "KoreanKdramaConfession",
            KittenTts2VoiceId.KoreanNewsAnchor => "KoreanNewsAnchor",
            KittenTts2VoiceId.LittleGirlExplaining => "LittleGirlExplaining",
            KittenTts2VoiceId.NervousFirstDate => "NervousFirstDate",
            KittenTts2VoiceId.NewsAnchorFemale => "NewsAnchorFemale",
            KittenTts2VoiceId.PilotCabinAnnouncement => "PilotCabinAnnouncement",
            KittenTts2VoiceId.PolishBabciaPierogi => "PolishBabciaPierogi",
            KittenTts2VoiceId.PolishMountainGuide => "PolishMountainGuide",
            KittenTts2VoiceId.PolishNewsAnchor => "PolishNewsAnchor",
            KittenTts2VoiceId.PolishPoetryStudent => "PolishPoetryStudent",
            KittenTts2VoiceId.PortugueseBeach => "PortugueseBeach",
            KittenTts2VoiceId.PortugueseBusiness => "PortugueseBusiness",
            KittenTts2VoiceId.PortugueseFado => "PortugueseFado",
            KittenTts2VoiceId.PortugueseGrandpaFarm => "PortugueseGrandpaFarm",
            KittenTts2VoiceId.PortugueseKidFutebol => "PortugueseKidFutebol",
            KittenTts2VoiceId.PortugueseNewsAnchor => "PortugueseNewsAnchor",
            KittenTts2VoiceId.RussianBabushka => "RussianBabushka",
            KittenTts2VoiceId.RussianFairyTale => "RussianFairyTale",
            KittenTts2VoiceId.RussianHockeyCheer => "RussianHockeyCheer",
            KittenTts2VoiceId.RussianLiteratureTeacher => "RussianLiteratureTeacher",
            KittenTts2VoiceId.RussianSpaceControl => "RussianSpaceControl",
            KittenTts2VoiceId.RussianTaxiDriver => "RussianTaxiDriver",
            KittenTts2VoiceId.SarcasticCoworker => "SarcasticCoworker",
            KittenTts2VoiceId.SchoolboyPresentation => "SchoolboyPresentation",
            KittenTts2VoiceId.SpanishAbuela => "SpanishAbuela",
            KittenTts2VoiceId.SpanishBreakupSad => "SpanishBreakupSad",
            KittenTts2VoiceId.SpanishChildCuento => "SpanishChildCuento",
            KittenTts2VoiceId.SpanishNews => "SpanishNews",
            KittenTts2VoiceId.SpanishTango => "SpanishTango",
            KittenTts2VoiceId.TurkishFisherman => "TurkishFisherman",
            KittenTts2VoiceId.TurkishGrandmaCooking => "TurkishGrandmaCooking",
            KittenTts2VoiceId.TurkishNewsAnchor => "TurkishNewsAnchor",
            KittenTts2VoiceId.WeatherPresenterCheerful => "WeatherPresenterCheerful",
            _ => throw new ArgumentException("kitten_tts2 clone voice_id must be one of: Bella, Jasper, Luna, Bruno, Rosie, Hugo, Kiki, Leo, Matthew, Elliot, Willow, Dolores, Victor, Dante, Alfred, Saoirse, Claire, Raven, Marcus, Herbert, Diana, Laurence, Maeve, Walter, Edith, Miles, Grace, Reginald, Iris, Frank, Serena, Julian, Eleanor, Otis, Vincent, Martha, Sable, Victoria, PreparedBruno, Arabic, Chinese, French, German, Hindi, Italian, Portuguese, Russian, Spanish, AirportAnnouncer, ArabicChildSchool, ArabicFalconer, ArabicMother, ArabicNews, AudiobookNarratorFemale, BoredTeenager, CalmHostageNegotiator, ChineseKidSpringFestival, ChineseLivestreamSeller, ChineseNewsCctv, ChineseXianxiaMaster, DutchBaker, DutchGrandpaStory, DutchWeatherPresenter, FairytaleNarratorWhimsical, FrenchDocumentary, FrenchGrandmereRecette, FrenchTeacher, FrenchWaiter, GermanKidBirthday, GermanKneipe, GermanProfessor, GermanTrainAnnouncement, GrandmaBedtimeStory, GriefEulogy, HindiBollywoodDrama, HindiCollegeFriends, HindiDadiKahani, HindiYogaTeacher, IndonesianNewsAnchor, IndonesianTourGuide, ItalianChildSchool, ItalianFashionDesigner, ItalianFootballFan, ItalianGondolier, ItalianNonnaKitchen, ItalianOperaTenor, JapaneseAnimeHeroine, JapaneseGrandma, JapaneseSalarymanApology, JapaneseTrainConductor, KoreanGrandpaAdvice, KoreanKdramaConfession, KoreanNewsAnchor, LittleGirlExplaining, NervousFirstDate, NewsAnchorFemale, PilotCabinAnnouncement, PolishBabciaPierogi, PolishMountainGuide, PolishNewsAnchor, PolishPoetryStudent, PortugueseBeach, PortugueseBusiness, PortugueseFado, PortugueseGrandpaFarm, PortugueseKidFutebol, PortugueseNewsAnchor, RussianBabushka, RussianFairyTale, RussianHockeyCheer, RussianLiteratureTeacher, RussianSpaceControl, RussianTaxiDriver, SarcasticCoworker, SchoolboyPresentation, SpanishAbuela, SpanishBreakupSad, SpanishChildCuento, SpanishNews, SpanishTango, TurkishFisherman, TurkishGrandmaCooking, TurkishNewsAnchor, WeatherPresenterCheerful"),
            };
        }

        if (Preset != null && Preset != KittenTts2Preset.Stable)
        {
            options["preset"] = Preset switch
            {
            KittenTts2Preset.Stable => "stable",
            KittenTts2Preset.Expressive => "expressive",
            _ => throw new ArgumentException("kitten_tts2 clone preset must be one of: stable, expressive"),
            };
        }

        if (Temperature != null)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinP != null)
        {
            options["min_p"] = MinP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.1)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null && TextChunkSize != 380)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != KittenTts2TextChunkMode.TagAware)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            KittenTts2TextChunkMode.Default => "default",
            KittenTts2TextChunkMode.TagAware => "tag_aware",
            KittenTts2TextChunkMode.Japanese => "japanese",
            KittenTts2TextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("kitten_tts2 clone text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        return options;
    }
}
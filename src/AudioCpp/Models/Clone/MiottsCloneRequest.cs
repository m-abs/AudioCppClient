using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>miotts</c> spec, clone task (model default <c>miotts_1_7b_q8_0</c>).</summary>
public sealed record MiottsCloneRequest : CloneRequest
{
    /// <summary>Maximum generated tokens; default 700.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Generation control; defaults to the checkpoint config.</summary>
    public long? TopK { get; init; } = 50;

    /// <summary>Generation control; defaults to the checkpoint config.</summary>
    public double? TopP { get; init; } = 1;

    /// <summary>Generation control; defaults to the checkpoint config.</summary>
    public double? Temperature { get; init; } = 0.8;

    /// <summary>Generation control; defaults to the checkpoint config.</summary>
    public double? RepetitionPenalty { get; init; } = 1;

    /// <summary>Presence penalty; default 0.</summary>
    public double? PresencePenalty { get; init; }

    /// <summary>Frequency penalty; default 0.</summary>
    public double? FrequencyPenalty { get; init; }

    /// <summary>Generation control; defaults to the checkpoint config.</summary>
    public bool? DoSample { get; init; } = false;

    /// <summary>Sampling seed; omitted chooses a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Candidate count, capped by the session maximum; greater than one enables best-of-N.</summary>
    public long? BestOfN { get; init; } = 1;

    /// <summary>Override the session best-of-N setting.</summary>
    public bool? BestOfNEnabled { get; init; } = false;

    /// <summary>ASR language used to rank candidates.</summary>
    public MiottsBestOfNLanguage? BestOfNLanguage { get; init; }

    /// <summary>Framework long-form text chunk size; inherits the session setting.</summary>
    public long? TextChunkSize { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("miotts clone max_tokens must be at least 1", "MaxTokens");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("miotts clone top_p must be between 0 and 1", "TopP");
        }

        if (Temperature != null && (Temperature.Value < 0 || Temperature.Value > 2))
        {
            throw new ArgumentException("miotts clone temperature must be between 0 and 2", "Temperature");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 1 || RepetitionPenalty.Value > 1.5))
        {
            throw new ArgumentException("miotts clone repetition_penalty must be between 1 and 1.5", "RepetitionPenalty");
        }

        if (PresencePenalty != null && (PresencePenalty.Value < 0 || PresencePenalty.Value > 1))
        {
            throw new ArgumentException("miotts clone presence_penalty must be between 0 and 1", "PresencePenalty");
        }

        if (FrequencyPenalty != null && (FrequencyPenalty.Value < 0 || FrequencyPenalty.Value > 1))
        {
            throw new ArgumentException("miotts clone frequency_penalty must be between 0 and 1", "FrequencyPenalty");
        }

        if (BestOfN != null && (BestOfN.Value < 1))
        {
            throw new ArgumentException("miotts clone best_of_n must be at least 1", "BestOfN");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("miotts clone text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (MaxTokens != null)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (PresencePenalty != null)
        {
            options["presence_penalty"] = PresencePenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FrequencyPenalty != null)
        {
            options["frequency_penalty"] = FrequencyPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DoSample != null && DoSample != false)
        {
            options["do_sample"] = DoSample.Value ? "true" : "false";
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (BestOfN != null)
        {
            options["best_of_n"] = BestOfN.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (BestOfNEnabled != null && BestOfNEnabled != false)
        {
            options["best_of_n_enabled"] = BestOfNEnabled.Value ? "true" : "false";
        }

        if (BestOfNLanguage != null)
        {
            options["best_of_n_language"] = BestOfNLanguage switch
            {
            MiottsBestOfNLanguage.Auto => "auto",
            MiottsBestOfNLanguage.En => "en",
            MiottsBestOfNLanguage.Ja => "ja",
            _ => throw new ArgumentException("miotts clone best_of_n_language must be one of: auto, en, ja"),
            };
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}
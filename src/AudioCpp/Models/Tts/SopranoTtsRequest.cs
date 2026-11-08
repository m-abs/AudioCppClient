using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>soprano_tts</c> spec, tts task (model default <c>soprano_1_1_80m_q8_0</c>).</summary>
public sealed record SopranoTtsRequest : TtsRequest
{
    /// <summary>Maximum generated audio frames for the autoregressive LM; default 512.</summary>
    public long? MaxTokens { get; init; } = 512;

    /// <summary>Autoregressive sampling temperature; must be &gt; 0, default 0.3.</summary>
    public double? Temperature { get; init; } = 0.3;

    /// <summary>Nucleus sampling probability; default 0.95. Values &gt;=1 disable nucleus filtering.</summary>
    public double? TopP { get; init; } = 0.95;

    /// <summary>Repetition penalty applied to the LM head; default 1.2. Must be positive; values below 1 are accepted.</summary>
    public double? RepetitionPenalty { get; init; } = 1.2;

    /// <summary>Additive bias on the EOS token logit during generation. Positive values make the model stop sooner when speech ends (mitigating runaway generations that hit max_tokens); negative values encourage longer utterances. Default 0 disables the adjustment.</summary>
    public double? EosBias { get; init; } = 0;

    /// <summary>Autoregressive sampling seed; omitted requests choose a random seed.</summary>
    public string? Seed { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("soprano_tts tts max_tokens must be at least 1", "MaxTokens");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("soprano_tts tts temperature must be at least 0", "Temperature");
        }

        if (TopP != null && (TopP.Value < 0))
        {
            throw new ArgumentException("soprano_tts tts top_p must be at least 0", "TopP");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (MaxTokens != null && MaxTokens != 512)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0.3)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.95)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.2)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (EosBias != null && EosBias != 0)
        {
            options["eos_bias"] = EosBias.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        return options;
    }
}
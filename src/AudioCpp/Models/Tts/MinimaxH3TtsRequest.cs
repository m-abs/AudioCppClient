using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>minimax_h3</c> spec, tts task (model default <c>minimax_h3_q4_k</c>).</summary>
public sealed record MinimaxH3TtsRequest : TtsRequest
{
    /// <summary>Positive joint DiT denoising step count; default 50.</summary>
    public long? NumInferenceSteps { get; init; } = 12;

    /// <summary>Unsigned 32-bit generation seed in [0, 4294967295] for deterministic initial latents; default 42.</summary>
    public string? Seed { get; init; } = "42";

    /// <summary>Target video canvas height used by the joint T2VA latent layout; omitted or nonpositive values use 768. Smaller positive sizes are rounded up to the supported geometry.</summary>
    public long? Height { get; init; } = 768;

    /// <summary>Target video canvas width used by the joint T2VA latent layout; omitted or nonpositive values use 1344. Smaller positive sizes are rounded up to the supported geometry.</summary>
    public long? Width { get; init; } = 1344;

    /// <summary>Target video frame count before H3 alignment; omitted or nonpositive values use 124. MiniMax-H3 video is 24 fps; positive counts are rounded to the supported temporal geometry.</summary>
    public long? NumFrames { get; init; } = 241;

    /// <summary>Classifier-free guidance scale; 1.0 matches the baseline T2VA audio run.</summary>
    public double? GuidanceScale { get; init; } = 1;

    /// <summary>Joint DiT sampler: euler, res_multistep, dpmpp_2m, or unipc.</summary>
    public MinimaxH3SamplerMode? SamplerMode { get; init; } = MinimaxH3SamplerMode.Euler;

    /// <summary>Negative prompt used by classifier-free guidance when guidance_scale is not 1.0.</summary>
    public string? NegativePrompt { get; init; } = " ";

    /// <summary>Video FlowMatch scheduler shift.</summary>
    public double? FlowShift { get; init; } = 12;

    /// <summary>Audio FlowMatch scheduler shift.</summary>
    public double? AudioFlowShift { get; init; } = 3;

    /// <summary>Decode and return RGB24 video frames as an output artifact.</summary>
    public bool? ReturnVideo { get; init; } = false;

    /// <summary>Run the prompt encoder with scoped layer-group weights instead of a full prompt encoder weight store.</summary>
    public bool? TextLayerwise { get; init; } = false;

    /// <summary>Prompt encoder layer-group size when text_layerwise is enabled.</summary>
    public long? TextLayerwiseBatch { get; init; } = 1;

    /// <summary>Run the DiT denoiser with scoped prelude, block-group, and final weights instead of a full DiT weight store.</summary>
    public bool? DitLayerwise { get; init; } = false;

    /// <summary>DiT block-group size when dit_layerwise is enabled.</summary>
    public long? DitLayerwiseBatch { get; init; } = 1;

    /// <summary>Token chunk size for the per-token DiT MLP in layerwise high-resolution mode. Zero disables chunking.</summary>
    public long? DitMlpChunkTokens { get; init; } = 0;

    /// <summary>Optional DiT acceleration mode: none, first_block_cache, or spectrum. Requires dit_layerwise=false and sampler_mode=euler; Spectrum additionally requires guidance_scale=1.</summary>
    public string? DitAcceleration { get; init; } = "none";

    /// <summary>First-block cache residual-difference threshold.</summary>
    public double? FirstBlockCacheThreshold { get; init; } = 0.1;

    /// <summary>Denoising progress fraction where first-block caching begins; must be below end_percent. Used to derive the sigma window when explicit sigma endpoints are omitted.</summary>
    public double? FirstBlockCacheStartPercent { get; init; } = 0.1;

    /// <summary>Denoising progress fraction where first-block caching ends; must exceed start_percent. Used to derive the sigma window when explicit sigma endpoints are omitted.</summary>
    public double? FirstBlockCacheEndPercent { get; init; } = 0.95;

    /// <summary>Largest video sigma where first-block cache may be used. Supply together with first_block_cache_end_sigma; otherwise both endpoints are derived from the denoising progress fractions and schedule.</summary>
    public double? FirstBlockCacheStartSigma { get; init; }

    /// <summary>Smallest video sigma where first-block cache may be used. Supply together with first_block_cache_start_sigma; otherwise both endpoints are derived from the denoising progress fractions and schedule.</summary>
    public double? FirstBlockCacheEndSigma { get; init; }

    /// <summary>Maximum consecutive first-block cache hits before forcing a full DiT step.</summary>
    public long? FirstBlockCacheMaxConsecutive { get; init; } = 2;

    /// <summary>Full DiT warmup steps before spectrum forecasting may skip steps.</summary>
    public long? SpectrumWarmupSteps { get; init; } = 1;

    /// <summary>Initial spectrum forecast scheduling window.</summary>
    public double? SpectrumInitialWindow { get; init; } = 2;

    /// <summary>Spectrum forecast window growth after actual DiT steps.</summary>
    public double? SpectrumFlexWindow { get; init; } = 0.75;

    /// <summary>Chebyshev polynomial degree for spectrum forecasting.</summary>
    public long? SpectrumDegree { get; init; } = 1;

    /// <summary>Maximum spectrum forecast history rows.</summary>
    public long? SpectrumHistorySize { get; init; } = 8;

    /// <summary>Ridge regularization for spectrum forecasting.</summary>
    public double? SpectrumRidgeLambda { get; init; } = 0.1;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("minimax_h3 tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (TextLayerwiseBatch != null && (TextLayerwiseBatch.Value < 1))
        {
            throw new ArgumentException("minimax_h3 tts text_layerwise_batch must be at least 1", "TextLayerwiseBatch");
        }

        if (DitLayerwiseBatch != null && (DitLayerwiseBatch.Value < 1))
        {
            throw new ArgumentException("minimax_h3 tts dit_layerwise_batch must be at least 1", "DitLayerwiseBatch");
        }

        if (DitMlpChunkTokens != null && (DitMlpChunkTokens.Value < 0))
        {
            throw new ArgumentException("minimax_h3 tts dit_mlp_chunk_tokens must be at least 0", "DitMlpChunkTokens");
        }

        if (FirstBlockCacheThreshold != null && (FirstBlockCacheThreshold.Value < 0))
        {
            throw new ArgumentException("minimax_h3 tts first_block_cache_threshold must be at least 0", "FirstBlockCacheThreshold");
        }

        if (FirstBlockCacheStartPercent != null && (FirstBlockCacheStartPercent.Value < 0 || FirstBlockCacheStartPercent.Value > 1))
        {
            throw new ArgumentException("minimax_h3 tts first_block_cache_start_percent must be between 0 and 1", "FirstBlockCacheStartPercent");
        }

        if (FirstBlockCacheEndPercent != null && (FirstBlockCacheEndPercent.Value < 0 || FirstBlockCacheEndPercent.Value > 1))
        {
            throw new ArgumentException("minimax_h3 tts first_block_cache_end_percent must be between 0 and 1", "FirstBlockCacheEndPercent");
        }

        if (FirstBlockCacheMaxConsecutive != null && (FirstBlockCacheMaxConsecutive.Value < 1))
        {
            throw new ArgumentException("minimax_h3 tts first_block_cache_max_consecutive must be at least 1", "FirstBlockCacheMaxConsecutive");
        }

        if (SpectrumWarmupSteps != null && (SpectrumWarmupSteps.Value < 1))
        {
            throw new ArgumentException("minimax_h3 tts spectrum_warmup_steps must be at least 1", "SpectrumWarmupSteps");
        }

        if (SpectrumInitialWindow != null && (SpectrumInitialWindow.Value < 1))
        {
            throw new ArgumentException("minimax_h3 tts spectrum_initial_window must be at least 1", "SpectrumInitialWindow");
        }

        if (SpectrumFlexWindow != null && (SpectrumFlexWindow.Value < 0))
        {
            throw new ArgumentException("minimax_h3 tts spectrum_flex_window must be at least 0", "SpectrumFlexWindow");
        }

        if (SpectrumDegree != null && (SpectrumDegree.Value < 0))
        {
            throw new ArgumentException("minimax_h3 tts spectrum_degree must be at least 0", "SpectrumDegree");
        }

        if (SpectrumHistorySize != null && (SpectrumHistorySize.Value < 1))
        {
            throw new ArgumentException("minimax_h3 tts spectrum_history_size must be at least 1", "SpectrumHistorySize");
        }

        if (SpectrumRidgeLambda != null && (SpectrumRidgeLambda.Value < 0))
        {
            throw new ArgumentException("minimax_h3 tts spectrum_ridge_lambda must be at least 0", "SpectrumRidgeLambda");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (NumInferenceSteps != null && NumInferenceSteps != 50)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "42")
        {
            options["seed"] = Seed;
        }

        if (Height != null && Height != 768)
        {
            options["height"] = Height.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Width != null && Width != 1344)
        {
            options["width"] = Width.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumFrames != null && NumFrames != 124)
        {
            options["num_frames"] = NumFrames.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 1)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SamplerMode != null && SamplerMode != MinimaxH3SamplerMode.Euler)
        {
            options["sampler_mode"] = SamplerMode switch
            {
            MinimaxH3SamplerMode.Euler => "euler",
            MinimaxH3SamplerMode.ResMultistep => "res_multistep",
            MinimaxH3SamplerMode.Dpmpp2m => "dpmpp_2m",
            MinimaxH3SamplerMode.Unipc => "unipc",
            _ => throw new ArgumentException("minimax_h3 tts sampler_mode must be one of: euler, res_multistep, dpmpp_2m, unipc"),
            };
        }

        if (NegativePrompt != null && NegativePrompt != " ")
        {
            options["negative_prompt"] = NegativePrompt;
        }

        if (FlowShift != null && FlowShift != 12)
        {
            options["flow_shift"] = FlowShift.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (AudioFlowShift != null && AudioFlowShift != 3)
        {
            options["audio_flow_shift"] = AudioFlowShift.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (ReturnVideo != null && ReturnVideo != false)
        {
            options["return_video"] = ReturnVideo.Value ? "true" : "false";
        }

        if (TextLayerwise != null && TextLayerwise != false)
        {
            options["text_layerwise"] = TextLayerwise.Value ? "true" : "false";
        }

        if (TextLayerwiseBatch != null && TextLayerwiseBatch != 1)
        {
            options["text_layerwise_batch"] = TextLayerwiseBatch.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DitLayerwise != null && DitLayerwise != false)
        {
            options["dit_layerwise"] = DitLayerwise.Value ? "true" : "false";
        }

        if (DitLayerwiseBatch != null && DitLayerwiseBatch != 1)
        {
            options["dit_layerwise_batch"] = DitLayerwiseBatch.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DitMlpChunkTokens != null && DitMlpChunkTokens != 0)
        {
            options["dit_mlp_chunk_tokens"] = DitMlpChunkTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DitAcceleration != null && DitAcceleration != "none")
        {
            options["dit_acceleration"] = DitAcceleration;
        }

        if (FirstBlockCacheThreshold != null && FirstBlockCacheThreshold != 0.1)
        {
            options["first_block_cache_threshold"] = FirstBlockCacheThreshold.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FirstBlockCacheStartPercent != null && FirstBlockCacheStartPercent != 0.1)
        {
            options["first_block_cache_start_percent"] = FirstBlockCacheStartPercent.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FirstBlockCacheEndPercent != null && FirstBlockCacheEndPercent != 0.95)
        {
            options["first_block_cache_end_percent"] = FirstBlockCacheEndPercent.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FirstBlockCacheStartSigma != null)
        {
            options["first_block_cache_start_sigma"] = FirstBlockCacheStartSigma.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FirstBlockCacheEndSigma != null)
        {
            options["first_block_cache_end_sigma"] = FirstBlockCacheEndSigma.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FirstBlockCacheMaxConsecutive != null && FirstBlockCacheMaxConsecutive != 2)
        {
            options["first_block_cache_max_consecutive"] = FirstBlockCacheMaxConsecutive.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpectrumWarmupSteps != null && SpectrumWarmupSteps != 1)
        {
            options["spectrum_warmup_steps"] = SpectrumWarmupSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpectrumInitialWindow != null && SpectrumInitialWindow != 2)
        {
            options["spectrum_initial_window"] = SpectrumInitialWindow.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpectrumFlexWindow != null && SpectrumFlexWindow != 0.75)
        {
            options["spectrum_flex_window"] = SpectrumFlexWindow.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpectrumDegree != null && SpectrumDegree != 1)
        {
            options["spectrum_degree"] = SpectrumDegree.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpectrumHistorySize != null && SpectrumHistorySize != 8)
        {
            options["spectrum_history_size"] = SpectrumHistorySize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpectrumRidgeLambda != null && SpectrumRidgeLambda != 0.1)
        {
            options["spectrum_ridge_lambda"] = SpectrumRidgeLambda.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}
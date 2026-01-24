using System;
using System.Collections.Generic;
using System.Linq;

using fin.language.equations.fixedFunction;
using fin.language.equations.fixedFunction.impl;
using fin.model;
using fin.schema.color;
using fin.util.enumerables;

using grezzo.schema.cmb;
using grezzo.schema.cmb.mats;

using mats_Material = grezzo.schema.cmb.mats.Material;

namespace grezzo.material;

/// <summary>
///   Shamelessly stolen from https://github.com/naclomi/noclip.website/blob/8b0de601d6d8f596683f0bdee61a9681a42512f9/src/oot3d/render.ts
/// </summary>
public sealed class CmbCombinerGenerator {
  private readonly mats_Material cmbMaterial_;
  private readonly IFixedFunctionEquations<FixedFunctionSource> equations_;
  private readonly IFixedFunctionRegisters registers_;
  private readonly IColorOps cOps_;
  private readonly IScalarOps sOps_;

  private int constColorIndex_;
  private Rgba32 constColor_;

  private IColorValue previousColor_;
  private IColorValue previousColorBuffer_;
  private IScalarValue previousAlpha_;
  private IScalarValue previousAlphaBuffer_;

  private IColorValue primaryColor_;
  private IScalarValue primaryAlpha_;

  private IColorValue ambientAndDiffuseLightColor_;
  private IScalarValue ambientAndDiffuseLightAlpha_;

  private IColorValue specularLightColor_;
  private IScalarValue specularLightAlpha_;

  private readonly bool hasVertexColors_;

  public CmbCombinerGenerator(mats_Material cmbMaterial,
                              bool hasVertexColors,
                              IFixedFunctionMaterial finMaterial) {
      this.cmbMaterial_ = cmbMaterial;
      this.equations_ = finMaterial.Equations;
      this.registers_ = finMaterial.Registers;

      this.cOps_ = this.equations_.ColorOps;
      this.sOps_ = this.equations_.ScalarOps;

      var bufferColor = cmbMaterial.bufferColor;
      this.previousColorBuffer_ =
          new ColorConstant(bufferColor[0], bufferColor[1], bufferColor[2]);
      this.previousAlphaBuffer_ = new ScalarConstant(bufferColor[3]);

      this.hasVertexColors_ = hasVertexColors;
  }

  // TODO: Color is way too bright in OoT, looks like it expects vertex color
  // based lighting. 
  public void AddCombiners(IReadOnlyList<Combiner?> cmbCombiners) {
      var usesShaderLighting =
          this.cmbMaterial_.isFragmentLightingEnabled &&
          cmbCombiners
              .Nonnull()
              .SelectMany(combiner
                              => combiner.colorSources.Concat(
                                  combiner.alphaSources))
              .Any(source
                       => source is TexCombinerSource.FRAGMENT_PRIMARY_COLOR
                                    or TexCombinerSource
                                        .FRAGMENT_SECONDARY_COLOR);
      var dependsOnLights =
          this.cmbMaterial_.isVertexLightingEnabled || usesShaderLighting;
      var needsToAddLightingToVertexColor =
          !usesShaderLighting && this.cmbMaterial_.isVertexLightingEnabled;

      if (!dependsOnLights) {
        this.ambientAndDiffuseLightColor_
            = this.equations_.CreateColorConstant(1);
        this.ambientAndDiffuseLightAlpha_
            = this.equations_.CreateScalarConstant(1);
        this.specularLightColor_ = this.equations_.CreateColorConstant(1);
        this.specularLightAlpha_ = this.equations_.CreateScalarConstant(1);
      } else {
        // TODO: Is this lighting calculation right??
        var ambientRgba = this.cmbMaterial_.AmbientColor;
        var ambientColor =
            new ColorConstant(ambientRgba.Rf, ambientRgba.Gf, ambientRgba.Bf);
        var ambientAlpha = new ScalarConstant(ambientRgba.Af);

        var diffuseRgba = this.cmbMaterial_.DiffuseRgba;
        var diffuseColor =
            new ColorConstant(diffuseRgba.Rf, diffuseRgba.Gf, diffuseRgba.Bf);
        var diffuseAlpha = new ScalarConstant(diffuseRgba.Af);

        var specularRgba0 = this.cmbMaterial_.Specular0Color;
        var specularColor0 =
            new ColorConstant(specularRgba0.Rf,
                              specularRgba0.Gf,
                              specularRgba0.Bf);
        var specularAlpha0 = new ScalarConstant(specularRgba0.Af);

        var specularRgba1 = this.cmbMaterial_.Specular1Color;
        var specularColor1 =
            new ColorConstant(specularRgba1.Rf,
                              specularRgba1.Gf,
                              specularRgba1.Bf);
        var specularAlpha1 = new ScalarConstant(specularRgba1.Af);

        this.ambientAndDiffuseLightColor_ =
            this.cOps_.Add(
                this.cOps_.Multiply(
                    this.equations_.CreateOrGetColorInput(
                        FixedFunctionSource.LIGHT_AMBIENT_COLOR),
                    ambientColor),
                this.cOps_.Multiply(
                    this.equations_.GetMergedLightDiffuseColor(),
                    diffuseColor));
        this.ambientAndDiffuseLightAlpha_ =
            this.sOps_.Add(
                this.sOps_.Multiply(
                    this.equations_.CreateOrGetScalarInput(
                        FixedFunctionSource.LIGHT_AMBIENT_ALPHA),
                    ambientAlpha),
                this.sOps_.Multiply(
                    this.equations_.GetMergedLightDiffuseAlpha(),
                    diffuseAlpha));

        this.specularLightColor_ =
            this.cOps_.Multiply(
                this.equations_.GetMergedLightSpecularColor(),
                this.cOps_.Add(specularColor0, specularColor1));
        this.specularLightAlpha_ =
            this.sOps_.Multiply(
                this.equations_.GetMergedLightSpecularAlpha(),
                this.sOps_.Add(specularAlpha0, specularAlpha1));
      }

      this.primaryColor_ = this.hasVertexColors_ ? this.equations_.CreateOrGetColorInput(
          FixedFunctionSource.VERTEX_COLOR_0) : this.cOps_.One;
      this.primaryAlpha_ = this.hasVertexColors_ ? this.equations_.CreateOrGetScalarInput(
          FixedFunctionSource.VERTEX_ALPHA_0) : this.sOps_.One;

      if (needsToAddLightingToVertexColor) {
        this.primaryColor_ = this.cOps_.Multiply(
            this.primaryColor_,
            this.cOps_.Add(this.ambientAndDiffuseLightColor_,
                           this.specularLightColor_));
      }

      foreach (var cmbCombiner in cmbCombiners) {
        if (cmbCombiner == null) {
          continue;
        }

        this.AddCombiner_(cmbCombiner);
      }

      this.equations_.CreateColorOutput(
          FixedFunctionSource.OUTPUT_COLOR,
          this.previousColor_ ?? this.cOps_.Zero);
      this.equations_.CreateScalarOutput(
          FixedFunctionSource.OUTPUT_ALPHA,
          this.previousAlpha_ ?? this.sOps_.Zero);
    }

  public void AddCombiner_(Combiner cmbCombiner) {
      this.constColorIndex_ = cmbCombiner.constColorIndex;
      this.constColor_ =
          this.cmbMaterial_.ConstantColors[this.constColorIndex_];

      // Combine values
      var colorSources =
          cmbCombiner.colorSources
                     .Zip(cmbCombiner.colorOperands)
                     .Select(this.GetOppedSourceColor_)
                     .ToArray();
      var newPreviousColor = this.Combine_(
          this.cOps_,
          colorSources,
          cmbCombiner.combinerModeColor,
          cmbCombiner.scaleColor);

      var alphaSources =
          cmbCombiner.alphaSources
                     .Zip(cmbCombiner.alphaOperands)
                     .Select(this.GetOppedSourceAlpha_)
                     .ToArray();
      var newPreviousAlpha = this.Combine_(
          this.sOps_,
          alphaSources,
          cmbCombiner.combinerModeAlpha,
          cmbCombiner.scaleAlpha);

      // Get buffer
      var newPreviousColorBuffer = cmbCombiner.bufferColor switch {
          TexBufferSource.PREVIOUS_BUFFER => this.previousColorBuffer_,
          TexBufferSource.PREVIOUS       => this.previousColor_,
          _                              => throw new ArgumentOutOfRangeException()
      };
      var newPreviousAlphaBuffer = cmbCombiner.bufferAlpha switch {
          TexBufferSource.PREVIOUS_BUFFER => this.previousAlphaBuffer_,
          TexBufferSource.PREVIOUS       => this.previousAlpha_,
          _                              => throw new ArgumentOutOfRangeException()
      };

      this.previousColor_ = newPreviousColor;
      if (newPreviousColor != null) {
        newPreviousColor.Clamp = true;
      }

      this.previousColorBuffer_ = newPreviousColorBuffer;
      this.previousAlpha_ = newPreviousAlpha;
      this.previousAlphaBuffer_ = newPreviousAlphaBuffer;
    }

  private IColorValue GetOppedSourceColor_(
      (TexCombinerSource combinerSource, TexCombinerColorOp colorOp) input) {
      var (combinerSource, colorOp) = input;

      if (colorOp is TexCombinerColorOp.COLOR
                     or TexCombinerColorOp.ONE_MINUS_COLOR) {
        var colorValue = this.GetColorValue_(combinerSource);

        if (colorOp is TexCombinerColorOp.ONE_MINUS_COLOR) {
          colorValue = this.cOps_.Subtract(this.cOps_.One, colorValue);
        }

        return colorValue;
      }

      var channelValue = this.GetScalarValue_(
          combinerSource,
          this.GetChannelAndIsOneMinus_(
              (TexCombinerAlphaOp) colorOp));
      return new ColorWrapper(channelValue);
    }

  private IScalarValue GetOppedSourceAlpha_(
      (TexCombinerSource combinerSource, TexCombinerAlphaOp alphaOp) input)
    => this.GetScalarValue_(input.combinerSource,
                            this.GetChannelAndIsOneMinus_(input.alphaOp));

  private IColorValue GetColorValue_(TexCombinerSource combinerSource)
    => combinerSource switch {
        TexCombinerSource.TEXTURE0 => this.equations_.CreateOrGetColorInput(
            FixedFunctionSource.TEXTURE_COLOR_0),
        TexCombinerSource.TEXTURE1 => this.equations_.CreateOrGetColorInput(
            FixedFunctionSource.TEXTURE_COLOR_1),
        TexCombinerSource.TEXTURE2 => this.equations_.CreateOrGetColorInput(
            FixedFunctionSource.TEXTURE_COLOR_2),
        TexCombinerSource.TEXTURE3 => this.equations_.CreateOrGetColorInput(
            FixedFunctionSource.TEXTURE_COLOR_3),
        TexCombinerSource.CONSTANT =>
            this.registers_.GetOrCreateColorRegister(
                $"3dsColor{this.constColorIndex_}",
                this.equations_.CreateColorConstant(
                    this.constColor_.Rf,
                    this.constColor_.Gf,
                    this.constColor_.Bf)),
        TexCombinerSource.PRIMARY_COLOR   => this.primaryColor_,
        TexCombinerSource.PREVIOUS       => this.previousColor_,
        TexCombinerSource.PREVIOUS_BUFFER => this.previousColorBuffer_,
        TexCombinerSource.FRAGMENT_PRIMARY_COLOR => this
            .ambientAndDiffuseLightColor_,
        TexCombinerSource.FRAGMENT_SECONDARY_COLOR => this.specularLightColor_,
        _                                        => throw new ArgumentOutOfRangeException(nameof(combinerSource), combinerSource, null)
    };

  private IScalarValue GetScalarValue_(
      TexCombinerSource combinerSource,
      (Channel, bool) channelAndIsOneMinus) {
      IScalarValue channelValue;

      var (channel, isOneMinus) = channelAndIsOneMinus;
      if (channel != Channel.A) {
        var colorValue = this.GetColorValue_(combinerSource);
        channelValue = channel switch {
            Channel.R => colorValue.R,
            Channel.G => colorValue.G,
            Channel.B => colorValue.B,
            _         => throw new ArgumentOutOfRangeException()
        };
      } else {
        channelValue = combinerSource switch {
            TexCombinerSource.TEXTURE0 =>
                this.equations_.CreateOrGetScalarInput(
                    FixedFunctionSource.TEXTURE_ALPHA_0),
            TexCombinerSource.TEXTURE1 =>
                this.equations_.CreateOrGetScalarInput(
                    FixedFunctionSource.TEXTURE_ALPHA_1),
            TexCombinerSource.TEXTURE2 =>
                this.equations_.CreateOrGetScalarInput(
                    FixedFunctionSource.TEXTURE_ALPHA_2),
            TexCombinerSource.TEXTURE3 =>
                this.equations_.CreateOrGetScalarInput(
                    FixedFunctionSource.TEXTURE_ALPHA_3),
            TexCombinerSource.CONSTANT =>
                this.registers_.GetOrCreateScalarRegister(
                    $"3dsAlpha{this.constColorIndex_}",
                    this.equations_.CreateScalarConstant(
                        this.constColor_.Af)),
            TexCombinerSource.PRIMARY_COLOR   => this.primaryAlpha_,
            TexCombinerSource.PREVIOUS       => this.previousAlpha_,
            TexCombinerSource.PREVIOUS_BUFFER => this.previousAlphaBuffer_,
            TexCombinerSource.FRAGMENT_PRIMARY_COLOR => this
                .ambientAndDiffuseLightAlpha_,
            TexCombinerSource.FRAGMENT_SECONDARY_COLOR =>
                this.specularLightAlpha_,
        };
      }

      if (isOneMinus) {
        channelValue = this.sOps_.Subtract(this.sOps_.One, channelValue);
      }

      return channelValue;
    }

  private enum Channel {
    R,
    G,
    B,
    A
  }

  private (Channel, bool) GetChannelAndIsOneMinus_(
      TexCombinerAlphaOp scalarOp)
    => scalarOp switch {
        TexCombinerAlphaOp.RED           => (Channel.R, false),
        TexCombinerAlphaOp.ONE_MINUS_RED   => (Channel.R, true),
        TexCombinerAlphaOp.GREEN         => (Channel.G, false),
        TexCombinerAlphaOp.ONE_MINUS_GREEN => (Channel.G, true),
        TexCombinerAlphaOp.BLUE          => (Channel.B, false),
        TexCombinerAlphaOp.ONE_MINUS_BLUE  => (Channel.B, true),
        TexCombinerAlphaOp.ALPHA         => (Channel.A, false),
        TexCombinerAlphaOp.ONE_MINUS_ALPHA => (Channel.A, true),
    };

  private TValue Combine_<TValue, TConstant>(
      IFixedFunctionOps<TValue, TConstant> fixedFunctionOps,
      IReadOnlyList<TValue> sources,
      TexCombineMode combineMode,
      TexCombineScale combineScale)
      where TValue : IValue<TValue>
      where TConstant : IConstant<TValue>, TValue {
      // TODO: Implement dot-product ones
      var combinedValue = combineMode switch {
          TexCombineMode.REPLACE => sources[0],
          TexCombineMode.MODULATE => fixedFunctionOps.Multiply(
              sources[0],
              sources[1]),
          TexCombineMode.ADD => fixedFunctionOps.Add(sources[0], sources[1]),
          TexCombineMode.ADD_SIGNED => fixedFunctionOps.Subtract(
              fixedFunctionOps.Add(sources[0], sources[1]),
              fixedFunctionOps.Half),
          TexCombineMode.SUBTRACT => fixedFunctionOps.Subtract(
              sources[0],
              sources[1]),
          TexCombineMode.MULT_ADD => fixedFunctionOps.Add(
              fixedFunctionOps.Multiply(sources[0], sources[1]),
              sources[2]),
          TexCombineMode.ADD_MULT => this.AddMult_(fixedFunctionOps, sources),
          TexCombineMode.INTERPOLATE => fixedFunctionOps.Add(
              fixedFunctionOps.Multiply(sources[0],
                                        fixedFunctionOps.Subtract(
                                            fixedFunctionOps.One,
                                            sources[2])),
              fixedFunctionOps.Multiply(sources[1], sources[2])),
          _ => throw new NotImplementedException()
      };

      return combineScale switch {
          TexCombineScale.ONE => combinedValue,
          TexCombineScale.TWO => fixedFunctionOps.MultiplyWithConstant(
              combinedValue,
              2),
          TexCombineScale.FOUR => fixedFunctionOps.MultiplyWithConstant(
              combinedValue,
              4),
          _ => throw new ArgumentOutOfRangeException(nameof(combineScale), combineScale, null)
      };
    }

  private TValue AddMult_<TValue, TConstant>(
      IFixedFunctionOps<TValue, TConstant> fixedFunctionOps,
      IReadOnlyList<TValue> sources
  ) where TValue : IValue<TValue>
    where TConstant : IConstant<TValue>, TValue {
      var addedValue = fixedFunctionOps.Add(sources[0], sources[1]);
      if (addedValue is IColorValue colorValue) {
        colorValue.Clamp = true;
      }

      var value = fixedFunctionOps.Multiply(addedValue, sources[2]);

      return value;
    }
}
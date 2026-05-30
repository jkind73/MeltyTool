using System;
using System.Collections.Generic;

using fin.language.equations.fixedFunction.util;
using fin.model;

namespace fin.language.equations.fixedFunction;

public enum LightingChannel {
  DIFFUSE,
  SPECULAR,
  AMBIENT,
  OTHER,
}

public class FixedFunctionsEquationsLightingExtractor {
  private readonly IDictionary<IValue, bool> dependsOnLightingChannel_
      = new Dictionary<IValue, bool>();

  public void ExtractLightingChannels(
      IFixedFunctionEquations<FixedFunctionSource> equations,
      out (IColorValue, IScalarValue) diffuseColorAlpha,
      out (IColorValue, IScalarValue) specularColorAlpha,
      out (IColorValue, IScalarValue) ambientColorAlpha,
      out (IColorValue, IScalarValue) otherColorAlpha) {
    var outputColor = equations.ColorOutputs[FixedFunctionSource.OUTPUT_COLOR];
    var outputAlpha = equations.ScalarOutputs[FixedFunctionSource.OUTPUT_ALPHA];

    diffuseColorAlpha = (
        this.ExtractLightingChannel(outputColor, LightingChannel.DIFFUSE),
        this.ExtractLightingChannel_(outputAlpha, LightingChannel.DIFFUSE));
    specularColorAlpha = (
        this.ExtractLightingChannel(outputColor, LightingChannel.SPECULAR),
        this.ExtractLightingChannel_(outputAlpha, LightingChannel.SPECULAR));
    ambientColorAlpha = (
        this.ExtractLightingChannel(outputColor, LightingChannel.AMBIENT),
        this.ExtractLightingChannel_(outputAlpha, LightingChannel.AMBIENT));
    otherColorAlpha = (
        this.ExtractLightingChannel(outputColor, LightingChannel.OTHER),
        this.ExtractLightingChannel_(outputAlpha, LightingChannel.OTHER));
  }

  public IColorValue ExtractLightingChannel(
      IColorValue src,
      LightingChannel channel) {
    var (foundChannel, value) = this.ExtractLightingChannelImpl_(src, channel, false);
    if (foundChannel || channel is LightingChannel.OTHER) {
      if (value.IsZero()) {
        return ColorConstant.ZERO;
      }

      if (value.IsOne()) {
        return ColorConstant.ONE;
      }

      return value;
    }

    return ColorConstant.ZERO;
  }

  public IScalarValue ExtractLightingChannel_(
      IScalarValue src,
      LightingChannel channel) {
    var (foundChannel, value) = this.ExtractLightingChannelImpl_(src, channel, false);
    if (foundChannel || channel is LightingChannel.OTHER) {
      if (value.IsZero()) {
        return ScalarConstant.ZERO;
      }

      if (value.IsOne()) {
        return ScalarConstant.ONE;
      }

      return value;
    }

    return ScalarConstant.ZERO;
  }

  public (bool, IColorValue) ExtractLightingChannelImpl_(
      IColorValue src,
      LightingChannel channel,
      bool foundChannelInParentTerm) {
    switch (src) {
      case IColorExpression colorExpression:
        return this.ExtractLightingChannelImplExpression_(colorExpression, channel, foundChannelInParentTerm);
      case IColorTerm colorTerm:
        return this.ExtractLightingChannelImplTerm_(colorTerm, channel, foundChannelInParentTerm);
      case IColorFactor colorFactor:
        return this.ExtractLightingChannelImplFactor_(colorFactor, channel, foundChannelInParentTerm);
      case IColorValueTernaryOperator colorValueTernary: {
        var (trueFoundChannel, trueValue)
            = this.ExtractLightingChannelImpl_(colorValueTernary.TrueValue, channel, foundChannelInParentTerm);
        var (falseFoundChannel, falseValue)
            = this.ExtractLightingChannelImpl_(colorValueTernary.FalseValue, channel, foundChannelInParentTerm);

        return (trueFoundChannel || falseFoundChannel,
                new ColorValueTernaryOperator {
                    ComparisonType = colorValueTernary.ComparisonType,
                    Lhs = colorValueTernary.Lhs,
                    Rhs = colorValueTernary.Rhs,
                    TrueValue = trueValue,
                    FalseValue = falseValue,
                });
      }
      default: throw new ArgumentOutOfRangeException(nameof(src));
    }
  }

  public (bool, IScalarValue) ExtractLightingChannelImpl_(
      IScalarValue src,
      LightingChannel channel,
      bool foundChannelInParentTerm) {
    switch (src) {
      case IScalarExpression scalarExpression:
        return this.ExtractLightingChannelImplExpression_(scalarExpression, channel, foundChannelInParentTerm);
      case IScalarTerm scalarTerm:
        return this.ExtractLightingChannelImplTerm_(scalarTerm, channel, foundChannelInParentTerm);
      case IScalarFactor scalarFactor:
        return this.ExtractLightingChannelImplFactor_(scalarFactor, channel, foundChannelInParentTerm);
      default: throw new ArgumentOutOfRangeException(nameof(src));
    }
  }

  private (bool, IColorValue) ExtractLightingChannelImplExpression_(
      IColorExpression src,
      LightingChannel channel,
      bool foundChannelInParentTerm) {
    var foundChannel = false;
    var newTerms = new List<IColorValue>();

    foreach (var oldTerm in src.Terms) {
      var (currentFoundChannel, value)
          = this.ExtractLightingChannelImpl_(oldTerm, channel, foundChannelInParentTerm);
      foundChannel |= currentFoundChannel;
      if ((foundChannelInParentTerm || currentFoundChannel || channel is LightingChannel.OTHER) &&
          !value.IsZero()) {
        newTerms.Add(value);
      }
    }

    if (newTerms.Count == 0) {
      return (foundChannel, ColorConstant.ZERO);
    }

    if (newTerms.Count == 1) {
      return (foundChannel, newTerms[0]);
    }

    return (foundChannel, new ColorExpression(newTerms));
  }

  private (bool, IScalarValue) ExtractLightingChannelImplExpression_(
      IScalarExpression src,
      LightingChannel channel,
      bool foundChannelInParentTerm) {
    var foundChannel = false;
    var newTerms = new List<IScalarValue>();

    foreach (var oldTerm in src.Terms) {
      var (currentFoundChannel, value)
          = this.ExtractLightingChannelImpl_(oldTerm, channel, foundChannelInParentTerm);
      foundChannel |= currentFoundChannel;
      if ((foundChannelInParentTerm || currentFoundChannel || channel is LightingChannel.OTHER) &&
          !value.IsZero()) {
        newTerms.Add(value);
      }
    }

    if (newTerms.Count == 0) {
      return (foundChannel, ScalarConstant.ZERO);
    }

    if (newTerms.Count == 1) {
      return (foundChannel, newTerms[0]);
    }

    return (foundChannel, new ScalarExpression(newTerms));
  }

  private (bool, IColorValue) ExtractLightingChannelImplTerm_(
      IColorTerm src,
      LightingChannel channel,
      bool foundChannelInParentTerm) {
    var foundChannel = false;
    var newNumeratorFactors = new List<IColorValue>();
    List<IColorValue>? newDenominatorFactors = null;

    foreach (var oldNumeratorFactor in FixedFunctionsEquationsUtil
                 .OrderFactorsWithChannelsFirst(this.dependsOnLightingChannel_,
                                                src.NumeratorFactors)) {
      var (currentFoundChannel, value)
          = this.ExtractLightingChannelImpl_(oldNumeratorFactor, channel, foundChannelInParentTerm);
      foundChannelInParentTerm |= foundChannel |= currentFoundChannel;

      if (value.IsZero()) {
        return (false, ColorConstant.ZERO);
      }

      if (!value.IsOne()) {
        newNumeratorFactors.Add(value);
      }
    }

    if (src.DenominatorFactors != null) {
      newDenominatorFactors = new List<IColorValue>();
      foreach (var oldDenominatorFactor in FixedFunctionsEquationsUtil
                   .OrderFactorsWithChannelsFirst(this.dependsOnLightingChannel_,
                                                  src.DenominatorFactors)) {
        var (currentFoundChannel, value)
            = this.ExtractLightingChannelImpl_(oldDenominatorFactor, channel, foundChannelInParentTerm);
        foundChannelInParentTerm |= foundChannel |= currentFoundChannel;
        if (!value.IsOne()) {
          newDenominatorFactors.Add(value);
        }
      }
    }

    if (newNumeratorFactors.Count == 0 &&
        (src.DenominatorFactors?.Count ?? 0) == 0) {
      return (foundChannel, ColorConstant.ONE);
    }

    if (newNumeratorFactors.Count == 1 &&
        (src.DenominatorFactors?.Count ?? 0) == 0) {
      return (foundChannel, newNumeratorFactors[0]);
    }

    return (foundChannel,
            new ColorTerm(newNumeratorFactors, newDenominatorFactors));
  }

  private (bool, IScalarValue) ExtractLightingChannelImplTerm_(
      IScalarTerm src,
      LightingChannel channel,
      bool foundChannelInParentTerm) {
    var foundChannel = false;
    var newNumeratorFactors = new List<IScalarValue>();
    List<IScalarValue>? newDenominatorFactors = null;

    foreach (var oldNumeratorFactor in FixedFunctionsEquationsUtil
                 .OrderFactorsWithChannelsFirst(this.dependsOnLightingChannel_,
                                                src.NumeratorFactors)) {
      var (currentFoundChannel, value)
          = this.ExtractLightingChannelImpl_(oldNumeratorFactor, channel, foundChannelInParentTerm);
      foundChannelInParentTerm |= foundChannel |= currentFoundChannel;

      if (value.IsZero()) {
        return (false, ScalarConstant.ZERO);
      }

      if (!value.IsOne()) {
        newNumeratorFactors.Add(value);
      }
    }

    if (src.DenominatorFactors != null) {
      newDenominatorFactors = new List<IScalarValue>();
      foreach (var oldDenominatorFactor in FixedFunctionsEquationsUtil
                   .OrderFactorsWithChannelsFirst(this.dependsOnLightingChannel_,
                                                  src.DenominatorFactors)) {
        var (currentFoundChannel, value)
            = this.ExtractLightingChannelImpl_(oldDenominatorFactor, channel, foundChannelInParentTerm);
        foundChannelInParentTerm |= foundChannel |= currentFoundChannel;
        if (!value.IsOne()) {
          newDenominatorFactors.Add(value);
        }
      }
    }

    if (newNumeratorFactors.Count == 0 &&
        (src.DenominatorFactors?.Count ?? 0) == 0) {
      return (foundChannel, ScalarConstant.ONE);
    }

    if (newNumeratorFactors.Count == 1 &&
        (src.DenominatorFactors?.Count ?? 0) == 0) {
      return (foundChannel, newNumeratorFactors[0]);
    }

    return (foundChannel,
            new ScalarTerm(newNumeratorFactors, newDenominatorFactors));
  }

  public (bool, IColorValue) ExtractLightingChannelImplFactor_(
      IColorFactor src,
      LightingChannel channel,
      bool foundChannelInParentTerm) {
    switch (src) {
      case ColorWrapper colorWrapper: {
        if (colorWrapper.Intensity != null) {
          var (foundChannel, value)
              = this.ExtractLightingChannelImpl_(colorWrapper.Intensity, channel, foundChannelInParentTerm);
          return (foundChannel, new ColorWrapper(value));
        }

        var (foundChannelR, valueR)
            = this.ExtractLightingChannelImpl_(colorWrapper.R, channel, foundChannelInParentTerm);
        var (foundChannelG, valueG)
            = this.ExtractLightingChannelImpl_(colorWrapper.G, channel, foundChannelInParentTerm);
        var (foundChannelB, valueB)
            = this.ExtractLightingChannelImpl_(colorWrapper.B, channel, foundChannelInParentTerm);
        return (foundChannelR || foundChannelG || foundChannelB,
                new ColorWrapper(valueR, valueG, valueB));
      }
      case IColorInput<FixedFunctionSource> colorInput: {
        return this.ExtractLightingChannelImplInput_(colorInput, channel);
      }
      case IColorOutput<FixedFunctionSource> colorOutput: {
        return this.ExtractLightingChannelImpl_(colorOutput.ColorValue, channel, foundChannelInParentTerm);
      }
      default: {
        return (false, src);
      }
    }
  }

  public (bool, IScalarValue) ExtractLightingChannelImplFactor_(
      IScalarFactor src,
      LightingChannel channel,
      bool foundChannelInParentTerm) {
    switch (src) {
      case IColorNamedValueSwizzle<FixedFunctionSource> colorNamedValueSwizzle
          : {
        var (foundChannel, value)
            = this.ExtractLightingChannelImpl_(colorNamedValueSwizzle.Source, channel, foundChannelInParentTerm);

        if (value is IColorIdentifiedValue<FixedFunctionSource>
            identifiedValue) {
          return (foundChannel,
                  new ColorNamedValueSwizzle<FixedFunctionSource>(
                      identifiedValue,
                      colorNamedValueSwizzle.SwizzleType));
        }

        return (false, ScalarConstant.ZERO);
      }
      case IColorValueSwizzle colorValueSwizzle: {
        var (foundChannel, value)
            = this.ExtractLightingChannelImpl_(colorValueSwizzle.Source, channel, foundChannelInParentTerm);
        return (foundChannel,
                new ColorValueSwizzle(value, colorValueSwizzle.SwizzleType));
      }
      case IScalarInput<FixedFunctionSource> scalarInput: {
        return this.ExtractLightingChannelImplInput_(scalarInput, channel);
      }
      case IScalarOutput scalarOutput: {
        return this.ExtractLightingChannelImpl_(scalarOutput.ScalarValue, channel, foundChannelInParentTerm);
      }
      default: {
        return (false, src);
      }
    }
  }

  public (bool, IColorFactor) ExtractLightingChannelImplInput_(
      IColorInput<FixedFunctionSource> src,
      LightingChannel channel) {
    var identifier = src.Identifier;
    if (identifier.IsDiffuse()) {
      if (channel is LightingChannel.DIFFUSE) {
        return (true, ColorConstant.ONE);
      }

      return (false, ColorConstant.ZERO);
    }

    if (identifier.IsSpecular()) {
      if (channel is LightingChannel.SPECULAR) {
        return (true, ColorConstant.ONE);
      }

      return (false, ColorConstant.ZERO);
    }

    if (identifier.IsAmbient()) {
      if (channel is LightingChannel.AMBIENT) {
        return (true, ColorConstant.ONE);
      }

      return (false, ColorConstant.ZERO);
    }

    return (false, src);
  }

  public (bool, IScalarFactor) ExtractLightingChannelImplInput_(
      IScalarInput<FixedFunctionSource> src,
      LightingChannel channel) {
    var identifier = src.Identifier;
    if (identifier.IsDiffuse()) {
      if (channel is LightingChannel.DIFFUSE) {
        return (true, ScalarConstant.ONE);
      }

      return (false, ScalarConstant.ZERO);
    }

    if (identifier.IsSpecular()) {
      if (channel is LightingChannel.SPECULAR) {
        return (true, ScalarConstant.ONE);
      }

      return (false, ScalarConstant.ZERO);
    }

    if (identifier.IsAmbient()) {
      if (channel is LightingChannel.AMBIENT) {
        return (true, ScalarConstant.ONE);
      }

      return (false, ScalarConstant.ZERO);
    }

    return (false, src);
  }
}
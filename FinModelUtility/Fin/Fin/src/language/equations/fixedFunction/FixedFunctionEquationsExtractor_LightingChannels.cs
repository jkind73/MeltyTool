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

public static partial class FixedFunctionsEquationsExtractor {
  public static void ExtractLightingChannels(
      IFixedFunctionEquations<FixedFunctionSource> equations,
      out (IColorValue, IScalarValue) diffuseColorAlpha,
      out (IColorValue, IScalarValue) specularColorAlpha,
      out (IColorValue, IScalarValue) ambientColorAlpha,
      out (IColorValue, IScalarValue) otherColorAlpha) {
    var outputColor = equations.ColorOutputs[FixedFunctionSource.OUTPUT_COLOR];
    var outputAlpha = equations.ScalarOutputs[FixedFunctionSource.OUTPUT_ALPHA];

    diffuseColorAlpha = (
        outputColor.ExtractLightingChannel(LightingChannel.DIFFUSE),
        outputAlpha.ExtractLightingChannel(LightingChannel.DIFFUSE));
    specularColorAlpha = (
        outputColor.ExtractLightingChannel(LightingChannel.SPECULAR),
        outputAlpha.ExtractLightingChannel(LightingChannel.SPECULAR));
    ambientColorAlpha = (
        outputColor.ExtractLightingChannel(LightingChannel.AMBIENT),
        outputAlpha.ExtractLightingChannel(LightingChannel.AMBIENT));
    otherColorAlpha = (
        outputColor.ExtractLightingChannel(LightingChannel.OTHER),
        outputAlpha.ExtractLightingChannel(LightingChannel.OTHER));
  }

  public static IColorValue ExtractLightingChannel(
      this IColorValue src,
      LightingChannel channel) {
    var (foundChannel, value) = src.ExtractLightingChannelImpl_(channel);
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

  public static IScalarValue ExtractLightingChannel(
      this IScalarValue src,
      LightingChannel channel) {
    var (foundChannel, value) = src.ExtractLightingChannelImpl_(channel);
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

  public static (bool, IColorValue) ExtractLightingChannelImpl_(
      this IColorValue src,
      LightingChannel channel) {
    switch (src) {
      case IColorExpression colorExpression:
        return colorExpression.ExtractLightingChannelImplExpression_(channel);
      case IColorTerm colorTerm:
        return colorTerm.ExtractLightingChannelImplTerm_(channel);
      case IColorFactor colorFactor:
        return colorFactor.ExtractLightingChannelImplFactor_(channel);
      case IColorValueTernaryOperator colorValueTernary: {
        var (trueFoundChannel, trueValue)
            = colorValueTernary.TrueValue.ExtractLightingChannelImpl_(channel);
        var (falseFoundChannel, falseValue)
            = colorValueTernary.TrueValue.ExtractLightingChannelImpl_(channel);

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

  public static (bool, IScalarValue) ExtractLightingChannelImpl_(
      this IScalarValue src,
      LightingChannel channel) {
    switch (src) {
      case IScalarExpression scalarExpression:
        return scalarExpression.ExtractLightingChannelImplExpression_(channel);
      case IScalarTerm scalarTerm:
        return scalarTerm.ExtractLightingChannelImplTerm_(channel);
      case IScalarFactor scalarFactor:
        return scalarFactor.ExtractLightingChannelImplFactor_(channel);
      default: throw new ArgumentOutOfRangeException(nameof(src));
    }
  }

  private static (bool, IColorValue) ExtractLightingChannelImplExpression_(
      this IColorExpression src,
      LightingChannel channel) {
    var foundChannel = false;
    var newTerms = new List<IColorValue>();

    foreach (var oldTerm in src.Terms) {
      var (currentFoundChannel, value)
          = oldTerm.ExtractLightingChannelImpl_(channel);
      foundChannel |= currentFoundChannel;
      if ((currentFoundChannel || channel is LightingChannel.OTHER) &&
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

  private static (bool, IScalarValue) ExtractLightingChannelImplExpression_(
      this IScalarExpression src,
      LightingChannel channel) {
    var foundChannel = false;
    var newTerms = new List<IScalarValue>();

    foreach (var oldTerm in src.Terms) {
      var (currentFoundChannel, value)
          = oldTerm.ExtractLightingChannelImpl_(channel);
      foundChannel |= currentFoundChannel;
      if ((currentFoundChannel || channel is LightingChannel.OTHER) &&
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

  private static (bool, IColorValue) ExtractLightingChannelImplTerm_(
      this IColorTerm src,
      LightingChannel channel) {
    var foundChannel = false;
    var newNumeratorFactors = new List<IColorValue>();
    List<IColorValue>? newDenominatorFactors = null;

    foreach (var oldNumeratorFactor in src.NumeratorFactors) {
      var (currentFoundChannel, value)
          = oldNumeratorFactor.ExtractLightingChannelImpl_(channel);
      foundChannel |= currentFoundChannel;

      if (value.IsZero()) {
        return (false, ColorConstant.ZERO);
      }

      if (!value.IsOne()) {
        newNumeratorFactors.Add(value);
      }
    }

    if (src.DenominatorFactors != null) {
      newDenominatorFactors = new List<IColorValue>();
      foreach (var oldDenominatorFactor in src.DenominatorFactors) {
        var (currentFoundChannel, value)
            = oldDenominatorFactor.ExtractLightingChannelImpl_(channel);
        foundChannel |= currentFoundChannel;
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

  private static (bool, IScalarValue) ExtractLightingChannelImplTerm_(
      this IScalarTerm src,
      LightingChannel channel) {
    var foundChannel = false;
    var newNumeratorFactors = new List<IScalarValue>();
    List<IScalarValue>? newDenominatorFactors = null;

    foreach (var oldNumeratorFactor in src.NumeratorFactors) {
      var (currentFoundChannel, value)
          = oldNumeratorFactor.ExtractLightingChannelImpl_(channel);
      foundChannel |= currentFoundChannel;

      if (value.IsZero()) {
        return (false, ScalarConstant.ZERO);
      }

      if (!value.IsOne()) {
        newNumeratorFactors.Add(value);
      }
    }

    if (src.DenominatorFactors != null) {
      newDenominatorFactors = new List<IScalarValue>();
      foreach (var oldDenominatorFactor in src.DenominatorFactors) {
        var (currentFoundChannel, value)
            = oldDenominatorFactor.ExtractLightingChannelImpl_(channel);
        foundChannel |= currentFoundChannel;
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

  public static (bool, IColorValue) ExtractLightingChannelImplFactor_(
      this IColorFactor src,
      LightingChannel channel) {
    switch (src) {
      case ColorWrapper colorWrapper: {
        if (colorWrapper.Intensity != null) {
          var (foundChannel, value)
              = colorWrapper.Intensity.ExtractLightingChannelImpl_(channel);
          return (foundChannel, new ColorWrapper(value));
        }

        var (foundChannelR, valueR)
            = colorWrapper.R.ExtractLightingChannelImpl_(channel);
        var (foundChannelG, valueG)
            = colorWrapper.G.ExtractLightingChannelImpl_(channel);
        var (foundChannelB, valueB)
            = colorWrapper.B.ExtractLightingChannelImpl_(channel);
        return (foundChannelR || foundChannelG || foundChannelB,
                new ColorWrapper(valueR, valueG, valueB));
      }
      case IColorInput<FixedFunctionSource> colorInput: {
        return colorInput.ExtractLightingChannelImplInput_(channel);
      }
      case IColorOutput<FixedFunctionSource> colorOutput: {
        return colorOutput.ColorValue.ExtractLightingChannelImpl_(channel);
      }
      default: {
        return (false, src);
      }
    }
  }

  public static (bool, IScalarValue) ExtractLightingChannelImplFactor_(
      this IScalarFactor src,
      LightingChannel channel) {
    switch (src) {
      case IColorNamedValueSwizzle<FixedFunctionSource> colorNamedValueSwizzle
          : {
        var (foundChannel, value)
            = colorNamedValueSwizzle.Source
                                    .ExtractLightingChannelImpl_(channel);

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
            = colorValueSwizzle.Source.ExtractLightingChannelImpl_(channel);
        return (foundChannel,
                new ColorValueSwizzle(value, colorValueSwizzle.SwizzleType));
      }
      case IScalarInput<FixedFunctionSource> scalarInput: {
        return scalarInput.ExtractLightingChannelImplInput_(channel);
      }
      case IScalarOutput scalarOutput: {
        return scalarOutput.ScalarValue.ExtractLightingChannelImpl_(channel);
      }
      default: {
        return (false, src);
      }
    }
  }

  public static (bool, IColorFactor) ExtractLightingChannelImplInput_(
      this IColorInput<FixedFunctionSource> src,
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

  public static (bool, IScalarFactor) ExtractLightingChannelImplInput_(
      this IScalarInput<FixedFunctionSource> src,
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
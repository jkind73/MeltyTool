using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.data.queues;
using fin.language.equations.fixedFunction.util;
using fin.math;
using fin.model;
using fin.model.util;
using fin.util.enumerables;

namespace fin.language.equations.fixedFunction;

public static partial class FixedFunctionsEquationsUtil {
  public static (IReadOnlyTexture? primaryTexture, Vector4 primaryColor)
      ExtractPrimaryTextureAndColor(
          IFixedFunctionMaterial material,
          (IColorValue, IScalarValue) srcColorAlpha) {
    var (srcColor, srcAlpha) = srcColorAlpha;

    IReadOnlyTexture? primaryTexture = null;
    Vector4 primaryColor = Vector4.One;

    if (srcColor.IsZero()) {
      primaryColor.X = primaryColor.Y = primaryColor.Z = 0;
    } else if (srcColor.IsOne()) {
      primaryColor.X = primaryColor.Y = primaryColor.Z = 1;
    } else {
      var foundColor = false;
      var foundTextures = new HashSet<IReadOnlyTexture>();

      // TODO: Surely there's a better way to do this...
      var colorQueue = new FinTuple2Queue<IColorValue, bool>((srcColor, false));
      while (colorQueue.TryDequeue(out var colorValue, out var inDenominator)) {
        switch (colorValue) {
          case IColorConstant colorConstant: {
            if (!foundColor &&
                colorConstant.RValue.IsInRange(0, 1) &&
                colorConstant.GValue.IsInRange(0, 1) &&
                colorConstant.BValue.IsInRange(0, 1)) {
              foundColor = true;
              if (!inDenominator) {
                primaryColor.X = colorConstant.RValue;
                primaryColor.Y = colorConstant.GValue;
                primaryColor.Z = colorConstant.BValue;
              } else {
                primaryColor.X = 1 / colorConstant.RValue;
                primaryColor.Y = 1 / colorConstant.GValue;
                primaryColor.Z = 1 / colorConstant.BValue;
              }
            }

            break;
          }
          case IColorRegister colorRegister: {
            var colorConstant = colorRegister.DefaultValue;

            if (!foundColor &&
                colorConstant.RValue.IsInRange(0, 1) &&
                colorConstant.GValue.IsInRange(0, 1) &&
                colorConstant.BValue.IsInRange(0, 1)) {
              foundColor = true;
              if (!inDenominator) {
                primaryColor.X = colorConstant.RValue;
                primaryColor.Y = colorConstant.GValue;
                primaryColor.Z = colorConstant.BValue;
              } else {
                primaryColor.X = 1 / colorConstant.RValue;
                primaryColor.Y = 1 / colorConstant.GValue;
                primaryColor.Z = 1 / colorConstant.BValue;
              }
            }

            break;
          }
          case IColorInput<FixedFunctionSource> colorInput: {
            if (colorInput.Identifier.IsTexture(out var index)) {
              var texture = material.TextureSources[index];
              if (texture != null) {
                foundTextures.Add(texture);
              }
            }

            break;
          }
          case IColorExpression colorExpression: {
            colorQueue.Enqueue(
                colorExpression.Terms.Select(t => (t, inDenominator)));
            break;
          }
          case IColorTerm colorTerm: {
            colorQueue.Enqueue(
                colorTerm
                    .NumeratorFactors.Select(f => (f, inDenominator))
                    .ConcatIfNonnull(
                        colorTerm.DenominatorFactors
                                 ?.Select(f => (f, !inDenominator))));
            break;
          }
          case IColorValueTernaryOperator colorValueTernary: {
            colorQueue.Enqueue((colorValueTernary.TrueValue, inDenominator));
            break;
          }
        }
      }

      primaryTexture = foundTextures.GetPrimaryByPriority();
    }

    if (srcAlpha.IsZero()) {
      primaryColor.W = 0;
    } else if (srcAlpha.IsOne()) {
      primaryColor.W = 1;
    } else {
      // TODO: Surely there's a better way to do this...
      var alphaQueue
          = new FinTuple2Queue<IScalarValue, bool>((srcAlpha, false));
      while (alphaQueue.TryDequeue(out var alphaValue, out var inDenominator)) {
        switch (alphaValue) {
          case IScalarConstant scalarConstant: {
            var alpha = scalarConstant.Value;
            if (alpha.IsInRange(0, 1)) {
              if (!inDenominator) {
                primaryColor.W = scalarConstant.Value;
              } else {
                primaryColor.W = 1 / scalarConstant.Value;
              }
              goto DoneLookingForAlpha;
            }

            break;
          }
          case IScalarRegister scalarRegister: {
            var scalarConstant = scalarRegister.DefaultValue;
            var alpha = scalarConstant.Value;
            if (alpha.IsInRange(0, 1)) {
              if (!inDenominator) {
                primaryColor.W = scalarConstant.Value;
              } else {
                primaryColor.W = 1 / scalarConstant.Value;
              }
              goto DoneLookingForAlpha;
            }

            break;
          }
          case IScalarExpression scalarExpression: {
            alphaQueue.Enqueue(
                scalarExpression.Terms.Select(t => (t, inDenominator)));
            break;
          }
          case IScalarTerm scalarTerm: {
            alphaQueue.Enqueue(
                scalarTerm
                    .NumeratorFactors.Select(f => (f, inDenominator))
                    .ConcatIfNonnull(
                        scalarTerm.DenominatorFactors
                                  ?.Select(f => (f, !inDenominator))));
            break;
          }
        }
      }

      DoneLookingForAlpha: ;
    }

    return (primaryTexture, primaryColor);
  }
}
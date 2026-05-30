using System.Collections.Generic;
using System.Linq;

using fin.model;
using fin.util.enumerables;

namespace fin.language.equations.fixedFunction;

public static partial class FixedFunctionsEquationsUtil {
  public static IEnumerable<TValue> OrderFactorsWithChannelsFirst<TValue>(
      IDictionary<IValue, bool> dependsOnLightingChannel,
      IEnumerable<TValue> factors)
      where TValue : IValue
    => factors
        .OrderByDescending(f => DependsOnLightingChannel_(
                               dependsOnLightingChannel,
                               f));

  private static bool DependsOnLightingChannel_(
      IDictionary<IValue, bool> dependsOnLightingChannel,
      IValue value) {
    if (dependsOnLightingChannel.TryGetValue(value, out var doesDepend)) {
      return doesDepend;
    }

    return dependsOnLightingChannel[value] = value switch {
        IColorExpression colorExpression
            => colorExpression.Terms.Any(t => DependsOnLightingChannel_(
                                             dependsOnLightingChannel,
                                             t)),
        IScalarExpression scalarExpression
            => scalarExpression.Terms.Any(t => DependsOnLightingChannel_(
                                              dependsOnLightingChannel,
                                              t)),
        IColorTerm colorTerm
            => colorTerm.NumeratorFactors
                        .ConcatIfNonnull(colorTerm.DenominatorFactors)
                        .Any(f => DependsOnLightingChannel_(
                                 dependsOnLightingChannel,
                                 f)),
        IScalarTerm scalarTerm
            => scalarTerm.NumeratorFactors
                         .ConcatIfNonnull(scalarTerm.DenominatorFactors)
                         .Any(f => DependsOnLightingChannel_(
                                  dependsOnLightingChannel,
                                  f)),
        IColorValueSwizzle colorValueSwizzle
            => DependsOnLightingChannel_(dependsOnLightingChannel,
                                         colorValueSwizzle.Source),
        IColorValueTernaryOperator colorValueTernaryOperator
            => DependsOnLightingChannel_(
                   dependsOnLightingChannel,
                   colorValueTernaryOperator.TrueValue) ||
               DependsOnLightingChannel_(
                   dependsOnLightingChannel,
                   colorValueTernaryOperator.FalseValue),
        IColorInput<FixedFunctionSource> colorInput
            => colorInput.Identifier.IsLighting(),
        IScalarInput<FixedFunctionSource> scalarInput
            => scalarInput.Identifier.IsLighting(),
        _ => false,
    };
  }
}
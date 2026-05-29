using fin.model;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.language.equations.fixedFunction;

public sealed partial class FixedFunctionEquationsExtractorTests {
  [Test]
  public void TestZero() {
    var equations = new FixedFunctionEquations<FixedFunctionSource>();
    equations.SetOutputColorAlpha((ColorConstant.ZERO, ScalarConstant.ZERO));

    FixedFunctionsEquationsExtractor.ExtractLightingChannels(
        equations,
        out var diffuseColorAlpha,
        out var specularColorAlpha,
        out var ambientColorAlpha,
        out var otherColorAlpha);

    Assert.AreEqual((ColorConstant.ZERO, ScalarConstant.ZERO),
                    diffuseColorAlpha);
    Assert.AreEqual((ColorConstant.ZERO, ScalarConstant.ZERO),
                    specularColorAlpha);
    Assert.AreEqual((ColorConstant.ZERO, ScalarConstant.ZERO),
                    ambientColorAlpha);
    Assert.AreEqual((ColorConstant.ZERO, ScalarConstant.ZERO),
                    otherColorAlpha);
  }

  [Test]
  public void TestOne() {
    var equations = new FixedFunctionEquations<FixedFunctionSource>();
    equations.SetOutputColorAlpha((ColorConstant.ONE, ScalarConstant.ONE));

    FixedFunctionsEquationsExtractor.ExtractLightingChannels(
        equations,
        out var diffuseColorAlpha,
        out var specularColorAlpha,
        out var ambientColorAlpha,
        out var otherColorAlpha);

    Assert.AreEqual((ColorConstant.ZERO, ScalarConstant.ZERO),
                    diffuseColorAlpha);
    Assert.AreEqual((ColorConstant.ZERO, ScalarConstant.ZERO),
                    specularColorAlpha);
    Assert.AreEqual((ColorConstant.ZERO, ScalarConstant.ZERO),
                    ambientColorAlpha);
    Assert.AreEqual((ColorConstant.ONE, ScalarConstant.ONE),
                    otherColorAlpha);
  }

  [Test]
  public void TestAllButOther() {
    var equations = new FixedFunctionEquations<FixedFunctionSource>();

    var diffuseColor
        = equations
          .CreateOrGetColorInput(FixedFunctionSource.TEXTURE_COLOR_0)
          .Multiply(equations.CreateOrGetColorInput(
                        FixedFunctionSource.VERTEX_COLOR_0));
    var diffuseAlpha
        = equations
          .CreateOrGetScalarInput(FixedFunctionSource.TEXTURE_ALPHA_0)
          .Multiply(equations.CreateOrGetScalarInput(
                        FixedFunctionSource.VERTEX_ALPHA_0));

    var specularColor
        = equations.CreateOrGetColorInput(FixedFunctionSource.TEXTURE_COLOR_1);
    var specularAlpha
        = equations.CreateOrGetScalarInput(FixedFunctionSource.TEXTURE_ALPHA_1);

    var ambientColor
        = equations.CreateOrGetColorInput(FixedFunctionSource.TEXTURE_COLOR_2);
    var ambientAlpha
        = equations.CreateOrGetScalarInput(FixedFunctionSource.TEXTURE_ALPHA_2);

    var outputColor
        = diffuseColor
          .Multiply(
              equations.CreateOrGetColorInput(
                  FixedFunctionSource.LIGHT_DIFFUSE_COLOR_MERGED))
          .Add(specularColor.Multiply(
                   equations.CreateOrGetColorInput(
                       FixedFunctionSource.LIGHT_SPECULAR_COLOR_MERGED)))
          .Add(ambientColor.Multiply(
                   equations.CreateOrGetColorInput(
                       FixedFunctionSource.LIGHT_AMBIENT_COLOR)));
    var outputAlpha
        = diffuseAlpha
          .Multiply(
              equations.CreateOrGetScalarInput(
                  FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_MERGED))
          .Add(specularAlpha.Multiply(
                   equations.CreateOrGetScalarInput(
                       FixedFunctionSource.LIGHT_SPECULAR_ALPHA_MERGED)))
          .Add(ambientAlpha.Multiply(
                   equations.CreateOrGetScalarInput(
                       FixedFunctionSource.LIGHT_AMBIENT_ALPHA)));

    equations.SetOutputColorAlpha((outputColor, outputAlpha));

    FixedFunctionsEquationsExtractor.ExtractLightingChannels(
        equations,
        out var diffuseColorAlpha,
        out var specularColorAlpha,
        out var ambientColorAlpha,
        out var otherColorAlpha);

    Assert.AreEqual((diffuseColor, diffuseAlpha),
                    diffuseColorAlpha);
    Assert.AreEqual((specularColor, specularAlpha),
                    specularColorAlpha);
    Assert.AreEqual((ambientColor, ambientAlpha),
                    ambientColorAlpha);
    Assert.AreEqual((ColorConstant.ZERO, ScalarConstant.ZERO),
                    otherColorAlpha);
  }

    [Test]
  public void TestAll() {
    var equations = new FixedFunctionEquations<FixedFunctionSource>();

    var diffuseColor
        = equations
          .CreateOrGetColorInput(FixedFunctionSource.TEXTURE_COLOR_0)
          .Multiply(equations.CreateOrGetColorInput(
                        FixedFunctionSource.VERTEX_COLOR_0));
    var diffuseAlpha
        = equations
          .CreateOrGetScalarInput(FixedFunctionSource.TEXTURE_ALPHA_0)
          .Multiply(equations.CreateOrGetScalarInput(
                        FixedFunctionSource.VERTEX_ALPHA_0));

    var specularColor
        = equations.CreateOrGetColorInput(FixedFunctionSource.TEXTURE_COLOR_1);
    var specularAlpha
        = equations.CreateOrGetScalarInput(FixedFunctionSource.TEXTURE_ALPHA_1);

    var ambientColor
        = equations.CreateOrGetColorInput(FixedFunctionSource.TEXTURE_COLOR_2);
    var ambientAlpha
        = equations.CreateOrGetScalarInput(FixedFunctionSource.TEXTURE_ALPHA_2);

    var otherColor
        = equations.CreateOrGetColorInput(FixedFunctionSource.TEXTURE_COLOR_3);
    var otherAlpha
        = equations.CreateOrGetScalarInput(FixedFunctionSource.TEXTURE_ALPHA_3);

    var outputColor
        = diffuseColor
          .Multiply(
              equations.CreateOrGetColorInput(
                  FixedFunctionSource.LIGHT_DIFFUSE_COLOR_MERGED))
          .Add(specularColor.Multiply(
                   equations.CreateOrGetColorInput(
                       FixedFunctionSource.LIGHT_SPECULAR_COLOR_MERGED)))
          .Add(ambientColor.Multiply(
                   equations.CreateOrGetColorInput(
                       FixedFunctionSource.LIGHT_AMBIENT_COLOR)))
          .Add(otherColor);
    var outputAlpha
        = diffuseAlpha
          .Multiply(
              equations.CreateOrGetScalarInput(
                  FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_MERGED))
          .Add(specularAlpha.Multiply(
                   equations.CreateOrGetScalarInput(
                       FixedFunctionSource.LIGHT_SPECULAR_ALPHA_MERGED)))
          .Add(ambientAlpha.Multiply(
                   equations.CreateOrGetScalarInput(
                       FixedFunctionSource.LIGHT_AMBIENT_ALPHA)))
          .Add(otherAlpha);

    equations.SetOutputColorAlpha((outputColor, outputAlpha));

    FixedFunctionsEquationsExtractor.ExtractLightingChannels(
        equations,
        out var diffuseColorAlpha,
        out var specularColorAlpha,
        out var ambientColorAlpha,
        out var otherColorAlpha);

    Assert.AreEqual((diffuseColor, diffuseAlpha),
                    diffuseColorAlpha);
    Assert.AreEqual((specularColor, specularAlpha),
                    specularColorAlpha);
    Assert.AreEqual((ambientColor, ambientAlpha),
                    ambientColorAlpha);
    Assert.AreEqual((otherColor, otherAlpha),
                    otherColorAlpha);
  }
}
using fin.model;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.language.equations.fixedFunction;

public sealed class FixedFunctionEquationsExtractorTests {
  [Test]
  public void TestZero() {
    var equations = new FixedFunctionEquations<FixedFunctionSource>();
    equations.SetOutputColorAlpha((ColorConstant.ZERO, ScalarConstant.ZERO));

    new FixedFunctionsEquationsLightingExtractor().ExtractLightingChannels(
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

    new FixedFunctionsEquationsLightingExtractor().ExtractLightingChannels(
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

    new FixedFunctionsEquationsLightingExtractor().ExtractLightingChannels(
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

    new FixedFunctionsEquationsLightingExtractor().ExtractLightingChannels(
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

  [Test]
  public void TestDistributed() {
    var equations = new FixedFunctionEquations<FixedFunctionSource>();

    var texColor0
        = equations.CreateOrGetColorInput(FixedFunctionSource.TEXTURE_COLOR_0);
    var texAlpha0
        = equations.CreateOrGetScalarInput(FixedFunctionSource.TEXTURE_ALPHA_0);

    var outputColor = texColor0
        .Multiply(
            equations
                .CreateOrGetColorInput(
                    FixedFunctionSource.LIGHT_DIFFUSE_COLOR_MERGED)
                .Add(equations.CreateOrGetColorInput(
                         FixedFunctionSource.LIGHT_SPECULAR_COLOR_MERGED))
                .Add(equations.CreateOrGetColorInput(
                         FixedFunctionSource.LIGHT_AMBIENT_COLOR))
                .Add(ScalarConstant.ONE));
    var outputAlpha = texAlpha0
        .Multiply(
            equations
                .CreateOrGetScalarInput(
                    FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_MERGED)
                .Add(equations.CreateOrGetScalarInput(
                         FixedFunctionSource.LIGHT_SPECULAR_ALPHA_MERGED))
                .Add(equations.CreateOrGetScalarInput(
                         FixedFunctionSource.LIGHT_AMBIENT_ALPHA))
                .Add(ScalarConstant.ONE));


    equations.SetOutputColorAlpha((outputColor, outputAlpha));

    new FixedFunctionsEquationsLightingExtractor().ExtractLightingChannels(
        equations,
        out var diffuseColorAlpha,
        out var specularColorAlpha,
        out var ambientColorAlpha,
        out var otherColorAlpha);

    Assert.AreEqual((texColor0, texAlpha0), diffuseColorAlpha);
    Assert.AreEqual((texColor0, texAlpha0), specularColorAlpha);
    Assert.AreEqual((texColor0, texAlpha0), ambientColorAlpha);
    Assert.AreEqual((texColor0, texAlpha0), otherColorAlpha);
  }

  [Test]
  public void TestDistributedExpressions() {
    var equations = new FixedFunctionEquations<FixedFunctionSource>();

    var texColor0And1
        = equations.CreateOrGetColorInput(FixedFunctionSource.TEXTURE_COLOR_0)
                   .Add(equations.CreateOrGetColorInput(
                            FixedFunctionSource.TEXTURE_COLOR_1));
    var texAlpha0And1
        = equations.CreateOrGetScalarInput(FixedFunctionSource.TEXTURE_ALPHA_0)
                   .Add(equations.CreateOrGetScalarInput(
                            FixedFunctionSource.TEXTURE_ALPHA_1));

    var outputColor = texColor0And1
        .Multiply(
            equations
                .CreateOrGetColorInput(
                    FixedFunctionSource.LIGHT_DIFFUSE_COLOR_MERGED)
                .Add(equations.CreateOrGetColorInput(
                         FixedFunctionSource.LIGHT_SPECULAR_COLOR_MERGED))
                .Add(equations.CreateOrGetColorInput(
                         FixedFunctionSource.LIGHT_AMBIENT_COLOR))
                .Add(ScalarConstant.ONE));
    var outputAlpha = texAlpha0And1
        .Multiply(
            equations
                .CreateOrGetScalarInput(
                    FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_MERGED)
                .Add(equations.CreateOrGetScalarInput(
                         FixedFunctionSource.LIGHT_SPECULAR_ALPHA_MERGED))
                .Add(equations.CreateOrGetScalarInput(
                         FixedFunctionSource.LIGHT_AMBIENT_ALPHA))
                .Add(ScalarConstant.ONE));

    equations.SetOutputColorAlpha((outputColor, outputAlpha));

    new FixedFunctionsEquationsLightingExtractor().ExtractLightingChannels(
        equations,
        out var diffuseColorAlpha,
        out var specularColorAlpha,
        out var ambientColorAlpha,
        out var otherColorAlpha);

    Assert.AreEqual((texColor0And1, texAlpha0And1), diffuseColorAlpha);
    Assert.AreEqual((texColor0And1, texAlpha0And1), specularColorAlpha);
    Assert.AreEqual((texColor0And1, texAlpha0And1), ambientColorAlpha);
    Assert.AreEqual((texColor0And1, texAlpha0And1), otherColorAlpha);
  }
}
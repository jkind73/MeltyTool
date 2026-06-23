using System.Drawing;

using fin.image;
using fin.model.impl;
using fin.util.asserts;

using NUnit.Framework;

using Assert = NUnit.Framework.Assert;

namespace fin.model.util;

public sealed class PrimaryTextureFinderTests {
  [Test]
  public void TestOrdersAsExpected() {
    var model = ModelImpl.CreateForViewer();
    var mm = model.MaterialManager;

    var redImage = FinImage.Create1x1FromColor(Color.Red);
    var transparentRedImage
        = FinImage.Create1x1FromColor(Color.FromArgb(128, 255, 0, 0));
    var whiteImage = FinImage.Create1x1FromColor(Color.White);
    var transparentWhiteImage
        = FinImage.Create1x1FromColor(Color.FromArgb(128, 255, 255, 255));

    var redTexture = mm.CreateTexture(redImage);
    redTexture.Name = "red";
    var transparentRedTexture = mm.CreateTexture(transparentRedImage);
    transparentRedTexture.Name = "transparentRed";
    var whiteTexture = mm.CreateTexture(whiteImage);
    whiteTexture.Name = "white";
    var transparentWhiteTexture = mm.CreateTexture(transparentWhiteImage);
    transparentWhiteTexture.Name = "transparentWhite";

    var sphericalRedTexture = mm.CreateTexture(redImage);
    sphericalRedTexture.Name = "sphericalRed";
    sphericalRedTexture.UvType = UvType.SPHERICAL;
    var sphericalTransparentRedTexture = mm.CreateTexture(transparentRedImage);
    sphericalTransparentRedTexture.Name = "sphericalTransparentRed";
    sphericalTransparentRedTexture.UvType = UvType.SPHERICAL;
    var sphericalWhiteTexture = mm.CreateTexture(whiteImage);
    sphericalWhiteTexture.Name = "sphericalWhite";
    sphericalWhiteTexture.UvType = UvType.SPHERICAL;
    var sphericalTransparentWhiteTexture
        = mm.CreateTexture(transparentWhiteImage);
    sphericalTransparentWhiteTexture.Name = "sphericalTransparentWhite";
    sphericalTransparentWhiteTexture.UvType = UvType.SPHERICAL;

    var textures = new IReadOnlyTexture[] {
        sphericalRedTexture,
        redTexture,
        sphericalTransparentRedTexture, 
        transparentRedTexture, 
        whiteTexture,
        sphericalWhiteTexture,
        transparentWhiteTexture, 
        sphericalTransparentWhiteTexture
    };

    Asserts.SequenceEqual(
        [
            redTexture,
            whiteTexture,
            transparentRedTexture,
            transparentWhiteTexture,
            sphericalRedTexture,
            sphericalWhiteTexture,
            sphericalTransparentRedTexture,
            sphericalTransparentWhiteTexture
        ],
        textures.Prioritize());

    Assert.AreEqual(redTexture, textures.GetPrimaryByPriority());
  }
}
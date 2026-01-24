using System;
using System.Drawing;
using System.Numerics;

using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.ui.rendering;
using fin.ui.rendering.gl.model;

namespace marioartisttool.view.games.ball;

public sealed class BallRenderer(float radius) : IRenderable {
  private ModelRenderer? impl_;

  ~BallRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_?.Dispose();

  public void Render() {
    this.impl_ ??= this.GenerateModelIfNull_();
    this.impl_.Render();
  }

  private ModelRenderer GenerateModelIfNull_() {
    var model = ModelImpl.CreateForViewer();

    var whiteMaterial = model.MaterialManager.AddColorMaterial(Color.White);
    whiteMaterial.CullingMode = CullingMode.SHOW_BACK_ONLY;

    var blackMaterial = model.MaterialManager.AddColorMaterial(Color.Black);

    whiteMaterial.DepthCompareType
        = blackMaterial.DepthCompareType = DepthCompareType.ALWAYS;

    var mesh = model.Skin.AddMesh();
    mesh.AddSimpleSphere(model.Skin, Vector3.Zero, radius, 30, whiteMaterial);
    mesh.AddSimpleSphere(model.Skin,
                         Vector3.Zero,
                         radius * .9f,
                         30,
                         blackMaterial);

    var modelRenderer = new ModelRenderer(model);
    modelRenderer.GenerateModelIfNull();

    return modelRenderer;
  }
}
using System;
using System.Drawing;
using System.Numerics;

using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.ui.rendering;
using fin.ui.rendering.gl.model;

namespace marioartisttool.view.games.ball;

public sealed class TransparentBallRenderer(float radius) : IRenderable {
  private ModelRenderer? impl_;

  ~TransparentBallRenderer() => this.ReleaseUnmanagedResources_();

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

    var blackMaterial = model.MaterialManager.AddColorMaterial(Color.FromArgb(64, 0, 0, 0));
    blackMaterial.DepthCompareType = DepthCompareType.Always;

    var mesh = model.Skin.AddMesh();
    mesh.AddSimpleSphere(model.Skin, Vector3.Zero, radius, 30, blackMaterial);
    model.RemoveAllNormals();

    var modelRenderer = new ModelRenderer(model);
    modelRenderer.GenerateModelIfNull();

    return modelRenderer;
  }
}
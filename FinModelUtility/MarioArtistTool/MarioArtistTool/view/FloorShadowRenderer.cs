using System;
using System.Drawing;
using System.Numerics;

using fin.image.util;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.model;
using fin.ui.rendering.viewer;

namespace marioartisttool.view;

public sealed class FloorShadowRenderer : IOrthoRenderable {
  private IModelRenderer? impl_;

  public float ViewportWidth { get; set; }
  public float ViewportHeight { get; set; }

  public float NearPlane { get; set; }
  public float FarPlane { get; set; }

  ~FloorShadowRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_?.Dispose();

  public void Render() {
    this.impl_ ??= this.GenerateModelIfNull_();

    GlTransform.MatrixMode(TransformMatrixMode.MODEL);
    GlTransform.LoadIdentity();

    var viewportWidth = this.ViewportWidth;
    var viewportHeight = this.ViewportHeight;

    var shadowWidth = viewportWidth;
    var shadowHeight = viewportHeight * .125f;

    var shadowY = viewportHeight - shadowHeight;

    GlTransform.Ortho2d(0, (int) viewportWidth, (int) viewportHeight, 0);
    GlTransform.Translate(0, shadowY, 0);
    GlTransform.Scale(shadowWidth, shadowHeight, 1);

    this.impl_.Render();
  }

  private IModelRenderer GenerateModelIfNull_() {
    var model = ModelImpl.CreateForViewer();

    var mesh = model.Skin.AddMesh();

    var material = model.MaterialManager.AddColorMaterial(Color.White);
    material.DepthMode = DepthMode.READ_ONLY;

    var transparent = new Vector4(0, 0, 0, 0);
    var shadow = new Vector4(0, 0, 0, .75f);

    var v0 = model.Skin.AddVertex(-1, -1, 0);
    v0.SetColor(transparent);

    var v1 = model.Skin.AddVertex(1, -1, 0);
    v1.SetColor(transparent);

    var v2 = model.Skin.AddVertex(1, 1, 0);
    v2.SetColor(shadow);

    var v3 = model.Skin.AddVertex(-1, 1, 0);
    v3.SetColor(shadow);

    mesh.AddQuads(v0, v1, v2, v3).SetMaterial(material);

    var modelRenderer = new ModelRenderer(model);
    modelRenderer.GenerateModelIfNull();

    return modelRenderer;
  }
}
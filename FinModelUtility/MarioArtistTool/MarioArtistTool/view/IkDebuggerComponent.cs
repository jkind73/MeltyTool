using System;
using System.Drawing;
using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.scene;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.model;
using fin.util.time;

namespace marioartisttool.view;

public class IkDebuggerComponent(Vector3? localTarget, Color color) : ISceneNodeRenderComponent {
  private IModelRenderer ballRenderer_;
  private IModelRenderer boneRenderer_;

  ~IkDebuggerComponent() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.ballRenderer_.Dispose();
    this.boneRenderer_.Dispose();
  }

  private void GenerateModelIfNull_() {
    if (this.ballRenderer_ != null) {
      return;
    }

    var model = ModelImpl.CreateForViewer();

    var radius = 5;

    var whiteMaterial = model.MaterialManager.AddColorMaterial(Color.Black);
    whiteMaterial.CullingMode = CullingMode.SHOW_BACK_ONLY;

    var blackMaterial = model.MaterialManager.AddColorMaterial(color);

    whiteMaterial.DepthCompareType
        = blackMaterial.DepthCompareType = DepthCompareType.Always;

    var mesh = model.Skin.AddMesh();
    mesh.AddSimpleSphere(model.Skin, Vector3.Zero, radius, 30, whiteMaterial);
    mesh.AddSimpleSphere(model.Skin,
                         Vector3.Zero,
                         radius * .9f,
                         30,
                         blackMaterial);
    model.RemoveAllNormals();

    var modelRenderer = new ModelRenderer(model);
    modelRenderer.GenerateModelIfNull();

    this.ballRenderer_ = modelRenderer;
    this.boneRenderer_ = SkeletonRenderer.GenerateBoneRenderer();
  }

  public void Render(ISceneNodeInstance self) {
    this.GenerateModelIfNull_();

    var frameTime
        = (float) FrameTime.ElapsedTimeSinceApplicationOpened.TotalSeconds;

    Vector3 offset;
    Vector3 target;
    float rollRadians0 = 0;
    float rollRadians1 = 0;
    float length0;
    float length1;

    if (localTarget != null) {
      offset = self.Transform.WorldMatrix.Translation;
      target = offset + localTarget.Value;
      length0 = 100;
      length1 = 100;
    } else {
      var pitchDegreesTowardsOffset
          = 0; //MathF.Sin(frameTime * 22 * FinTrig.DEG_2_RAD) * 22;
      var pitchDegreesTowardsTarget
          = 0; //MathF.Sin(frameTime * 34 * FinTrig.DEG_2_RAD) * 34;

      rollRadians0
          = MathF.Sin(frameTime * 43 * FinTrig.DEG_2_RAD) * 43 * FinTrig.DEG_2_RAD;
      rollRadians1
          = 0; //MathF.Sin(frameTime * 37 * FinTrig.DEG_2_RAD) * 37 * FinTrig.DEG_2_RAD;

      var yawDegreesTowardsOffset = frameTime * 30;
      var yawDegreesTowardsTarget = frameTime * 25;

      var offsetDistance = 20;
      var targetDistance = 50;

      length0 = 50;
      length1 = 40;

      FinTrig.FromPitchYawDegrees(
          pitchDegreesTowardsOffset,
          yawDegreesTowardsOffset,
          out var offsetXNormal,
          out var offsetYNormal,
          out var offsetZNormal);
      FinTrig.FromPitchYawDegrees(
          pitchDegreesTowardsTarget,
          yawDegreesTowardsTarget,
          out var targetXNormal,
          out var targetYNormal,
          out var targetZNormal);

      offset = new Vector3(offsetXNormal, offsetYNormal, offsetZNormal) *
                   offsetDistance;
      target = new Vector3(targetXNormal, targetYNormal, targetZNormal) *
                   targetDistance;
    }

    GlTransform.PushMatrix();
    {
      if (false) {
        var quaternion = FinTrig.GetQuaternionTowards(target - offset, rollRadians0);

        GlTransform.MultMatrix(
            SystemMatrix4x4Util.FromTrs(
                FinTrig.ConvertFromZUpToYUp(offset),
                FinTrig.ConvertFromZUpToYUp(quaternion),
                new Vector3((target - offset).Length())));
        this.boneRenderer_.Render();
      } else {
        var (quaternion0, quaternion1) = FinTrig.GetQuaternionsTowards(
                (length0, rollRadians0),
                (length1, rollRadians1),
                target - offset,
                out _,
                out var middlePoint);

        GlTransform.PushMatrix();
        {
          GlTransform.Translate(FinTrig.ConvertFromZUpToYUp(offset + middlePoint));
          GlTransform.Scale(.5f);
          this.ballRenderer_.Render();
        }
        GlTransform.PopMatrix();

        GlTransform.MultMatrix(
            SystemMatrix4x4Util.FromTrs(
                FinTrig.ConvertFromZUpToYUp(offset),
                FinTrig.ConvertFromZUpToYUp(quaternion0),
                null));

        GlTransform.PushMatrix();
        {
          GlTransform.Scale(length0);
          this.boneRenderer_.Render();
        }
        GlTransform.PopMatrix();

        GlTransform.MultMatrix(
            SystemMatrix4x4Util.FromTrs(
                new Vector3(length0, 0, 0),
                quaternion1,
                new Vector3(length1)));
        this.boneRenderer_.Render();
      }
    }
    GlTransform.PopMatrix();

    GlTransform.PushMatrix();
    {
      GlTransform.Translate(FinTrig.ConvertFromZUpToYUp(offset));
      this.ballRenderer_.Render();
    }
    GlTransform.PopMatrix();

    GlTransform.PushMatrix();
    {
      GlTransform.Translate(FinTrig.ConvertFromZUpToYUp(target));
      this.ballRenderer_.Render();
    }
    GlTransform.PopMatrix();
  }
}
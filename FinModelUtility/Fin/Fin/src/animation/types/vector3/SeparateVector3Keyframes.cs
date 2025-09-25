using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.math.floats;

namespace fin.animation.types.vector3;

public class SeparateVector3Keyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, float> interpolator,
    IndividualInterpolationConfig<float>? individualConfigX,
    IndividualInterpolationConfig<float>? individualConfigY,
    IndividualInterpolationConfig<float>? individualConfigZ)
    : ISeparateVector3Keyframes<TKeyframe>
    where TKeyframe : IKeyframe<float> {
  public SeparateVector3Keyframes(
      ISharedInterpolationConfig sharedConfig,
      IKeyframeInterpolator<TKeyframe, float> interpolator,
      IndividualInterpolationConfig<float>? individualConfig = null)
      : this(sharedConfig,
             interpolator,
             individualConfig,
             individualConfig,
             individualConfig) { }

  public IReadOnlyList<IInterpolatableKeyframes<TKeyframe, float>> Axes { get; }
    = [
        new InterpolatedKeyframes<TKeyframe, float>(
            sharedConfig,
            interpolator,
            individualConfigX),
        new InterpolatedKeyframes<TKeyframe, float>(
            sharedConfig,
            interpolator,
            individualConfigY),
        new InterpolatedKeyframes<TKeyframe, float>(
            sharedConfig,
            interpolator,
            individualConfigZ),
    ];

  public bool TryGetAtFrame(float frame, out Vector3 value) {
    value = default;

    if (!this.Axes[0]
             .TryGetAtFrameOrDefault(frame, individualConfigX, out var x)) {
      return false;
    }

    if (!this.Axes[1]
             .TryGetAtFrameOrDefault(frame, individualConfigY, out var y)) {
      return false;
    }

    if (!this.Axes[2]
             .TryGetAtFrameOrDefault(frame, individualConfigZ, out var z)) {
      return false;
    }

    value = new Vector3(x, y, z);
    return true;
  }

  public void GetAllFrames(Span<Vector3> dst) {
    Span<float> x = stackalloc float[dst.Length];
    Span<float> y = stackalloc float[dst.Length];
    Span<float> z = stackalloc float[dst.Length];

    this.Axes[0].GetAllFrames(x);
    this.Axes[1].GetAllFrames(y);
    this.Axes[2].GetAllFrames(z);

    for (var i = 0; i < dst.Length; ++i) {
      dst[i] = new Vector3(x[i], y[i], z[i]);
    }
  }

  // TODO: Implement this
  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, Vector3 value)> keyframes,
      out IReadOnlyList<(Vector3 tangentIn, Vector3 tangentOut)>?
          tangentKeyframes) {
    var xAxis = this.Axes[0];
    var yAxis = this.Axes[1];
    var zAxis = this.Axes[2];

    if (!xAxis.HasAnyData || !yAxis.HasAnyData || !zAxis.HasAnyData) {
      keyframes = null!;
      tangentKeyframes = null;
      return false;
    }

    var xDefinitions = xAxis.Definitions;
    var yDefinitions = yAxis.Definitions;
    var zDefinitions = zAxis.Definitions;
    if (xDefinitions.Count != yDefinitions.Count ||
        xDefinitions.Count != zDefinitions.Count) {
      keyframes = null!;
      tangentKeyframes = null;
      return false;
    }

    var length = xDefinitions.Count;
    for (var i = 0; i < length; ++i) {
      if (!xDefinitions[i].Frame.IsRoughly(yDefinitions[i].Frame) ||
          !xDefinitions[i].Frame.IsRoughly(zDefinitions[i].Frame)) {
        keyframes = null!;
        tangentKeyframes = null;
        return false;
      }
    }

    if (xDefinitions
            is IReadOnlyList<KeyframeWithTangents<float>> xTangentDefinitions &&
        yDefinitions
            is IReadOnlyList<KeyframeWithTangents<float>> yTangentDefinitions &&
        zDefinitions
            is IReadOnlyList<KeyframeWithTangents<float>> zTangentDefinitions) {
      var mutableKeyframes = new (float, Vector3)[length];
      var mutableTangentKeyframes = new (Vector3, Vector3)[length];
      for (var i = 0; i < length; ++i) {
        var xKeyframe = xTangentDefinitions[i];
        var yKeyframe = yTangentDefinitions[i];
        var zKeyframe = zTangentDefinitions[i];

        mutableKeyframes[i] = (xKeyframe.Frame,
                               new Vector3(xKeyframe.ValueOut,
                                           yKeyframe.ValueOut,
                                           zKeyframe.ValueOut));
        mutableTangentKeyframes[i]
            = (new Vector3(xKeyframe.TangentIn ?? 0,
                           yKeyframe.TangentIn ?? 0,
                           zKeyframe.TangentIn ?? 0),
               new Vector3(xKeyframe.TangentOut ?? 0,
                           yKeyframe.TangentOut ?? 0,
                           zKeyframe.TangentOut ?? 0));
      }

      keyframes = mutableKeyframes;
      tangentKeyframes = mutableTangentKeyframes;
      return true;
    } else {
      var mutableKeyframes = new (float, Vector3)[length];
      for (var i = 0; i < length; ++i) {
        var xKeyframe = xDefinitions[i];
        var yKeyframe = yDefinitions[i];
        var zKeyframe = zDefinitions[i];

        mutableKeyframes[i] = (xKeyframe.Frame,
                               new Vector3(xKeyframe.ValueOut,
                                           yKeyframe.ValueOut,
                                           zKeyframe.ValueOut));
      }

      keyframes = mutableKeyframes;
      tangentKeyframes = null;
      return true;
    }
  }
}
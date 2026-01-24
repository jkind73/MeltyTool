namespace level5;

public enum AnimNodeHashType {
  NAME = 0,
  CRC32_C = 1,
}

/// <summary>
/// Stores animation data for transformations
/// Information is stored in tracks <see cref="GenericTransformTrack"/>
/// </summary>
public sealed class GenericAnimationTransform {
  public string Name { get; set; }

  public uint Hash { get; set; }

  public AnimNodeHashType HashType { get; set; } = AnimNodeHashType.NAME;

  public Dictionary<AnimationTrackFormat, GenericTransformTrack> Tracks {
    get;
  } = new();

  /// <summary>
  /// adds a new key frame to the animation
  /// </summary>
  /// <param name="frame"></param>
  /// <param name="value"></param>
  /// <param name="type"></param>
  /// <param name="interpolationType"></param>
  public void AddKey(float frame,
                     float value,
                     AnimationTrackFormat type,
                     InterpolationType interpolationType =
                         InterpolationType.LINEAR) {
      if (!this.Tracks.TryGetValue(type, out var track)) {
        track = new GenericTransformTrack(type);
        this.Tracks[type] = track;
      }

      track.AddKey(frame, value, interpolationType);
    }
}


public enum AnimationTrackFormat {
  TRANSLATE_X,
  TRANSLATE_Y,
  TRANSLATE_Z,
  ROTATE_X,
  ROTATE_Y,
  ROTATE_Z,
  SCALE_X,
  SCALE_Y,
  SCALE_Z,
  COMPENSATE_SCALE
}

public static class AnimationTrackFormatExtensions {
  public static bool IsTranslation(this AnimationTrackFormat value,
                                   out int axis)
    => value.IsInRange(AnimationTrackFormat.TRANSLATE_X, 3, out axis);

  public static bool IsRotation(this AnimationTrackFormat value,
                                out int axis)
    => value.IsInRange(AnimationTrackFormat.ROTATE_X, 3, out axis);

  public static bool IsScale(this AnimationTrackFormat value,
                             out int axis)
    => value.IsInRange(AnimationTrackFormat.SCALE_X, 3, out axis);

  public static bool IsInRange(this AnimationTrackFormat value,
                               AnimationTrackFormat min,
                               int max,
                               out int id) {
      id = value - min;
      return id >= 0 && id < max;
    }
}

/// <summary>
/// A track for <see cref="SBTransformAnimation"/>
/// See <see cref="AnimationTrackFormat"/> for supported types
/// </summary>
public sealed class GenericTransformTrack(AnimationTrackFormat type) {
  public AnimationTrackFormat Type { get; internal set; } = type;

  public GenericKeyGroup<float> Keys { get; } = new GenericKeyGroup<float>();

  public void AddKey(float frame,
                     float value,
                     InterpolationType interpolationType =
                         InterpolationType.LINEAR,
                     float inTan = 0,
                     float outTan = float.MaxValue) {
    this.Keys.AddKey(frame, value, interpolationType, inTan, outTan);
    }
}
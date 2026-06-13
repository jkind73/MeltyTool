using System;
using System.Collections.Generic;

namespace f3dzex2.image;

// TODO: Still a hack, need to properly emulate loading bytes into TMEM
public sealed class TmemAddressDictionary {
  private readonly SortedSet<Segment> impl_ = new();

  public uint this[uint tmemOffset] {
    get {
      foreach (var segment in this.impl_.Reverse()) {
        if (segment.TmemOffset <= tmemOffset) {
          return segment.SegmentedAddress + tmemOffset - segment.TmemOffset;
        }
      }

      throw new ArgumentException();
    }
    set {
      var newSegment = new Segment {
          TmemOffset = tmemOffset,
          SegmentedAddress = value,
      };
      this.impl_.Remove(newSegment);
      this.impl_.Add(newSegment);
    }
  }

  private struct Segment : IComparable<Segment>, IEquatable<Segment> {
    public uint TmemOffset { get; set; }
    public uint SegmentedAddress { get; set; }

    public override int GetHashCode() => (int) this.TmemOffset;

    public int CompareTo(Segment other)
      => this.TmemOffset.CompareTo(other.TmemOffset);

    public override bool Equals(object? obj)
      => obj is Segment other && this.Equals(other);

    public bool Equals(Segment other) => this.TmemOffset == other.TmemOffset;
  }
}
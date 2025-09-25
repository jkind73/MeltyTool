namespace modl.level;

public sealed class ImpactBase : BLevelObject {
  public SoundBase SoundBase { get; set; }
  public ExplodeBase? ExplosionType { get; set; }
  public TequilaEffectResource? TequilaEffect { get; set; }
}
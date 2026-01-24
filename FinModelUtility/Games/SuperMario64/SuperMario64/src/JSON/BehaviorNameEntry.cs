namespace sm64.JSON {
  public sealed class BehaviorNameEntry {
    string name_ = "";
    uint behavior_ = 0;

    public string Name {
      get { return this.name_; }
      set { this.name_ = value; }
    }

    public uint Behavior {
      get { return this.behavior_; }
    }

    public BehaviorNameEntry(string name, uint behavior) {
      this.name_ = name;
      this.behavior_ = behavior;
    }

    public override string ToString() {
      return this.name_ + " = [" + "0x" + this.behavior_.ToString("X8") + "]";
    }
  }
}
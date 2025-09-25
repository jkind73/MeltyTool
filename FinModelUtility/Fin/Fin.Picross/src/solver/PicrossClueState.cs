using readOnly;

namespace fin.picross.solver;

[GenerateReadOnly]
public partial interface IPicrossClueState {
  new IPicrossClue Clue { get; }
  new byte Length { get; }
  new bool Solved => this.StartIndex != null;
  new int? StartIndex { get; set; }
}

public record PicrossClueState(IPicrossClue Clue) : IPicrossClueState {
  public byte Length => this.Clue.Length;
  public int? StartIndex { get; set; }
}
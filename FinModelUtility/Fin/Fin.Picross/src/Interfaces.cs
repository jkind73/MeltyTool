using fin.data;

using readOnly;

namespace fin.picross;

[GenerateReadOnly]
public partial interface IPicrossDefinition : IGrid<bool> {
  string Name { get; set; }
}
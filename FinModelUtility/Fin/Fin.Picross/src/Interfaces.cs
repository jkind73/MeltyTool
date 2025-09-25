using fin.data;

using readOnly;

namespace fin.picross;

[GenerateReadOnly]
public partial interface IPicrossDefinition : IGrid<bool> {
  new string Name { get; set; }
}
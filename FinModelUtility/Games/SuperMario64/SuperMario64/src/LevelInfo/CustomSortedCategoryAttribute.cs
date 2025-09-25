using System.ComponentModel;

namespace sm64.LevelInfo {
  public sealed class CustomSortedCategoryAttribute(
      string category,
      ushort categoryPos,
      ushort totalCategories)
      : CategoryAttribute(category.PadLeft(
                              category.Length + (totalCategories - categoryPos),
                              NonPrintableChar)) {
    private const char NonPrintableChar = '\t';
  }
}
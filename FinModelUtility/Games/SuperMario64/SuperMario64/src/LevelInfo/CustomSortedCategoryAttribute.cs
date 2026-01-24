using System.ComponentModel;

namespace sm64.LevelInfo {
  public sealed class CustomSortedCategoryAttribute(
      string category,
      ushort categoryPos,
      ushort totalCategories)
      : CategoryAttribute(category.PadLeft(
                              category.Length + (totalCategories - categoryPos),
                              NON_PRINTABLE_CHAR_)) {
    private const char NON_PRINTABLE_CHAR_ = '\t';
  }
}
using schema.binary;

namespace tlpe.scb;

public enum SectionType : uint {
  SECTION_1 = 1,
  SECTION_2 = 2,
  MESH = 3,
  SECTION_6 = 6,
}

public interface ISection : IBinaryConvertible;

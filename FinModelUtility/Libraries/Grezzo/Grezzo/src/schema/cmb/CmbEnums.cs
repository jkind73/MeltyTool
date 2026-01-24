namespace grezzo.schema.cmb;

public enum DataType : uint {
  BYTE = 0x1400,
  U_BYTE = 0x1401,
  SHORT = 0x1402,
  U_SHORT = 0x1403,
  INT = 0x1404,
  U_INT = 0x1405,
  FLOAT = 0x1406
}

public enum TestFunc {
  INVALID = 0,
  NEVER = 512,
  LESS = 513,
  EQUAL = 514,
  LEQUAL = 515,
  GREATER = 516,
  NOTEQUAL = 517,
  GEQUAL = 518,
  ALWAYS = 519
}

public enum CullMode {
  NEVER = 0,
  BACK = 1,
  FRONT = 2,
  FRONT_AND_BACK = 3,
}

public enum BumpMode {
  NOT_USED = 25288,
  AS_BUMP = 25289,
  AS_TANGENT = 25290 // Doesn't exist in OoT3D
}

public enum BumpTexture {
  TEXTURE0 = 0x84C0,
  TEXTURE1 = 0x84C0,
  TEXTURE2 = 0x84C0
}

public enum BlendEquation {
  FUNC_ADD = 0x8006,
  FUNC_SUBTRACT = 0x800A,
  FUNC_REVERSE_SUBTRACT = 0x800B,
  MIN = 0x8007,
  MAX = 0x8008
}

public enum BlendMode {
  BLEND_NONE = 0,
  BLEND = 1,
  BLEND_SEPARATE = 2,
  LOGICAL_OP = 3
}

public enum BlendFactor {
  ZERO = 0,
  ONE = 1,
  SOURCE_COLOR = 768,
  ONE_MINUS_SOURCE_COLOR = 769,
  DESTINATION_COLOR = 774,
  ONE_MINUS_DESTINATION_COLOR = 775,
  SOURCE_ALPHA = 770,
  ONE_MINUS_SOURCE_ALPHA = 771,
  DESTINATION_ALPHA = 772,
  ONE_MINUS_DESTINATION_ALPHA = 773,
  CONSTANT_COLOR = 32769,
  ONE_MINUS_CONSTANT_COLOR = 32770,
  CONSTANT_ALPHA = 32771,
  ONE_MINUS_CONSTANT_ALPHA = 32772,
  SOURCE_ALPHA_SATURATE = 776
}

public enum TexCombineMode : ushort {
  REPLACE = 0x1E01,
  MODULATE = 0x2100,
  ADD = 0x0104,
  ADD_SIGNED = 0x8574,
  INTERPOLATE = 0x8575,
  SUBTRACT = 0x84E7,
  DOT_PRODUCT3_RGB = 0x86AE,
  DOT_PRODUCT3_RGBA = 0x86AF,
  MULT_ADD = 0x6401,
  ADD_MULT = 0x6402
}

public enum TexCombineScale : ushort {
  ONE = 1,
  TWO = 2,
  FOUR = 4
}

public enum TexCombinerSource : ushort {
  PRIMARY_COLOR = 0x8577,
  FRAGMENT_PRIMARY_COLOR = 0x6210,
  FRAGMENT_SECONDARY_COLOR = 0x6211,
  TEXTURE0 = 0x84C0,
  TEXTURE1 = 0x84C1,
  TEXTURE2 = 0x84C2,
  TEXTURE3 = 0x84C3,
  PREVIOUS_BUFFER = 0x8579,
  CONSTANT = 0x8576,
  PREVIOUS = 0x8578
}

public enum TexBufferSource : ushort {
  PREVIOUS_BUFFER = 0x8579,
  PREVIOUS = 0x8578
}

public enum TexCombinerColorOp : ushort {
  COLOR = 0x0300,
  ONE_MINUS_COLOR = 0x0301,
  ALPHA = 0x0302,
  ONE_MINUS_ALPHA = 0x0303,
  RED = 0x8580,
  ONE_MINUS_RED = 0x8583,
  GREEN = 0x8581,
  ONE_MINUS_GREEN = 0x8584,
  BLUE = 0x8582,
  ONE_MINUS_BLUE = 0x8585
}

public enum TexCombinerAlphaOp : ushort {
  ALPHA = 0x0302,
  ONE_MINUS_ALPHA = 0x0303,
  RED = 0x8580,
  ONE_MINUS_RED = 0x8583,
  GREEN = 0x8581,
  ONE_MINUS_GREEN = 0x8584,
  BLUE = 0x8582,
  ONE_MINUS_BLUE = 0x8585
}

public enum TextureMinFilter : ushort {
  NEAREST = 0x2600,
  LINEAR = 0x2601,
  NEAREST_MIPMAP_NEAREST = 0x2700,
  LINEAR_MIPMAP_NEAREST = 0x2701,
  NEAREST_MIPMAP_LINEAR = 0x2702,
  LINEAR_MIPMAP_LINEAR = 0x2703
}

public enum TextureMagFilter : ushort {
  NEAREST = 0x2600,
  LINEAR = 0x2601,
}

public enum TextureWrapMode : ushort {
  CLAMP_TO_BORDER = 0x2900,
  REPEAT = 0x2901,
  CLAMP_TO_EDGE = 0x812F,
  MIRROR = 0x8370
}

public enum TextureMappingType {
  EMPTY = 0,
  UV_COORDINATE_MAP = 1,
  CAMERA_CUBE_ENV_MAP = 2,
  CAMERA_SPHERE_ENV_MAP = 3,
  PROJECTION_MAP = 4
}

public enum TextureMatrixMode {
  DCC_MAYA = 0,
  DCC_SOFT_IMAGE = 1,
  DCC3DS_MAX = 2
}

public enum LutInput : ushort {
  COS_NORMAL_HALF = 25248,
  COS_VIEW_HALF = 25249,
  COS_NORMAL_VIEW = 25250,
  COS_LIGHT_NORMAL = 25251,
  COS_LIGHT_SPOT = 25252,
  COS_PHI = 25253
}

public enum LayerConfig {
  LAYER_CONFIG0 = 25264,
  LAYER_CONFIG1 = 25265,
  LAYER_CONFIG2 = 25266,
  LAYER_CONFIG3 = 25267,
  LAYER_CONFIG4 = 25268,
  LAYER_CONFIG5 = 25269,
  LAYER_CONFIG6 = 25270,
  LAYER_CONFIG7 = 25271
}

public enum FresnelConfig {
  NO = 25280,
  PRI = 25281,
  SEC = 25282,
  PRI_SEC = 25283
}

public enum StencilTestOp {
  KEEP = 7680,
  ZERO = 0,
  REPLACE = 7681,
  INCREMENT = 7682,
  DECREMENT = 7683,
  INVERT = 5386,
  INCREMENT_WRAP = 34055,
  DECREMENT_WRAP = 34055
}

public enum VertexAttributeMode {
  ARRAY = 0,
  CONSTANT = 1,
}

public enum RenderLayer {
  OPAQUE, TRANSLUCENT, SUBTRACTIVE, ADDITIVE
}

public enum SkinningMode : ushort {
  SINGLE = 0,
  RIGID = 1,
  SMOOTH = 2,
}

public enum PrimitiveMode : uint {
  TRIANGLES = 0,
  TRIANGLE_STRIP = 1,
  TRIANGLE_FAN = 2,
}
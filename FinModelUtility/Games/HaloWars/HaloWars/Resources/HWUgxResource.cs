#nullable enable


using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml;

using Dxt;

using fin.data.lazy;
using fin.io;
using fin.math.rotations;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.util.asserts;

using granny3d;

using KSoft.IO;
using KSoft.Phoenix.Xmb;
using KSoft.Shell;

using schema.binary;

namespace HaloWarsTools;

public sealed class HwUgxResource : HwBinaryResource {
  public ModelImpl Mesh { get; private set; }
  private bool FlipFaces { get; set; }

  public static HwUgxResource? FromFile(
      HwContext context,
      string filename,
      bool flipFaces) {
    // Set the extension based on the resource type if the filename doesn't have one
    if (string.IsNullOrEmpty(Path.GetExtension(filename)) &&
        TypeExtensions.TryGetValue(HwResourceType.UGX,
                                   out string defaultExtension)) {
      filename = Path.ChangeExtension(filename, defaultExtension);
    }

    var resource = (HwUgxResource) CreateResource(context, filename);
    resource.Mesh = new ModelImpl {
        // TODO: Fix this
        FileBundle = null,
        Files = new HashSet<IReadOnlyGenericFile>(),
    };
    resource.FlipFaces = flipFaces;
    resource?.Load(File.ReadAllBytes(resource.AbsolutePath));

    return resource;
  }

  protected override void Load(byte[] bytes) {
    base.Load(bytes);

    this.ImportMesh_(bytes, this.Mesh);
  }

  private IList<IMaterial> GetMaterials_(IMaterialManager materialManager,
                                        byte[] bytes) {
    var textureFiles = new List<IReadOnlySystemFile>();
    HwBinaryResourceChunk materialChunk =
        GetFirstChunkOfType(HwBinaryResourceChunkType.UGX_MATERIAL_CHUNK);

    var bdt = new BinaryDataTree();
    bdt.ValidateData = false;

    var chunkStream = new MemoryStream(
        bytes,
        (int) materialChunk.offset,
        (int) materialChunk.size,
        false);
    using (chunkStream) {
      var es = new EndianStream(chunkStream, EndianFormat.LITTLE,
                                permissions: FileAccess.Read);
      es.StreamMode = FileAccess.Read;
      bdt.Serialize(es);
    }

    var lazyTextureDictionary = new LazyCaseInvariantStringDictionary<ITexture>(name
        => this.LoadTexture_(materialManager, name));
    var materials = new List<IMaterial>();

    var xml = bdt.ToXmlDocument();
    foreach (XmlElement ugxMaterials in xml.ChildNodes) {
      foreach (XmlElement ugxMaterial in ugxMaterials) {
        var type = ugxMaterial.GetType();

        var finMaterial = materialManager.AddStandardMaterial();
        finMaterial.Name = $"material_{materials.Count}";

        // TODO: Some textures are still not processed here:
        // - "_op_" seems to be opacity?
        // - "_xf_" is what?

        var maps = ugxMaterial["Maps"];
        if (maps != null) {
          var diffuse = maps["diffuse"];
          var diffuseMap = diffuse?["Map"];
          if (diffuseMap != null) {
            var diffuseMapName = diffuseMap.GetAttribute("Name");
            finMaterial.DiffuseTexture =
                lazyTextureDictionary[diffuseMapName];
          }

          var normal = maps["normal"];
          var normalMap = normal?["Map"];
          if (normalMap != null) {
            var normalMapName = normalMap.GetAttribute("Name");
            finMaterial.NormalTexture = lazyTextureDictionary[normalMapName];
          }

          var emissive = maps["emissive"];
          var emissiveMap = emissive?["Map"];
          if (emissiveMap != null) {
            var emissiveMapName = emissiveMap.GetAttribute("Name");
            finMaterial.EmissiveTexture =
                lazyTextureDictionary[emissiveMapName];
          }

          var specular = maps["gloss"];
          var specularMap = specular?["Map"];
          if (specularMap != null) {
            var specularMapName = specularMap.GetAttribute("Name");
            finMaterial.SpecularTexture = lazyTextureDictionary[specularMapName];
          }
        }

        materials.Add(finMaterial);
      }
    }

    /*var skyTextures = new FinDirectory(
                          @"R:\Documents\CSharpWorkspace\Pikmin2Utility\cli\roms\halo_wars\art\environment\sky")
                      .GetExistingFiles()
                      .Where(f => f.Extension == ".ddx");*/
    return materials;
  }

  private ITexture LoadTexture_(IMaterialManager materialManager,
                               string name) {
    var localTexturePath = $"art{name}.ddx";
    var absoluteTexturePath =
        Path.Combine(this.Context.scratchDirectory, localTexturePath);

    var textureFile = new FinFile(absoluteTexturePath);
    var (textureType, dxt) = DxtDecoder.ReadDds(textureFile);

    var firstMipmap = dxt.First();
    var firstLevel = firstMipmap.First();
    var firstImage = firstLevel.Impl;

    var texture = materialManager.CreateTexture(firstImage);
    texture.Name = name;

    return texture;
  }

  private string GetStringAt_(byte[] bytes, int offset) {
    StringBuilder current = new StringBuilder();
    for (int i = offset; i < bytes.Length; i++) {
      char value = (char) bytes[i];
      if (value == 0) {
        break;
      } else {
        current.Append(value);
      }
    }

    return current.ToString();
  }

  private void ImportMesh_(byte[] bytes, ModelImpl finModel) {
    var finSkin = finModel.Skin;

    var finMaterials = this.GetMaterials_(finModel.MaterialManager, bytes);
    var nullMaterial = finModel.MaterialManager.AddStandardMaterial();

    int offset = 0;

    offset += 4; // 4 byte magic
    int tableOffset = BinaryUtils.ReadInt32BigEndian(bytes, offset);
    offset += 4;
    offset += 4; // 4 byte reserved
    offset += 4; // file size
    short tableCount = BinaryUtils.ReadInt16BigEndian(bytes, offset);
    offset += 2;
    offset += 2; // 2 byte reserved
    offset += 4; // 4 byte reserved
    offset += 8; // 8 byte reserved

    var boneIds = new byte[4];
    var boneWeights = new byte[4];

    List<MeshTableData> tableData = [];
    offset = tableOffset;
    for (int i = 0; i < tableCount; i++) {
      offset += 4; // 4 byte reserved
      int dataType = BinaryUtils.ReadInt32BigEndian(bytes, offset);
      offset += 4;
      int dataOffset = BinaryUtils.ReadInt32BigEndian(bytes, offset);
      offset += 4;
      int dataLength = BinaryUtils.ReadInt32BigEndian(bytes, offset);
      offset += 4;

      tableData.Add(new MeshTableData(dataType, dataOffset, dataLength));

      offset += 2; // 2 byte reserved
      offset += 2; // 2 byte reserved
      offset += 2; // 2 byte reserved
      offset += 2; // 2 byte reserved
    }

    int vertStart = 0;
    int faceStart = 0;

    Dictionary<int, List<MeshPolygonInfo>> meshArr =
        new Dictionary<int, List<MeshPolygonInfo>>();

    var localFinBones = new List<(IBone, IGrannyBone)>();

    for (int i = 0; i < tableCount; i++) {
      var tableChunk = tableData[i];
      offset = tableChunk.offset;

      switch (tableChunk.type) {
        case MeshDataType.GRX_CHUNK: {
          var grxStream =
              new MemoryStream(bytes, tableChunk.offset, tableChunk.length);

          using var grxEr =
              new SchemaBinaryReader(grxStream, Endianness.LittleEndian);

          var grannyFileInfo = new GrannyFileInfo();
          grannyFileInfo.Read(grxEr);

          Asserts.Equal(1, grannyFileInfo.SkeletonHeaderList.Count);
          var skeletonHeader = grannyFileInfo.SkeletonHeaderList[0];

          foreach (var grannyBone in skeletonHeader.Bones) {
            var parentIndex = grannyBone.ParentIndex;

            var isRoot = parentIndex == -1;
            var parentFinBone = isRoot
                ? finModel.Skeleton.Root
                : localFinBones[parentIndex].Item1;

            var position = grannyBone.LocalTransform.Position;
            var rotation =
                QuaternionUtil.ToEulerRadians(
                    grannyBone.LocalTransform.Orientation);
            var scaleShear = grannyBone.LocalTransform.ScaleShear;

            // Halo Wars coordinates have opposite handedness, so we must flip
            // X depending on how many submodels down we are.
            var xSign = this.FlipFaces ? -1 : 1;

            var finBone = 
                isRoot ?
                    parentFinBone.AddRoot(xSign * position.X, position.Y, position.Z)
                    : parentFinBone.AddChild(xSign * position.X, position.Y, position.Z);

            finBone.Transform.SetRotationRadians(rotation.X,
              xSign * rotation.Y,
              xSign * rotation.Z);
            finBone.Transform.SetScale(scaleShear[0].X,
                                            scaleShear[1].Y,
                                            scaleShear[2].Z);

            finBone.Name = grannyBone.Name;

            localFinBones.Add((finBone, grannyBone));
          }


          break;
        }

        case MeshDataType.MESH_INFO:
          offset += 2;  // 2 byte reserved
          offset += 2;  // 2 byte reserved
          offset += 4;  // 4 byte reserved
          offset += 48; // 48 byte reserved
          offset += 4;  // 4 byte reserved
          offset += 4;  // 4 byte reserved

          Dictionary<MeshSubDataType, MeshTableSubData> subDataList =
              new Dictionary<MeshSubDataType, MeshTableSubData>();
          for (int j = 0; j < 6; j++) {
            // Truncating to int because there's no fucking way we need more than 2 billion bytes, none of the files are that big
            int dataCount =
                (int) BinaryUtils.ReadInt64LittleEndian(bytes, offset);
            offset += 8;
            int dataOffset = tableData[i].offset +
                             (int) BinaryUtils.ReadInt64LittleEndian(
                                 bytes, offset);
            offset += 8;
            var subData = new MeshTableSubData(dataCount, dataOffset);
            subDataList.Add((MeshSubDataType) (j + 1), subData);
          }

          var boneData = subDataList[MeshSubDataType.BONE_DATA];

          var data = subDataList[MeshSubDataType.MESH_DATA];
          offset = data.offset;
          for (int j = 0; j < data.count; j++) {
            var polyInfo = new MeshPolygonInfo();
            polyInfo.materialId =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;
            polyInfo.polygonId =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;
            offset += 4; // 4 byte reserved
            polyInfo.boneId =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;
            polyInfo.faceOffset =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;
            polyInfo.faceCount =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;
            polyInfo.vertOffset =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;
            polyInfo.vertLength =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;
            polyInfo.vertSize =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;
            polyInfo.vertCount =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;

            offset += 16; // 16 byte reserved

            int nameOffset =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;
            int location =
                BinaryUtils.ReadInt32LittleEndian(
                    bytes, tableData[i].offset + nameOffset);
            polyInfo.name =
                this.GetStringAt_(bytes, tableData[i].offset + nameOffset);

            offset += 92; // 92 byte reserved

            if (!meshArr.ContainsKey(polyInfo.polygonId)) {
              meshArr.Add(polyInfo.polygonId, []);
            }

            meshArr[polyInfo.polygonId].Add(polyInfo);
          }

          data = subDataList[MeshSubDataType.BONE_DATA];
          offset = data.offset;
          for (int j = 0; j < data.count; j++) {
            int nameOffset =
                BinaryUtils.ReadInt32LittleEndian(bytes, offset);
            offset += 4;
            string boneName =
                this.GetStringAt_(bytes, tableData[i].offset + nameOffset);

            offset += 4; // 4 byte reserved

            offset += 4; // m11
            offset += 4; // m12
            offset += 4; // m13
            offset += 4; // m14

            offset += 4; // m21
            offset += 4; // m22
            offset += 4; // m23
            offset += 4; // m24

            offset += 4; // m31
            offset += 4; // m32
            offset += 4; // m33
            offset += 4; // m34

            offset += 4; // m41
            offset += 4; // m42
            offset += 4; // m43
            offset += 4; // m44

            offset += 4; // parentID
            offset += 4; // 4 byte reserved
          }

          break;
        case MeshDataType.INDEX_DATA:
          faceStart = offset;
          break;
        case MeshDataType.VERTEX_DATA:
          vertStart = offset;
          break;
      }
    }

    foreach (var entry in meshArr) {
      var mesh = finSkin.AddMesh();

      var polygonInfoList = entry.Value;
      for (int i = 0; i < polygonInfoList.Count; i++) {
        var polygonInfo = polygonInfoList[i];
        offset = polygonInfo.vertOffset + vertStart;

        var finVertices = new List<IVertex>();
        for (int j = 0; j < polygonInfo.vertCount; j++) {
          Vector3 position = Vector3.Zero;
          Vector3 normal = Vector3.Zero;
          Vector3 texcoord = Vector3.Zero;

          bool hasBones = false;

          switch (polygonInfo.vertSize) {
            case 0x18:
              this.ReadPosition_(ref position, bytes, ref offset);
              offset += 2; // 2 byte reserved
              this.ReadNormal_(ref normal, bytes, ref offset);
              texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              break;
            case 0x1c:
              this.ReadPosition_(ref position, bytes, ref offset);
              offset += 2; // 2 byte reserved
              this.ReadNormal_(ref normal, bytes, ref offset);
              texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              offset += 2; // 2 byte reserved
              offset += 2; // 2 byte reserved
              break;
            case 0x20:
              this.ReadPosition_(ref position, bytes, ref offset);
              offset += 2; // 2 byte reserved
              this.ReadNormal_(ref normal, bytes, ref offset);

              hasBones = true;
              for (var b = 0; b < 4; ++b) {
                boneIds[b] = BinaryUtils.ReadByteLittleEndian(bytes, offset);
                offset += 1;
              }
              for (var b = 0; b < 4; ++b) {
                boneWeights[b] =
                    BinaryUtils.ReadByteLittleEndian(bytes, offset);
                offset += 1;
              }
              texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              break;
            case 0x24:
              this.ReadPosition_(ref position, bytes, ref offset);
              offset += 2; // 2 byte reserved
              this.ReadNormal_(ref normal, bytes, ref offset);
              offset += 4; // 4 byte reserved
              offset += 4; // 4 byte reserved
              offset += 4; // 4 byte reserved
              texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              break;
            case 0x28:
              this.ReadPosition_(ref position, bytes, ref offset);
              offset += 2; // 2 byte reserved
              this.ReadNormal_(ref normal, bytes, ref offset);
              offset += 4; // 4 byte reserved
              offset += 4; // 4 byte reserved
              offset += 4; // 4 byte reserved
              texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              offset += 4; // 4 byte reserved
              break;
            case 0x2c:
              this.ReadPosition_(ref position, bytes, ref offset);
              offset += 2; // 2 byte reserved
              this.ReadNormal_(ref normal, bytes, ref offset);
              offset += 4; // 4 byte reserved
              offset += 4; // 4 byte reserved
              offset += 4; // 4 byte reserved

              hasBones = true;
              for (var b = 0; b < 4; ++b) {
                boneIds[b] = BinaryUtils.ReadByteLittleEndian(bytes, offset);
                offset += 1;
              }
              for (var b = 0; b < 4; ++b) {
                boneWeights[b] =
                    BinaryUtils.ReadByteLittleEndian(bytes, offset);
                offset += 1;
              }

              texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              break;
            case 0x30:
              this.ReadPosition_(ref position, bytes, ref offset);
              offset += 2; // 2 byte reserved
              this.ReadNormal_(ref normal, bytes, ref offset);
              offset += 4; // 4 byte reserved
              offset += 4; // 4 byte reserved
              offset += 4; // 4 byte reserved

              hasBones = true;
              for (var b = 0; b < 4; ++b) {
                boneIds[b] = BinaryUtils.ReadByteLittleEndian(bytes, offset);
                offset += 1;
              }
              for (var b = 0; b < 4; ++b) {
                boneWeights[b] =
                    BinaryUtils.ReadByteLittleEndian(bytes, offset);
                offset += 1;
              }

              offset += 4; // 4 byte reserved
              texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
              offset += 2;
              break;
            default:
              continue;
          }

          var finVertex = finSkin.AddVertex(position);
          finVertex.SetLocalNormal(normal);
          finVertex.SetUv(texcoord.X, texcoord.Y);

          if (hasBones) {
            var finBoneWeights =
                boneWeights
                    .Select((weight, index) => (weight, index))
                    .Where(weightAndIndex => weightAndIndex.weight > 0)
                    .Select(weightAndIndex => {
                      var weight = weightAndIndex.weight / 255f;
                      var index = weightAndIndex.index;

                      var (finBone, grannyBone) =
                          localFinBones[boneIds[index] - 1];
                      return new BoneWeight(finBone, null, weight);
                    })
                    .ToArray();

            finVertex.SetBoneWeights(
                finSkin.GetOrCreateBoneWeights(
                    VertexSpace.RELATIVE_TO_WORLD, finBoneWeights));
          } else {
            finVertex.SetBoneWeights(
                finSkin.GetOrCreateBoneWeights(
                    VertexSpace.RELATIVE_TO_WORLD, localFinBones[0].Item1));
          }

          finVertices.Add(finVertex);
        }

        var triangles =
            new (IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex)[polygonInfo.faceCount];

        offset = ((polygonInfo.faceOffset * 2) + faceStart);
        for (var j = 0; j < polygonInfo.faceCount; j++) {
          var fa = BinaryUtils.ReadUInt16LittleEndian(bytes, offset);
          offset += 2;
          var fb = BinaryUtils.ReadUInt16LittleEndian(bytes, offset);
          offset += 2;
          var fc = BinaryUtils.ReadUInt16LittleEndian(bytes, offset);
          offset += 2;

          // Halo Wars coordinates have opposite handedness, so we must flip
          // the faces depending on how many submodels down we are.
          if (this.FlipFaces) {
            triangles[j] = (finVertices[fa],
                            finVertices[fb],
                            finVertices[fc]);
          } else {
            triangles[j] = (finVertices[fa],
                            finVertices[fc],
                            finVertices[fb]);
          }

          /*if (!materials.ContainsKey(polygonInfo.MaterialId)) {
            materials.Add(polygonInfo.MaterialId,
                          new GenericMaterial(
                              "material_" + (polygonInfo.MaterialId + 1)));
          }

          if (!sections.ContainsKey(polygonInfo.PolygonId)) {
            sections.Add(polygonInfo.PolygonId,
                         new GenericMeshSection(
                             "object_" + (polygonInfo.PolygonId + 1)));
          }*/
        }

        var finPrimitive = mesh.AddTriangles(triangles);

        var materialId = polygonInfo.materialId;
        if (materialId < finMaterials.Count) {
          finPrimitive.SetMaterial(finMaterials[materialId]);
        } else {
          finPrimitive.SetMaterial(nullMaterial);
        }
      }
    }
  }

  private void ReadPosition_(ref Vector3 position,
                            byte[] bytes,
                            ref int offset) {
    // Halo Wars coordinates have opposite handedness, so we must flip X
    // depending on how many submodels down we are.
    var xSign = this.FlipFaces ? -1 : 1;
    position.X = xSign * BinaryUtils.ReadHalfLittleEndian(bytes, offset);
    offset += 2;
    position.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
    offset += 2;
    position.Z = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
    offset += 2;
  }

  private void ReadNormal_(ref Vector3 normal,
                          byte[] bytes,
                          ref int offset) {
    // Halo Wars coordinates have opposite handedness, so we must flip X
    // depending on how many submodels down we are.
    var xSign = this.FlipFaces ? -1 : 1;
    normal.X = xSign * BinaryUtils.ReadFloatLittleEndian(bytes, offset);
    offset += 4;
    normal.Y = BinaryUtils.ReadFloatLittleEndian(bytes, offset);
    offset += 4;
    normal.Z = BinaryUtils.ReadFloatLittleEndian(bytes, offset);
    offset += 4;
  }

  public struct MeshPolygonInfo {
    public int materialId;
    public int polygonId;
    public int boneId;
    public int faceOffset;
    public int faceCount;
    public int vertOffset;
    public int vertLength;
    public int vertSize;
    public int vertCount;
    public string name;
  }

  public struct MeshTableData(int dataType, int dataOffset, int dataLength) {
    public MeshDataType type = (MeshDataType) dataType;
    public int offset = dataOffset;
    public int length = dataLength;
  }

  public struct MeshTableSubData(int dataCount, int dataOffset) {
    public int offset = dataOffset;
    public int count = dataCount;
  }

  public enum MeshDataType {
    MESH_INFO = 0x700,
    INDEX_DATA = 0x701,
    VERTEX_DATA = 0x702,
    GRX_CHUNK = 0x703,
  }

  public enum MeshSubDataType {
    MESH_DATA = 1,
    BONE_DATA = 2,
    LINK_DATA = 3,
    MESH_ID = 4,
    MIN_BOUND = 5,
    MAX_BOUND = 6
  }
}
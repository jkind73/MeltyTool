using System.Collections.Generic;
using System.Linq;

using fin.schema;

using gx;
using gx.displayList;

using jsystem.G3D_Binary_File_Format;
using jsystem.schema.j3dgraph.bmd.shp1;

using schema.binary;

#pragma warning disable CS8604


namespace jsystem.GCN;

public partial class Bmd {
  public partial class Shp1Section {
    public const string SIGNATURE = "SHP1";
    public DataBlockHeader header;
    public ushort nrBatch;
    public ushort padding;
    public uint batchesOffset;
    public uint shapeRemapTableOffset;
    public short[] shapeRemapTable;
    public uint zero;
    public uint batchAttribsOffset;
    public uint matrixTableOffset;
    public uint dataOffset;
    public uint matrixDataOffset;
    public uint packetLocationsOffset;
    public Batch[] batches;

    public Shp1Section(IBinaryReader br, out bool ok) {
      long position1 = br.Position;
      bool ok1;
      this.header = new DataBlockHeader(br, "SHP1", out ok1);
      if (!ok1) {
        ok = false;
      } else {
        this.nrBatch = br.ReadUInt16();
        this.padding = br.ReadUInt16();
        this.batchesOffset = br.ReadUInt32();
        this.shapeRemapTableOffset = br.ReadUInt32();
        this.zero = br.ReadUInt32();
        this.batchAttribsOffset = br.ReadUInt32();
        this.matrixTableOffset = br.ReadUInt32();
        this.dataOffset = br.ReadUInt32();
        this.matrixDataOffset = br.ReadUInt32();
        this.packetLocationsOffset = br.ReadUInt32();
        long position2 = br.Position;
        {
          br.Position = position1 + (long) this.batchesOffset;
          this.batches = new Batch[(int) this.nrBatch];
          for (int index = 0; index < (int) this.nrBatch; ++index) {
            this.batches[index] = new Batch(br, position1, this);
          }
        }
        {
          br.Position = position1 + (long) this.shapeRemapTableOffset;
          this.shapeRemapTable = br.ReadInt16s(this.nrBatch);
        }
        br.Position = position1 + (long) this.header.size;
        ok = true;
      }
    }

    public enum MatrixType : byte {
      MTX = 0,
      B_BOARD = 1,
      YB_BOARD = 2,
      MULTI = 3,
    }

    public partial class Batch {
      public MatrixType matrixType;

      [Unknown]
      public byte unknown1;

      public ushort nrPacket;
      public ushort attribsOffset;
      public ushort firstMatrixData;
      public ushort firstPacketLocation;

      [Unknown]
      public ushort unknown2;

      public float boundingSphereReadius;
      public float[] boundingBoxMin;
      public float[] boundingBoxMax;
      public PacketLocation[] packetLocations;
      public Packet[] packets;

      public Batch(
          IBinaryReader br,
          long baseoffset,
          Shp1Section parent) {
        this.matrixType = (MatrixType) br.ReadByte();
        this.unknown1 = br.ReadByte();
        this.nrPacket = br.ReadUInt16();
        this.attribsOffset = br.ReadUInt16();
        this.firstMatrixData = br.ReadUInt16();
        this.firstPacketLocation = br.ReadUInt16();
        this.unknown2 = br.ReadUInt16();
        this.boundingSphereReadius = br.ReadSingle();
        this.boundingBoxMin = br.ReadSingles(3);
        this.boundingBoxMax = br.ReadSingles(3);
        long position = br.Position;
        br.Position = baseoffset +
                      (long) parent.batchAttribsOffset +
                      (long) this.attribsOffset;

        var batchAttributes = br.ReadNew<BatchAttributes>();

        this.packets = new Packet[(int) this.nrPacket];
        this.packetLocations = new PacketLocation[(int) this.nrPacket];
        for (int index = 0; index < (int) this.nrPacket; ++index) {
          br.Position = baseoffset +
                        (long) parent.packetLocationsOffset +
                        (long) (((int) this.firstPacketLocation + index) * 8);
          var packetLocation = new PacketLocation();
          packetLocation.Read(br);
          this.packetLocations[index] = packetLocation;

          br.Position = baseoffset +
                        (long) parent.dataOffset +
                        (long) this.packetLocations[index].offset;
          this.packets[index] = new Packet(br,
                                           (int) this.packetLocations[index]
                                               .size,
                                           batchAttributes);
          br.Position = baseoffset +
                        (long) parent.matrixDataOffset +
                        (long) (((int) this.firstMatrixData + index) * 8);
          this.packets[index].matrixData = br.ReadNew<MatrixData>();
          br.Position = baseoffset +
                        (long) parent.matrixTableOffset +
                        (long) (2U *
                                this.packets[index].matrixData
                                    .FirstIndex);
          this.packets[index].matrixTable =
              br.ReadUInt16s((int) this.packets[index].matrixData.Count);
        }

        br.Position = position;
      }

      public sealed class Packet {
        public Primitive[] primitives;
        public ushort[] matrixTable;
        public MatrixData matrixData;

        public Packet(
            IBinaryReader br,
            int length,
            IVertexDescriptor vertexDescriptor) {
          List<Primitive> primitiveList = [];
          var gxDisplayListReader = new GxDisplayListReader();
          br.Subread(
              length,
              () => {
                while (!br.Eof) {
                  var gxPrimitive
                      = gxDisplayListReader.Read(br, vertexDescriptor);
                  if (gxPrimitive == null) {
                    continue;
                  }

                  var primitive = new Primitive();
                  primitive.type = gxPrimitive.PrimitiveType;
                  primitive.points
                      = gxPrimitive
                        .Vertices
                        .Select(v => {
                          var index = new Primitive.Index();

                          index.posIndex = v.PositionIndex;
                          index.matrixIndex = v.JointIndex ?? 0;
                          index.normalIndex = v.NormalIndex;
                          index.colorIndices = v.ColorIndices;
                          index.texCoordIndices = v.TexCoordIndices;

                          return index;
                        })
                        .ToArray();

                  primitiveList.Add(primitive);
                }
              });

          this.primitives = primitiveList.ToArray();
        }

        public sealed class Primitive {
          public GxPrimitiveType type;
          public Index[] points;

          public sealed class Index {
            public ushort?[] colorIndices;
            public ushort?[] texCoordIndices;
            public ushort matrixIndex;
            public ushort posIndex;
            public ushort? normalIndex;
          }
        }
      }
    }
  }
}
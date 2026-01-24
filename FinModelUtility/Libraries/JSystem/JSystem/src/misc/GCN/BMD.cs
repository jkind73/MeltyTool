using System.Collections.Generic;
using System.IO;

using jsystem._3D_Formats;
using jsystem.schema.j3dgraph.bmd;
using jsystem.schema.j3dgraph.bmd.drw1;
using jsystem.schema.j3dgraph.bmd.evp1;
using jsystem.schema.j3dgraph.bmd.inf1;
using jsystem.schema.j3dgraph.bmd.jnt1;
using jsystem.schema.j3dgraph.bmd.tex1;

using schema.binary;

#pragma warning disable CS8604


namespace jsystem.GCN;

public partial class Bmd {
  public BmdHeader header;
  public Inf1 Inf1 { get; set; }
  public Vtx1Section vtx1;
  public Evp1 evp1;
  public Drw1 Drw1 { get; set; }
  public Jnt1 Jnt1 { get; set; }
  public Shp1Section shp1;
  public Mat3Section mat3;
  public Tex1 Tex1 { get; set; }

  public Bmd(byte[] file) {
    using var br =
        new SchemaBinaryReader((Stream) new MemoryStream(file),
                               Endianness.BigEndian);
    this.header = br.ReadNew<BmdHeader>();

    bool ok;
    while (!br.Eof) {
      switch (br.ReadString(4)) {
        case nameof(this.Inf1):
          br.Position -= 4L;
          this.Inf1 = br.ReadNew<Inf1>();
          break;
        case nameof(this.vtx1):
          br.Position -= 4L;
          this.vtx1 = new Vtx1Section(br, out ok);
          if (!ok) {
            // TODO: Message box
            //int num2 = (int) System.Windows.Forms.MessageBox.Show("Error 4");
            return;
          } else
            break;
        case nameof(this.evp1):
          br.Position -= 4L;
          this.evp1 = br.ReadNew<Evp1>();
          break;
        case nameof(this.Drw1):
          br.Position -= 4L;
          this.Drw1 = br.ReadNew<Drw1>();
          break;
        case nameof(this.Jnt1):
          br.Position -= 4L;
          this.Jnt1 = br.ReadNew<Jnt1>();
          break;
        case nameof(this.shp1):
          br.Position -= 4L;
          this.shp1 = new Shp1Section(br, out ok);
          if (!ok) {
            // TODO: Message box
            //int num2 = (int) System.Windows.Forms.MessageBox.Show("Error 7");
            return;
          } else
            break;
        case "MAT1" or "MAT2" or nameof(this.mat3):
          br.Position -= 4L;
          this.mat3 = new Mat3Section(br, out ok);
          if (!ok) {
            // TODO: Message box
            //int num2 = (int) System.Windows.Forms.MessageBox.Show("Error 8");
            return;
          } else
            break;
        case nameof(this.Tex1):
          br.Position -= 4L;
          this.Tex1 = br.ReadNew<Tex1>();
          break;
        default:
          return;
      }
    }
  }

  public Ma.Node[] GetJoints() {
    var nodeIndexStack = new Stack<int>();
    nodeIndexStack.Push(-1);
    var nodeList = new List<Ma.Node>();
    int nodeIndex = -1;
    foreach (Inf1Entry entry in this.Inf1.Data.Entries) {
      switch (entry.Type) {
        case Inf1EntryType.TERMINATOR:
          goto label_7;
        case Inf1EntryType.HIERARCHY_DOWN:
          nodeIndexStack.Push(nodeIndex);
          break;
        case Inf1EntryType.HIERARCHY_UP:
          nodeIndexStack.Pop();
          break;
        case Inf1EntryType.JOINT:
          var jnt1 = this.Jnt1.Data;
          var jointIndex = jnt1.RemapTable[entry.Index];
          nodeList.Add(new Ma.Node(
                           jnt1.Joints[jointIndex],
                           jnt1.StringTable[jointIndex],
                           nodeIndexStack.Peek()));
          nodeIndex = entry.Index;
          break;
        case Inf1EntryType.MATERIAL:
          break;
        case Inf1EntryType.SHAPE:
          break;
      }
    }

    label_7:
    return nodeList.ToArray();
  }
}
using System.Globalization;

using Newtonsoft.Json.Linq;

namespace sm64.JSON {
  public sealed class ModelComboFile {
    private static byte ParseByte_(string str) {
      bool isHex = false;
      if (str.StartsWith("0x")) {
        str = str[2..];
        isHex = true;
      } else if (str.StartsWith("$")) {
        str = str[1..];
        isHex = true;
      }
      if (!isHex)
        return byte.Parse(str);
      else
        return byte.Parse(str, NumberStyles.HexNumber);
    }

    private static uint ParseUInt_(string str) {
      bool isHex = false;
      if (str.StartsWith("0x")) {
        str = str[2..];
        isHex = true;
      } else if (str.StartsWith("$")) {
        str = str[1..];
        isHex = true;
      }
      if (!isHex)
        return uint.Parse(str);
      else
        return uint.Parse(str, NumberStyles.HexNumber);
    }

    public static void WriteObjectCombosFile(string filename) {
      Globals.objectComboEntries.Sort((x, y) => string.Compare(x.Name, y.Name));

      JArray array = [];
      foreach (ObjectComboEntry oce in Globals.objectComboEntries) {
        JObject entry = new JObject();
        entry["Name"] = oce.Name;
        entry["ModelID"] = "0x" + oce.ModelId.ToString("X2");
        entry["ModelAddress"] = "0x" + oce.ModelSegmentAddress.ToString("X8");
        entry["Behavior"] = "0x" + oce.Behavior.ToString("X8");
        if (oce.Bp1Name != null)
          entry["BP1_NAME"] = oce.Bp1Name;
        if (oce.Bp2Name != null)
          entry["BP2_NAME"] = oce.Bp2Name;
        if (oce.Bp3Name != null)
          entry["BP3_NAME"] = oce.Bp3Name;
        if (oce.Bp4Name != null)
          entry["BP4_NAME"] = oce.Bp4Name;
        if (oce.Bp1Description != null)
          entry["BP1_DESCRIPTION"] = oce.Bp1Description;
        if (oce.Bp2Description != null)
          entry["BP2_DESCRIPTION"] = oce.Bp2Description;
        if (oce.Bp3Description != null)
          entry["BP3_DESCRIPTION"] = oce.Bp3Description;
        if (oce.Bp4Description != null)
          entry["BP4_DESCRIPTION"] = oce.Bp4Description;
        array.Add(entry);
      }

      JObject o = new JObject();
      o["ObjectCombos"] = array;

      File.WriteAllText(filename, o.ToString());
    }

    private static bool CheckValidEntry_(JObject entry) {
      return (entry["Name"] != null && entry["ModelID"] != null &&
              entry["ModelAddress"] != null && entry["Behavior"] != null);
    }

    public static void ParseObjectCombos(string filename) {
      if (File.Exists(filename)) {
        string json = File.ReadAllText(filename);
        JObject o = JObject.Parse(json);
        if (o["ObjectCombos"] != null) {
          JArray array = (JArray) o["ObjectCombos"];
          foreach (JToken token in array.Children()) {
            JObject entry = (JObject) token;
            if (CheckValidEntry_(entry)) {
              string name = entry["Name"].ToString();
              string modelIdS = entry["ModelID"].ToString();
              string modelAddressS = entry["ModelAddress"].ToString();
              string behaviorS = entry["Behavior"].ToString();
              byte modelId = ParseByte_(modelIdS);
              uint modelAddress = ParseUInt_(modelAddressS);
              uint behavior = ParseUInt_(behaviorS);
              ObjectComboEntry oce =
                  new ObjectComboEntry(name, modelId, modelAddress, behavior);
              if (entry["BP1_NAME"] != null)
                oce.Bp1Name = entry["BP1_NAME"].ToString();
              if (entry["BP2_NAME"] != null)
                oce.Bp2Name = entry["BP2_NAME"].ToString();
              if (entry["BP3_NAME"] != null)
                oce.Bp3Name = entry["BP3_NAME"].ToString();
              if (entry["BP4_NAME"] != null)
                oce.Bp4Name = entry["BP4_NAME"].ToString();
              if (entry["BP1_DESCRIPTION"] != null)
                oce.Bp1Description = entry["BP1_DESCRIPTION"].ToString();
              if (entry["BP2_DESCRIPTION"] != null)
                oce.Bp2Description = entry["BP2_DESCRIPTION"].ToString();
              if (entry["BP3_DESCRIPTION"] != null)
                oce.Bp3Description = entry["BP3_DESCRIPTION"].ToString();
              if (entry["BP4_DESCRIPTION"] != null)
                oce.Bp4Description = entry["BP4_DESCRIPTION"].ToString();
              Globals.objectComboEntries.Add(oce);
            }
          }
        }
      }
    }
  }
}
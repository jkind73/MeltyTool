using System.Globalization;

using Newtonsoft.Json.Linq;

namespace sm64.JSON {
  public sealed class BehaviorNameFile {
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
        entry["Behavior"] = "0x" + oce.Behavior.ToString("X8");
        array.Add(entry);
      }

      JObject o = new JObject();
      o["BehaviorNames"] = array;

      File.WriteAllText(filename, o.ToString());
    }

    private static bool CheckValidEntry_(JObject entry) {
      return (entry["Name"] != null && entry["Behavior"] != null);
    }

    public static void ParseBehaviorNames(string filename) {
      if (File.Exists(filename)) {
        string json = File.ReadAllText(filename);
        JObject o = JObject.Parse(json);
        if (o["BehaviorNames"] != null) {
          JArray array = (JArray) o["BehaviorNames"];
          foreach (JToken token in array.Children()) {
            JObject entry = (JObject) token;
            if (CheckValidEntry_(entry)) {
              string name = entry["Name"].ToString();
              string behaviorS = entry["Behavior"].ToString();
              uint behavior = ParseUInt_(behaviorS);
              BehaviorNameEntry bne = new BehaviorNameEntry(name, behavior);
              Globals.behaviorNameEntries.Add(bne);
            }
          }
        }
      }
    }
  }
}
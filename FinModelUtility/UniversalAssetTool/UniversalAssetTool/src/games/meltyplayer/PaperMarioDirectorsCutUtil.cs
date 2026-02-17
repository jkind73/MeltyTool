using fin.io;
using fin.io.bundles;
using fin.model.io.importers.assimp;

using gm.api;

using pmdc.api;

namespace uni.games.meltyplayer;

public static class PaperMarioDirectorsCutUtil {
  public static void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory rootDir) {
    foreach (var modFile in rootDir.GetFilesWithFileType(".mod", true)) {
      organizer.Add(new AnnotatedFileBundle<D3dModelFileBundle>(
                        new D3dModelFileBundle {
                            D3dFile = modFile
                        },
                        modFile));
    }

    foreach (var objFile in rootDir.GetFilesWithFileType(".obj", true)) {
      organizer.Add(new AnnotatedFileBundle<AssimpModelFileBundle>(
                        new AssimpModelFileBundle {
                            MainFile = objFile
                        },
                        objFile));
    }

    foreach (var omdFile in rootDir.GetFilesWithFileType(".omd", true)) {
      organizer.Add(new AnnotatedFileBundle<OmdModelFileBundle>(
                        new OmdModelFileBundle {
                            OmdFile = omdFile
                        },
                        omdFile));
    }

    foreach (var lvlFile in rootDir.GetFilesWithFileType(".lvl", true)) {
      organizer.Add(new AnnotatedFileBundle<LvlSceneFileBundle>(
                        new LvlSceneFileBundle {
                            LvlFile = lvlFile,
                            RootDirectory = rootDir
                        },
                        lvlFile));
    }

    var charactersDir = rootDir.AssertGetExistingSubdir("Characters");
    foreach (var characterDir in charactersDir.GetExistingSubdirs()) {
      var animationImageFiles
          = characterDir.GetFilesWithFileType(".gif").ToArray();
      organizer.Add(new PmdcCharacterModelFileBundle {
          AnimationImageFiles = animationImageFiles,
          CharactersDirectory = charactersDir,
      }.Annotate(animationImageFiles[0]));
    }
  }
}
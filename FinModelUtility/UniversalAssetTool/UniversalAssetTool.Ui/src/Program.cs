using System;
using System.Globalization;
using System.Windows.Forms;

using fin.exporter.assimp;
using fin.io;
using fin.model.io.exporters;
using fin.ui;
using fin.ui.rendering.gl;

using pikmin1.api;

using uni.api;
using uni.cli;
using uni.ui.winforms;

namespace uni.ui;

public sealed class Program {
  [STAThread]
  public static void Main(string[] args) {
    UiUtil.Initialize();
    OpenGlVersionService.Init(false);

    Cli.Run(args,
            () => {
              DesignModeUtil.InDesignMode = false;
              ApplicationConfiguration.Initialize();
              Application.Run(new UniversalAssetToolForm());
            },
            () => {
              var swallowDir = new FinDirectory(
                  "C:\\Users\\Ryan\\Documents\\CSharpWorkspace\\MeltyTool\\cli\\roms\\pikmin_1\\extracted\\dataDir\\tekis\\swallow");

              var model = new GlobalModelImporter().Import(
                  new ModModelFileBundle {
                      ModFile = swallowDir.AssertGetExistingFile("swallow.mod"),
                      AnmFile = swallowDir.AssertGetExistingFile("swallow.anm"),
                  });

              new AssimpDirectExporter().ExportModel(new ModelExporterParams {
                Model = model,
                OutputFile = new FinFile("C:\\Users\\Ryan\\Documents\\CSharpWorkspace\\MeltyTool\\cli\\out\\test\\test.fbx"),
              });
            });
  }
}
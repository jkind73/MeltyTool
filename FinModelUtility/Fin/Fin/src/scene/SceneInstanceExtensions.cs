using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

using fin.color;
using fin.data.queues;
using fin.model;
using fin.model.util;
using fin.scene.components;
using fin.schema.vector;
using fin.ui;
using fin.ui.rendering;
using fin.util.linq;


namespace fin.scene;

public static class SceneInstanceExtensions {
  public static IEnumerable<ISceneNodeInstance> EnumerateAllNodes(
      this ISceneInstance sceneInstance) {
    var queue = new FinQueue<ISceneNodeInstance>(
        sceneInstance.Areas.SelectMany(a => a.RootNodes));
    while (queue.TryDequeue(out var node)) {
      yield return node;
      queue.Enqueue(node.ChildNodes);
    }
  }

  public static IEnumerable<IAnimatableModel> EnumerateAllAnimatableModels(
      this ISceneInstance scene) {
    var queue = new FinQueue<ISceneModelInstance>();
    foreach (var node in scene.EnumerateAllNodes()) {
      queue.Enqueue(node.Models);

      foreach (var modelRenderComponent
               in node.Definition.Components
                      .WhereIs<ISceneNodeComponent, IModelRenderComponent>()) {
        yield return modelRenderComponent;
      }
    }

    while (queue.TryDequeue(out var model)) {
      yield return model;
      queue.Enqueue(model.Children.Values);
    }
  }
}
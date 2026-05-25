using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetupGalleryPanels
{
    [MenuItem("Tools/Setup Gallery Panels (4资料2 ~ 4资料21)")]
    public static void Run()
    {
        var scene = SceneManager.GetActiveScene();
        GameObject source = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            source = FindRecursive(root.transform, "4资料1");
            if (source != null) break;
        }

        if (source == null)
        {
            Debug.LogError("找不到 4资料1");
            return;
        }

        var sourceGC = source.GetComponent<GalleryController>();
        if (sourceGC == null)
        {
            Debug.LogError("4资料1 上没有 GalleryController 组件");
            return;
        }

        var parent = source.transform.parent;
        if (parent == null)
        {
            Debug.LogError("4资料1 没有父节点");
            return;
        }

        int count = 0;
        for (int i = 2; i <= 21; i++)
        {
            var child = parent.Find("4资料" + i);
            if (child == null) continue;

            var go = child.gameObject;
            var existing = go.GetComponent<GalleryController>();
            if (existing != null)
                Undo.DestroyObjectImmediate(existing);

            var gc = Undo.AddComponent<GalleryController>(go);
            EditorUtility.CopySerialized(sourceGC, gc);
            count++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"已为 {count} 个面板添加 GalleryController 并复制配置。");
    }

    private static GameObject FindRecursive(Transform t, string name)
    {
        if (t.name == name) return t.gameObject;
        for (int i = 0; i < t.childCount; i++)
        {
            var found = FindRecursive(t.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}

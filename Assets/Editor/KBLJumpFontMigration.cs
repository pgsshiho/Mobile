using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 씬과 프리팹의 TMP 글꼴을 프로젝트 표준 글꼴로 통일한다.
/// 새 텍스트는 TMP Settings의 기본 글꼴 설정을 사용한다.
/// </summary>
[InitializeOnLoad]
public static class KBLJumpFontMigration
{
    private const string FontPath = "Assets/Fonts/KBLJump/KBLJump_EB_Condensed SDF.asset";
    private const string MigrationKey = "KBLJumpFontMigration_v1";

    static KBLJumpFontMigration()
    {
        EditorApplication.delayCall += MigrateOnce;
    }

    [MenuItem("Tools/OverCharge/폰트/KBLJump 폰트 전체 적용")]
    public static void MigrateOnce()
    {
        if (EditorPrefs.GetBool(MigrationKey, false)) return;

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
        {
            Debug.LogError("[Font] KBLJump_EB_Condensed SDF 폰트 에셋을 찾지 못했습니다.");
            return;
        }

        int changedAssets = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool changed = Apply(root.GetComponentsInChildren<TMP_Text>(true), font);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                changedAssets++;
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Scene scene = SceneManager.GetSceneByPath(path);
            bool wasAlreadyOpen = scene.isLoaded;
            if (!wasAlreadyOpen)
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            bool changed = false;
            foreach (GameObject root in scene.GetRootGameObjects())
                changed |= Apply(root.GetComponentsInChildren<TMP_Text>(true), font);
            if (changed)
            {
                EditorSceneManager.SaveScene(scene);
                changedAssets++;
            }
            if (!wasAlreadyOpen)
                EditorSceneManager.CloseScene(scene, true);
        }

        AssetDatabase.SaveAssets();
        EditorPrefs.SetBool(MigrationKey, true);
        Debug.Log($"[Font] KBLJump 폰트 적용 완료: {changedAssets}개 에셋");
    }

    private static bool Apply(TMP_Text[] texts, TMP_FontAsset font)
    {
        bool changed = false;
        foreach (TMP_Text text in texts)
        {
            if (text.font == font) continue;
            text.font = font;
            EditorUtility.SetDirty(text);
            changed = true;
        }
        return changed;
    }
}

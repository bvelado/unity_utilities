using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class EnterPlayModeFromFirstSceneInBuild
{
    private const string LOAD_LAST_SCENES_ON_ENTERED_EDIT_MODE_PREF = "LoadLastScenesOnEnteredEditMode";
    private const string LAST_SCENES_BUILD_INDEXES_PREF = "LastScenesBuildIndexes";
    private const string LAST_ACTIVE_SCENE_BUILD_INDEX_PREF = "LastActiveSceneBuildIndex";

    private const char SCENE_INDEXES_SEPARATOR = ',';

    static EnterPlayModeFromFirstSceneInBuild()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Utilities/Play from first scene in build")]
    public static void EnterPlayMode()
    {
        if (EditorApplication.isPlaying == true)
        {
            return;
        }

        if(EditorBuildSettings.scenes.Length < 1)
        {
            Debug.LogWarning("No scene has been added to the build settings.");
            return;
        }

        // Register current scenes
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < EditorSceneManager.loadedSceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);

            if(EditorSceneManager.GetActiveScene() == scene)
            {
                EditorPrefs.SetInt(LAST_ACTIVE_SCENE_BUILD_INDEX_PREF, scene.buildIndex);
            }
            sb.Append(scene.buildIndex);
            if(i < EditorSceneManager.sceneCount - 1)
            {
                sb.Append(SCENE_INDEXES_SEPARATOR);
            }
        }

        // Register the current scenes build indexes (comma separated)
        EditorPrefs.SetString(LAST_SCENES_BUILD_INDEXES_PREF, sb.ToString());

        sb.Clear();
        sb = null;

        // Prompt to save current scenes changes
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // Set the callback flag
        EditorPrefs.SetBool(LOAD_LAST_SCENES_ON_ENTERED_EDIT_MODE_PREF, true);

        // Open the first scene in build settings
        EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(0), OpenSceneMode.Single);
        EditorApplication.isPlaying = true;        
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange newState)
    {
        if (newState == PlayModeStateChange.EnteredEditMode && EditorPrefs.GetBool(LOAD_LAST_SCENES_ON_ENTERED_EDIT_MODE_PREF, false))
        {
            // Reset the callback flag
            EditorPrefs.SetBool(LOAD_LAST_SCENES_ON_ENTERED_EDIT_MODE_PREF, false);
            
            // Split the build indexes
            var buildIndexes = EditorPrefs.GetString(LAST_SCENES_BUILD_INDEXES_PREF).Split(SCENE_INDEXES_SEPARATOR);
            var activeSceneIndex = EditorPrefs.GetInt(LAST_ACTIVE_SCENE_BUILD_INDEX_PREF, 0);

            for (int i = 0; i < buildIndexes.Length; i++)
            {
                int buildIndex = -1;
                if (int.TryParse(buildIndexes[i], out buildIndex))
                {
                    var scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
                    var loadedScene = EditorSceneManager.OpenScene(scenePath, i != 0 ? OpenSceneMode.Additive : OpenSceneMode.Single);

                    if (buildIndex == activeSceneIndex)
                    {
                        EditorSceneManager.SetActiveScene(loadedScene);
                    }
                }
            }
        }
    }
}

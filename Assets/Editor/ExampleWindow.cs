using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.IO;

[System.Serializable]
public class BuildSceneData {
    public List<SceneAsset> sceneAssets;
}

public class ExampleWindow : EditorWindow {
    private BuildSceneData data;
    private ReorderableList reorderableList;

    // Add menu item named "Example Window" to the Window menu
    [MenuItem("Window/Example Window %g")]
    public static void ShowWindow() {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow(typeof(ExampleWindow));
    }

    private void OnEnable() {
        //If Data is not Created Yet
        if (data == null) {
            //Create new ScriptableObject Asset
            string _dataPath = Application.temporaryCachePath + "/tempsavetool.scenelist";
            if (File.Exists(_dataPath)) {
                data = JsonUtility.FromJson<BuildSceneData>(File.ReadAllText(_dataPath));

                if (data.sceneAssets == null) {
                    data.sceneAssets = new List<SceneAsset>(EditorBuildSettings.scenes.Length);
                    //Load Build Scenes into List
                    foreach (var _scene in EditorBuildSettings.scenes) {
                        data.sceneAssets.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(_scene.path));
                    }

                    saveTempDataAsset();
                }
            } else {
                File.WriteAllText(_dataPath, JsonUtility.ToJson(data));
                data = new BuildSceneData();
                data.sceneAssets = new List<SceneAsset>(EditorBuildSettings.scenes.Length);

                //Load Build Scenes into List
                foreach (var _scene in EditorBuildSettings.scenes) {
                    data.sceneAssets.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(_scene.path));
                }

                saveTempDataAsset();
            }
        }

        if (reorderableList == null || reorderableList.list == null) {
            Debug.Log("Reorder List Created");

            //Setup Reorderable List for the Scene List
            reorderableList = new ReorderableList(data.sceneAssets, typeof(BuildSceneData), true, true, true, true);

            //Callbacks for the Reorderable List
            reorderableList.drawHeaderCallback = drawHeaderCallback;
            reorderableList.drawElementCallback = drawElementCallback;
            reorderableList.onAddCallback = onAddCallback;
            reorderableList.onChangedCallback = onChangeCallback;
        }
    }

    void drawHeaderCallback(Rect rect) {
        EditorGUI.LabelField(rect, "Build Scenes");
    }

    void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused) {
        Rect _buttonRect = rect;
        _buttonRect.width /= 3f;

        GUI.enabled = data.sceneAssets[index] != null;
        GUI.Button(_buttonRect, "Test");

        Rect _objectRect = rect;
        _objectRect.width -= (_buttonRect.width + 2);
        _objectRect.x += _buttonRect.width + 2;
        GUI.enabled = true;
        var _sceneObj = (SceneAsset)EditorGUI.ObjectField(_objectRect, data.sceneAssets[index], typeof(SceneAsset), false);

        if (_sceneObj != data.sceneAssets[index]) {
            Debug.Log("Scene Field Updated");
            data.sceneAssets[index] = _sceneObj;
            saveTempDataAsset();
        }
    }

    void onAddCallback(ReorderableList l) {
        data.sceneAssets.Add(null);
    }

    void onChangeCallback(ReorderableList l) {
        saveTempDataAsset();
    }

    void OnGUI() {
        reorderableList.DoLayoutList();

        GUILayout.Space(5);
        if (GUILayout.Button("Apply To Build Settings")) {
            SetEditorBuildSettingsScenes();
        }
    }

    public void SetEditorBuildSettingsScenes() {
        // Find valid Scene paths and make a list of EditorBuildSettingsScene
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        foreach (var sceneAsset in data.sceneAssets) {
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            if (!string.IsNullOrEmpty(scenePath))
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
        }

        // Set the Build Settings window Scene list
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }

    void saveTempDataAsset() {
        string _dataPath = Application.temporaryCachePath + "/tempsavetool.scenelist";
        File.WriteAllText(_dataPath, JsonUtility.ToJson(data));

        Debug.Log(data.sceneAssets.Count);

        Debug.Log("Applying to Build Settings");
        SetEditorBuildSettingsScenes();
    }
}
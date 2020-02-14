using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class SceneListEditor : EditorWindow {
    #region TemplateVariable
    private Color editorSkinColor = new Color(0.6901961f, 0.6980392f, 0.7098039f, 1f);
    private Color editorSkinBGColor = new Color(0.224f, 0.243f, 0.275f, 0.6f);
    private GUIContent authorInfo = new GUIContent("Author - Jisan Haider Joy", "Find me at,\nhttps://www.facebook.com/jisanhaiderjoy");
    private GUIContent toolTitle = new GUIContent("Scene List", "Title ToolTip");
    private string version = "version - 1.0.0";
    #endregion

    #region ToolbarVariable
    private int topToolbarSelection = 0;
    private GUIContent[] topToolbarNames = {
        new GUIContent("All Scenes", "List of all available Scenes"),
        new GUIContent("Starred", "List of all Starred Scenes"),
        new GUIContent("Others", "Other Tooltip")
    };
    #endregion

    #region AllSceneVariables
    GUIContent[] sceneNames;
    GUIContent[] scenePaths;

    //Star Icon Properties
    private GUIStyle starStyle;
    private bool[] starToggles = new bool[50];

    //SceneName Properties
    private GUIStyle sceneNameStyle;
    Rect[] _SceneNameRects = new Rect[50];

    //Scene Path Properties
    private GUIStyle scenePathStyle;
    Rect[] _ScenePathRects = new Rect[50];

    //Scroll View Properties
    private Vector2 scrollPos;
    Rect[] _HorizontalRects = new Rect[50];
    #endregion

    #region globalVariables
    private string projectPath;
    #endregion

    [MenuItem("Tools/Scene List v2")]
    static void InitWindow() {
        // Get existing open window or if none, make a new one:
        SceneListEditor window = (SceneListEditor)GetWindow(typeof(SceneListEditor));
        window.minSize = new Vector2(300, 300);
        window.Show();
    }

    private void OnEnable() {
        //Load Title Scene Asset Icon
        toolTitle.image = (Texture)EditorGUIUtility.Load("SceneAsset Icon");

        if (starStyle == null)
            InitStarToggleIcon();

        if (sceneNameStyle == null)
            InitSceneNameStyle();

        if (scenePathStyle == null)
            InitScenePathStyle();

        //Load Scenes into Array
        projectPath = Application.dataPath;
        RefreshList();
    }

    private void OnProjectChange() {
        RefreshList();
        Repaint();
    }

    private void RefreshList() {
        //allScenes = new List<sceneData>();
        string[] _searchedPaths = Directory.GetFiles(projectPath, "*.unity", SearchOption.AllDirectories);
        int _sceneCount = _searchedPaths.Length;

        sceneNames = new GUIContent[_sceneCount];
        scenePaths = new GUIContent[_sceneCount];

        starToggles = new bool[_sceneCount];
        _SceneNameRects = new Rect[_sceneCount];
        _ScenePathRects = new Rect[_sceneCount];
        _HorizontalRects = new Rect[_sceneCount];

        for (int i = 0; i < _sceneCount; i++) {
            string _relativePath = _searchedPaths[i].Substring(Application.dataPath.Length - 6);
            scenePaths[i] = new GUIContent(_relativePath);
            sceneNames[i] = new GUIContent(_relativePath.Substring(_relativePath.LastIndexOfAny(new[] { '/', '\\' }) + 1).Replace(".unity", ""));
        }
    }

    private void OnGUI() {
        //DrawGUITemplate
        DrawTemplate();

        //Toolbar Selection Design
        #region TopToolBar Design
        //Toolbar Background Rect Design
        EditorGUI.DrawRect(new Rect(0, 47, position.width, 50), editorSkinColor);

        //ToolBar Y Pos Spacing
        GUILayout.Space(57); //47 + 25 - 15 //BackgroundRect_Y + BackgroundRect_H/2 - ToolBar_H/2

        //For X Post Spacing
        EditorGUILayout.BeginHorizontal();
        //ToolBar X Pos Spacing
        GUILayout.Space(45);
        //ToolBar GUI Create
        topToolbarSelection = GUILayout.Toolbar(topToolbarSelection, topToolbarNames, GUILayout.Height(30));
        //ToolBar X Pos Spacing
        GUILayout.Space(45);
        EditorGUILayout.EndHorizontal();
        #endregion

        if (topToolbarSelection == 0) {
            GUILayout.Space(15);
            if (GUILayout.Button("Refresh List", GUILayout.Height(20f))) {
                RefreshList();
            }

            DrawAllScene();
            Repaint();
        } else if (topToolbarSelection == 1) {
            DrawStarredScene();
            Repaint();
        }
    }

    private void DrawAllScene() {
        GUILayout.Space(3f);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(position.height - 104 - 37 - 23f));
        {
            for (int i = 0; i < sceneNames.Length; i++) {
                var _tempRectHorizontal = EditorGUILayout.BeginHorizontal("Box", GUILayout.MinHeight(46));
                {
                    if (Event.current.type == EventType.Repaint) {
                        _HorizontalRects[i] = _tempRectHorizontal;
                    }

                    //Scene Element Background
                    EditorGUI.DrawRect(_HorizontalRects[i], editorSkinColor);

                    //Center position for each element
                    float _horizontalCenter = _HorizontalRects[i].height / 2f;

                    GUILayout.Space(3);
                    //Toggle Design
                    #region Toggle Design
                    EditorGUILayout.BeginVertical();
                    {
                        GUILayout.Space(_horizontalCenter - 17.5f);
                        starToggles[i] = EditorGUILayout.Toggle(starToggles[i], starStyle, GUILayout.Height(25), GUILayout.Width(25));
                    }
                    EditorGUILayout.EndVertical();
                    #endregion

                    GUILayout.Space(3);
                    //Text Design
                    #region Text Design
                    float _TextWidth = position.width - 170;
                    EditorGUILayout.BeginVertical();
                    {
                        GUILayout.Space(8f);

                        //Rect Create
                        var _tempRect = GUILayoutUtility.GetRect(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));
                        var _tempRect2 = GUILayoutUtility.GetRect(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));

                        if (Event.current.type == EventType.Repaint) {
                            _SceneNameRects[i] = _tempRect;
                            _ScenePathRects[i] = _tempRect2;
                        }

                        //Scene Name Area
                        GUILayout.BeginArea(_SceneNameRects[i]);
                        {
                            //Scene Name Label
                            EditorGUILayout.LabelField(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));
                        }
                        GUILayout.EndArea();

                        //Scene Path Area
                        GUILayout.BeginArea(_ScenePathRects[i]);
                        {
                            //Scene Path Label
                            EditorGUILayout.LabelField(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));
                        }
                        GUILayout.EndArea();

                        GUILayout.Space(8f);
                    }
                    EditorGUILayout.EndVertical();
                    #endregion

                    #region ButtonGroup1
                    EditorGUILayout.BeginVertical();
                    //Button Vertical Center Spacing
                    GUILayout.Space(_horizontalCenter - 22.5f);

                    if (GUILayout.Button("Open", GUILayout.Height(18))) {
                        if (EditorSceneManager.GetActiveScene().isDirty) {
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        }

                        EditorSceneManager.OpenScene(scenePaths[i].text, OpenSceneMode.Single);
                    }

                    if (GUILayout.Button("Add", GUILayout.Height(18))) {
                        if (EditorSceneManager.GetActiveScene().isDirty) {
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        }

                        EditorSceneManager.OpenScene(scenePaths[i].text, OpenSceneMode.Additive);
                    }
                    EditorGUILayout.EndVertical();
                    #endregion

                    #region ButtonGroup2
                    EditorGUILayout.BeginVertical();
                    //Button Vertical Center Spacing
                    GUILayout.Space(_horizontalCenter - 22.5f);

                    if (GUILayout.Button("Ping", GUILayout.Height(18))) {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(scenePaths[i].text, typeof(SceneAsset)));
                    }

                    if (GUILayout.Button("Delete", GUILayout.Height(18))) {
                        if (EditorUtility.DisplayDialog("Delete Selected Scene?", scenePaths[i].text + "\nAre you sure you want to delete the scene? \nYou can't Undo this action", "Yes", "No")) {
                            AssetDatabase.DeleteAsset(scenePaths[i].text);

                            //UpdateDatabase
                            RefreshList();
                        }
                    }
                    EditorGUILayout.EndVertical();
                    #endregion

                    GUILayout.Space(3f);
                }
                EditorGUILayout.EndHorizontal(); //End EachScene Horizontal Block
            } //End SceneList Loop
        }
        EditorGUILayout.EndScrollView(); //End ScrollView
    }

    private void DrawStarredScene() {
        GUILayout.Space(15);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(position.height - 104 - 37));
        {
            for (int i = 0; i < starToggles.Length; i++) {
                if (!starToggles[i])
                    continue;

                var _tempRectHorizontal = EditorGUILayout.BeginHorizontal("Box", GUILayout.MinHeight(46));
                {
                    if (Event.current.type == EventType.Repaint) {
                        _HorizontalRects[i] = _tempRectHorizontal;
                    }

                    //Scene Element Background
                    EditorGUI.DrawRect(_HorizontalRects[i], editorSkinColor);

                    //Center position for each element
                    float _horizontalCenter = _HorizontalRects[i].height / 2f;

                    GUILayout.Space(3);
                    //Text Design
                    #region Text Design
                    float _TextWidth = position.width - 150;
                    EditorGUILayout.BeginVertical();
                    {
                        GUILayout.Space(8f);

                        //Rect Create
                        var _tempRect = GUILayoutUtility.GetRect(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));
                        var _tempRect2 = GUILayoutUtility.GetRect(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));

                        if (Event.current.type == EventType.Repaint) {
                            _SceneNameRects[i] = _tempRect;
                            _ScenePathRects[i] = _tempRect2;
                        }

                        //Scene Name Area
                        GUILayout.BeginArea(_SceneNameRects[i]);
                        {
                            //Scene Name Label
                            EditorGUILayout.LabelField(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));
                        }
                        GUILayout.EndArea();

                        //Scene Path Area
                        GUILayout.BeginArea(_ScenePathRects[i]);
                        {
                            //Scene Path Label
                            EditorGUILayout.LabelField(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));
                        }
                        GUILayout.EndArea();

                        GUILayout.Space(8f);
                    }
                    EditorGUILayout.EndVertical();
                    #endregion

                    #region ButtonGroup1
                    EditorGUILayout.BeginVertical();
                    //Button Vertical Center Spacing
                    GUILayout.Space(_horizontalCenter - 22.5f);

                    if (GUILayout.Button("Open", GUILayout.Height(18))) {
                        if (EditorSceneManager.GetActiveScene().isDirty) {
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        }

                        EditorSceneManager.OpenScene(scenePaths[i].text, OpenSceneMode.Single);
                    }

                    if (GUILayout.Button("Add", GUILayout.Height(18))) {
                        if (EditorSceneManager.GetActiveScene().isDirty) {
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        }

                        EditorSceneManager.OpenScene(scenePaths[i].text, OpenSceneMode.Additive);
                    }
                    EditorGUILayout.EndVertical();
                    #endregion

                    #region ButtonGroup2
                    EditorGUILayout.BeginVertical();
                    //Button Vertical Center Spacing
                    GUILayout.Space(_horizontalCenter - 22.5f);

                    if (GUILayout.Button("Ping", GUILayout.Height(18))) {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(scenePaths[i].text, typeof(SceneAsset)));
                    }

                    if (GUILayout.Button("Delete", GUILayout.Height(18))) {
                        if (EditorUtility.DisplayDialog("Delete Selected Scene?", scenePaths[i].text + "\nAre you sure you want to delete the scene? \nYou can't Undo this action", "Yes", "No")) {
                            AssetDatabase.DeleteAsset(scenePaths[i].text);

                            //UpdateDatabase
                            RefreshList();
                        }
                    }
                    EditorGUILayout.EndVertical();
                    #endregion

                    GUILayout.Space(3f);
                }
                EditorGUILayout.EndHorizontal(); //End EachScene Horizontal Block
            } //End SceneList Loop
        }
        EditorGUILayout.EndScrollView(); //End ScrollView
    }

    #region Initialization
    private void InitStarToggleIcon() {
        starStyle = new GUIStyle();

        //Inactive Star Icon
        starStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SceneListTool/star_inactive.png");
        starStyle.hover.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SceneListTool/star_inactive.png");
        starStyle.active.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SceneListTool/star_inactive.png");

        //Active Star Icon
        starStyle.onNormal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SceneListTool/star_active.png");
        starStyle.onHover.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SceneListTool/star_active.png");
        starStyle.onActive.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SceneListTool/star_active.png");
    }

    private void InitSceneNameStyle() {
        sceneNameStyle = new GUIStyle {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
    }

    private void InitScenePathStyle() {
        scenePathStyle = new GUIStyle {
            fontSize = 10,
            fontStyle = FontStyle.Normal,
            wordWrap = true
        };
    }
    #endregion

    #region DesignTemplate
    private void DrawTemplate() {
        //Background Color
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), editorSkinBGColor);
        //Initializations
        GUIStyle style = new GUIStyle();

        //Top Design
        #region Top Design
        float TopRectHeight = 40;
        //Label Background Rects
        EditorGUI.DrawRect(new Rect(0, 0, position.width, TopRectHeight), editorSkinColor);

        //Tool Name Style
        style.alignment = TextAnchor.UpperCenter;
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 13;

        //Tool Name Text
        GUI.Label(new Rect(0, (TopRectHeight / 2f - 10f), position.width, 20f), toolTitle, style);
        #endregion

        //Bottom Design TradeMark
        #region bottomdesign Trademark
        EditorGUI.DrawRect(new Rect(0, position.height - 30f, position.width, 30f), editorSkinColor);
        GUI.Label(new Rect(10, position.height - 22.25f, 160f, 15f), authorInfo);
        GUI.Label(new Rect(position.width - 95f, position.height - 22.25f, position.width, 15f), version);
        #endregion

        #region ColorSettings
        editorSkinColor = EditorGUI.ColorField(new Rect(170f, position.height - 22.25f, 60f, 15f), editorSkinColor);
        editorSkinBGColor = EditorGUI.ColorField(new Rect(240f, position.height - 22.25f, 60f, 15f), editorSkinBGColor);
        #endregion
    }
    #endregion
}

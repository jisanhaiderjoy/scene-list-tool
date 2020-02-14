using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class SceneListEditor : EditorWindow {
    #region TEMPLATE_VARIABLES
    private Color editorSkinColor = new Color(0.6901961f, 0.6980392f, 0.7098039f, 1f);
    private Color editorSkinBGColor = new Color(0.224f, 0.243f, 0.275f, 0.6f);
    private GUIContent authorInfo = new GUIContent("Author - Jisan Haider Joy", "Find me at,\nhttps://www.facebook.com/jisanhaiderjoy");
    private GUIContent toolTitle = new GUIContent("Scene List", "Title ToolTip");
    private string version = "version - 1.0.0";
    #endregion

    #region TOOLBAR_VARIABLES
    private int topToolbarSelection = 0;
    private GUIContent[] topToolbarNames = {
        new GUIContent("All Scenes", "List of all available Scenes"),
        new GUIContent("Starred", "List of all Starred Scenes"),
        new GUIContent("Build List", "List of scenes added to build, can be managed from here")
    };
    #endregion

    #region ALLSCENE_VARIABLES
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

    #region GLOBAL_VARIABLES
    private string projectPath;
    #endregion

    #region BUTTON_GROUP_GUICONTENT
    GUIContent Open = new GUIContent("Open", "Open Scene Single");
    GUIContent Add = new GUIContent("Add", "Open Scene Additive");
    GUIContent Ping = new GUIContent("Ping", "Select file on the project window");
    GUIContent Delete = new GUIContent("Delete", "Delete the selected scene");
    GUIContent AddToBuild = new GUIContent("Add To Build", "Add to Build list and adjust if needed");
    #endregion

    #region UNITY_CALLBACKS
    /// <summary>
    /// Called by Unity to Open the Window
    /// </summary>
    [MenuItem("Tools/Scene List v2")]
    static void InitWindow() {
        // Get existing open window or if none, make a new one:
        SceneListEditor window = (SceneListEditor)GetWindow(typeof(SceneListEditor));
        window.minSize = new Vector2(300, 300);
        window.Show();
    }

    /// <summary>
    /// Called by Unity For Initialization
    /// </summary>
    private void OnEnable() {
        //Load Toolbar icon
        topToolbarNames[0].image = (Texture)EditorGUIUtility.Load("SceneAsset Icon");
        topToolbarNames[1].image = (Texture)EditorGUIUtility.Load("Favorite Icon");
        topToolbarNames[2].image = (Texture)EditorGUIUtility.Load("CustomSorting");

        //Load Title Scene Asset Icon
        toolTitle.image = (Texture)EditorGUIUtility.Load("SceneAsset Icon");

        //Load Button Asset Icon
        AddToBuild.image = (Texture)EditorGUIUtility.Load("UnityEditor.SceneHierarchyWindow");

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

    /// <summary>
    /// Called by Unity, When the Project is changed in anyway.
    /// Refreshes the Scene List if a Scene is Added/Deleted/Modified
    /// </summary>
    private void OnProjectChange() {
        RefreshList();
        Repaint();
    }

    /// <summary>
    /// Called by Unity on every frame of GUI
    /// </summary>
    private void OnGUI() {
        #region TEMPLATE_DESIGN_DRAW
        //DrawGUITemplate
        DrawTemplate();
        #endregion

        //Toolbar Selection Design
        #region TOP_TOOLBAR_DESIGN
        //Toolbar Background Rect Design
        EditorGUI.DrawRect(new Rect(0, 47, position.width, 50), editorSkinColor);

        //ToolBar Y Pos Spacing
        GUILayout.Space(57); //47 + 25 - 15 //BackgroundRect_Y + BackgroundRect_H/2 - ToolBar_H/2

        //For X Post Spacing
        EditorGUILayout.BeginHorizontal();
        //ToolBar X Pos Spacing
        GUILayout.Space(25);
        //ToolBar GUI Create
        topToolbarSelection = GUILayout.Toolbar(topToolbarSelection, topToolbarNames, GUILayout.Width(position.width - 50f), GUILayout.Height(30));
        //ToolBar X Pos Spacing
        GUILayout.Space(25);
        EditorGUILayout.EndHorizontal();
        #endregion

        #region BODY_DRAW
        //Based on the Toolbar Selection, UI is Drawn
        if (topToolbarSelection == 0) {
            //Draws the "All Scenes" List.
            DrawAllScene();
            //Repaints the GUI. Avoids Drawing Lag
            Repaint();
        } else if (topToolbarSelection == 1) {
            //Draws the "Starred Scene" List.
            DrawStarredScene();
            //Repaints the GUI. Avoids Drawing Lag
            Repaint();
        }
        #endregion
    }
    #endregion

    #region MAINCONTENT_DRAW
    /// <summary>
    /// Draws the "All Scenes" List. In other terms, Draws the List of All available Scenes in the Project
    /// </summary>
    private void DrawAllScene() {
        //Creates an Initial Spacing from the Toolbar
        GUILayout.Space(15);

        //Refresh List Button
        if (GUILayout.Button(new GUIContent("Refresh List", (Texture)EditorGUIUtility.Load("RotateTool"), ""), GUILayout.Height(20f))) {
            RefreshList();
            Repaint();
        }

        //A Spacing after the Refresh Button
        GUILayout.Space(3f);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(position.height - 104 - 37 - 23f));
        {
            //Loops through all the scenes found in the Project
            for (int i = 0; i < sceneNames.Length; i++) {
                //The main Horizontal Area of Each Elements. It's Rect Component is stored to calculate the Center.
                //Helps for making the Editor Responsive
                var _tempRectHorizontal = EditorGUILayout.BeginHorizontal("Box", GUILayout.MinHeight(46));
                {
                    //Only when the Window is repainted, the Temp Rect Value will be stored for that Element.
                    //Otherwise, Gives wrong value
                    if (Event.current.type == EventType.Repaint) {
                        _HorizontalRects[i] = _tempRectHorizontal;
                    }

                    //Scene Element Background
                    EditorGUI.DrawRect(_HorizontalRects[i], editorSkinColor);

                    //Center position for each element
                    float _horizontalCenter = _HorizontalRects[i].height / 2f;

                    //Creates a small space before drawing the StarToggle
                    GUILayout.Space(3);
                    //Toggle Design
                    #region TOGGLE_DESIGN
                    //Creates Vertical Area to Draw Label
                    EditorGUILayout.BeginVertical();
                    {
                        //Spacing added to Keep the Toggle at Center
                        GUILayout.Space(_horizontalCenter - 17.5f);
                        //Toggle Create with size 25x25
                        starToggles[i] = EditorGUILayout.Toggle(starToggles[i], starStyle, GUILayout.Height(25), GUILayout.Width(25));
                    }
                    EditorGUILayout.EndVertical();
                    #endregion

                    //Creates a small space before drawing the Text Labels
                    GUILayout.Space(3);
                    //Text Design
                    #region TEXT_LABELS_DESIGN
                    //The Width of the Text is defined as per the Window Width
                    float _TextWidth = position.width - 180f;
                    //Vertical Area Created To Draw Two Labels on top of each other
                    EditorGUILayout.BeginVertical();
                    {
                        //Spacing added to keep the Text Labels at Center of Background
                        GUILayout.Space(_horizontalCenter - 4f);
                        GUILayout.Space(-(_SceneNameRects[i].height / 2f + _ScenePathRects[i].height / 2f));

                        //Rect Create. A Space is reserved calculating the Name Length and Available Space.
                        //This is done to make the label Responsive.
                        var _tempRect = GUILayoutUtility.GetRect(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));
                        var _tempRect2 = GUILayoutUtility.GetRect(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));

                        //The Temp value is stored only when the Editor is Repainted. Otherwise gives wrong value
                        if (Event.current.type == EventType.Repaint) {
                            _SceneNameRects[i] = _tempRect;
                            _ScenePathRects[i] = _tempRect2;
                        }

                        //Scene Name Area Created, and Label Drawn
                        GUILayout.BeginArea(_SceneNameRects[i]);
                        {
                            //Scene Name Label
                            EditorGUILayout.LabelField(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));
                        }
                        GUILayout.EndArea();

                        //Scene Path Area Created, and Label Drawn
                        GUILayout.BeginArea(_ScenePathRects[i]);
                        {
                            //Scene Path Label
                            EditorGUILayout.LabelField(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));
                        }
                        GUILayout.EndArea();

                        //Some Extra Space added below
                        GUILayout.Space(8f);
                    }
                    EditorGUILayout.EndVertical();
                    #endregion

                    //Draw Action Buttons
                    DrawSceneActionButtons(i, _horizontalCenter);
                }
                EditorGUILayout.EndHorizontal(); //End EachScene Horizontal Block
            } //End SceneList Loop
        }
        EditorGUILayout.EndScrollView(); //End ScrollView
    }

    /// <summary>
    /// Draws the Starred Scene List. In other terms, Draws the List of Favorite Scenes
    /// </summary>
    private void DrawStarredScene() {
        //Creates an Initial Spacing from the Toolbar
        GUILayout.Space(15);

        //Creates a ScrollView
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(position.height - 104 - 37));
        {
            //Loops through the StarToggles that are true. Which is, the scenes that are starred
            for (int i = 0; i < starToggles.Length; i++) {
                //If Not Starred, Skip
                if (!starToggles[i])
                    continue;

                //The main Horizontal Area of Each Elements. It's Rect Component is stored to calculate the Center.
                //Helps for making the Editor Responsive
                var _tempRectHorizontal = EditorGUILayout.BeginHorizontal("Box", GUILayout.MinHeight(46));
                {
                    //Only when the Window is repainted, the Temp Rect Value will be stored for that Element.
                    //Otherwise, Gives wrong value
                    if (Event.current.type == EventType.Repaint) {
                        _HorizontalRects[i] = _tempRectHorizontal;
                    }

                    //Scene Element Background
                    EditorGUI.DrawRect(_HorizontalRects[i], editorSkinColor);

                    //Center position for each element
                    float _horizontalCenter = _HorizontalRects[i].height / 2f;

                    //Creates a small space before drawing the Text Labels
                    GUILayout.Space(3);

                    //Text Design
                    #region TEXT_LABEL_DESIGN
                    //The Width of the Text is defined as per the Window Width
                    float _TextWidth = position.width - 146f;
                    //Vertical Area Created To Draw Two Labels on top of each other
                    EditorGUILayout.BeginVertical();
                    {
                        //Spacing added to keep the Text Labels at Center of Background
                        GUILayout.Space(_horizontalCenter - 4f);
                        GUILayout.Space(-(_SceneNameRects[i].height / 2f + _ScenePathRects[i].height / 2f));

                        //Rect Create. A Space is reserved calculating the Name Length and Available Space.
                        //This is done to make the label Responsive.
                        var _tempRect = GUILayoutUtility.GetRect(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));
                        var _tempRect2 = GUILayoutUtility.GetRect(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));

                        //The Temp value is stored only when the Editor is Repainted. Otherwise gives wrong value
                        if (Event.current.type == EventType.Repaint) {
                            _SceneNameRects[i] = _tempRect;
                            _ScenePathRects[i] = _tempRect2;
                        }

                        //Scene Name Area Created, and Label Drawn
                        GUILayout.BeginArea(_SceneNameRects[i]);
                        {
                            //Scene Name Label
                            EditorGUILayout.LabelField(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));
                        }
                        GUILayout.EndArea();

                        //Scene Path Area Created, and Label Drawn
                        GUILayout.BeginArea(_ScenePathRects[i]);
                        {
                            //Scene Path Label
                            EditorGUILayout.LabelField(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));
                        }
                        GUILayout.EndArea();

                        //Some Extra Space added below
                        GUILayout.Space(8f);
                    }
                    EditorGUILayout.EndVertical();
                    #endregion

                    //Draw Action Buttons
                    DrawSceneActionButtons(i, _horizontalCenter);
                }
                EditorGUILayout.EndHorizontal(); //End EachScene Horizontal Block
            } //End SceneList Loop
        }
        EditorGUILayout.EndScrollView(); //End ScrollView
    }
    #endregion

    #region HELPERS
    /// <summary>
    /// Draws the Action Buttons for Each Scene in the List. This is common for both "All Scenes" and "Starred" Tab.
    /// Changing in this function will cause change in both
    /// </summary>
    /// <param name="i">Holds the index in the List</param>
    /// <param name="bg_center">The Center from the background for that element</param>
    private void DrawSceneActionButtons(int i, float bg_center) {
        //NOTE: In order to Change the buttons, Check the ButtonGroupGUIContent Region. OnEnable the icons are loaded

        #region ALL_BUTTONS
        //Creates a Vertical Group
        EditorGUILayout.BeginVertical();
        {
            //Sets the group to the Center from the Background, So that when the Background Height Increases
            //It will always stay in the Center.
            GUILayout.Space(bg_center - 32f);

            //Button Group 1 - "Open Button" & "Ping Button"
            #region BUTTON_GROUP_1
            //Creates a Horizontal Layout
            EditorGUILayout.BeginHorizontal();
            {
                //Open Button
                if (GUILayout.Button(Open, GUILayout.Width(53f), GUILayout.Height(18))) {
                    if (EditorSceneManager.GetActiveScene().isDirty) {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    }

                    EditorSceneManager.OpenScene(scenePaths[i].text, OpenSceneMode.Single);
                }

                //Ping Button
                if (GUILayout.Button(Ping, GUILayout.Width(53f), GUILayout.Height(18))) {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(scenePaths[i].text);
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(scenePaths[i].text, typeof(SceneAsset)));
                }
            }
            //End of ButtonGroup1 Horizontal Layout
            EditorGUILayout.EndHorizontal();
            #endregion

            //Button Group 2 - "Add Button" & "Delete Button"
            #region BUTTON_GROUP_2
            //Creates a Horizontal Layout
            EditorGUILayout.BeginHorizontal();
            {
                //Add button
                if (GUILayout.Button(Add, GUILayout.Width(53f), GUILayout.Height(18))) {
                    if (EditorSceneManager.GetActiveScene().isDirty) {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    }

                    EditorSceneManager.OpenScene(scenePaths[i].text, OpenSceneMode.Additive);
                }

                //Delete Button
                if (GUILayout.Button(Delete, GUILayout.Width(53f), GUILayout.Height(18))) {
                    if (EditorUtility.DisplayDialog("Delete Selected Scene?", scenePaths[i].text + "\nAre you sure you want to delete the scene? \nYou can't Undo this action", "Yes", "No")) {
                        AssetDatabase.DeleteAsset(scenePaths[i].text);

                        //UpdateDatabase
                        RefreshList();
                    }
                }
            }
            //End of ButtonGroup2 Horizontal Layout
            EditorGUILayout.EndHorizontal();
            #endregion

            //Creates the AddToBuild Button
            if (GUILayout.Button(AddToBuild, GUILayout.Width(109f), GUILayout.Height(18))) {

            }
        }
        EditorGUILayout.EndVertical();
        #endregion
    }

    /// <summary>
    /// Searches the Project File for Scene. Adds them into the Array by breaking down Name and Path
    /// Initializes other arrays based on the Available Scene Count
    /// </summary>
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
    #endregion

    #region INITIALIZATION
    //The Star Toggle Icon for "All Scenes" Tab
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

    //The Scene Name Font Style is Initialized
    private void InitSceneNameStyle() {
        sceneNameStyle = new GUIStyle {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
    }

    //The Scene Path Font Style is Initialized
    private void InitScenePathStyle() {
        scenePathStyle = new GUIStyle {
            fontSize = 10,
            fontStyle = FontStyle.Normal,
            wordWrap = true
        };
    }
    #endregion

    #region DESIGNTEMPLATE
    //A Signature Template Design, for Jisan Haider Joy
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

        //#region ColorSettings
        //editorSkinColor = EditorGUI.ColorField(new Rect(170f, position.height - 22.25f, 60f, 15f), editorSkinColor);
        //editorSkinBGColor = EditorGUI.ColorField(new Rect(240f, position.height - 22.25f, 60f, 15f), editorSkinBGColor);
        //#endregion
    }
    #endregion
}

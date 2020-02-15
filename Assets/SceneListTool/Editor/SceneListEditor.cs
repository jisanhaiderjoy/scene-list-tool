using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditorInternal;

namespace SceneListTool {
    public class SceneListData : ScriptableObject {
        public List<string> FavoriteScenes;
        public List<SceneAsset> BuildScenes;
    }

    public class SceneListEditor : EditorWindow {
        #region TEMPLATE_VARIABLES
        private GUIStyle ToolNameStyle;

        private Rect TemplateBackgroundRect;
        private Rect TitleBackgroundRect;
        private Rect ToolTitleNameRect;
        private Rect TradeMarkBackgroundRect;
        private Rect AuthorRect;
        private Rect VersionRect;

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
        new GUIContent("Build List", "List of scenes added to build, can be managed from here")};
        #endregion

        #region ALLSCENE_VARIABLES
        GUIContent RefreshButton = new GUIContent("Refresh List", "");
        GUIContent[] sceneNames;
        GUIContent[] scenePaths;

        //Star Icon Properties
        private GUIStyle starStyle;
        private bool[] starToggles;

        //SceneName Properties
        private GUIStyle sceneNameStyle;
        Rect[] SceneNameRects;

        //Scene Path Properties
        private GUIStyle scenePathStyle;
        Rect[] ScenePathRects;
        Rect LastFrameRectLastIndex;

        //Scroll View Properties
        private Vector2 scrollPos;
        Rect[] HorizontalRects;
        #endregion

        #region GLOBAL_VARIABLES
        private SceneListData SavedData;
        private string projectPath;
        private Rect windowPosition;
        #endregion

        #region BUTTON_GROUP_GUICONTENT
        GUIContent Open = new GUIContent("Open", "Open Scene Single");
        GUIContent Add = new GUIContent("Add", "Open Scene Additive");
        GUIContent Ping = new GUIContent("Ping", "Select file on the project window");
        GUIContent Delete = new GUIContent("Delete", "Delete the selected scene");
        GUIContent AddToBuild = new GUIContent("Add To Build", "Add to Build list and adjust if needed");
        #endregion

        #region BUILD_SETTINGS
        private ReorderableList reorderableList;
        #endregion

        #region UNITY_CALLBACKS
        /// <summary>
        /// Called by Unity to Open the Window
        /// </summary>
        [MenuItem("Tools/Scene List v2 &#s")]
        static void InitWindow() {
            // Get existing open window or if none, make a new one:
            SceneListEditor window = (SceneListEditor)GetWindow(typeof(SceneListEditor), false, "Scene List Tool", true);
            window.minSize = new Vector2(300, 300);
            window.Show();
        }

        /// <summary>
        /// Called by Unity For Initialization
        /// </summary>
        private void OnEnable() {
            //Cache the Window Size
            windowPosition = position;

            //Init Template Design Variables
            TemplateInit();

            //Load Toolbar icon
            topToolbarNames[0].image = (Texture)EditorGUIUtility.Load("SceneAsset Icon");
            topToolbarNames[1].image = (Texture)EditorGUIUtility.Load("Favorite Icon");
            topToolbarNames[2].image = (Texture)EditorGUIUtility.Load("CustomSorting");

            //Load Title Scene Asset Icon
            toolTitle.image = (Texture)EditorGUIUtility.Load("SceneAsset Icon");

            //Load Refresh Button Icon
            RefreshButton.image = (Texture)EditorGUIUtility.Load("RotateTool");

            //Load Button Asset Icon
            AddToBuild.image = (Texture)EditorGUIUtility.Load("UnityEditor.SceneHierarchyWindow");

            if (starStyle == null)
                InitStarToggleIcon();

            if (sceneNameStyle == null)
                InitSceneNameStyle();

            if (scenePathStyle == null)
                InitScenePathStyle();

            //Gets the Project Path
            projectPath = Application.dataPath;

            //Scriptable Data Loaded
            SavedData = AssetDatabase.LoadAssetAtPath<SceneListData>("Assets/SceneListTool/SceneListData.asset");

            //If Scriptable Data is not found, a new one is created and initialized for use
            if (SavedData == null) {
                SavedData = CreateInstance<SceneListData>();
                AssetDatabase.CreateAsset(SavedData, "Assets/SceneListTool/SceneListData.asset");

                SavedData.FavoriteScenes = new List<string>();

                SavedData.BuildScenes = new List<SceneAsset>();
                //Load Build Scenes into List
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++) {
                    SavedData.BuildScenes.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[i].path));
                }

                EditorUtility.SetDirty(SavedData);
                AssetDatabase.SaveAssets();
            }

            if (reorderableList == null || reorderableList.list == null) {
                Debug.Log("Reorder List Created");

                //Setup Reorderable List for the Scene List
                reorderableList = new ReorderableList(SavedData.BuildScenes, typeof(SceneListData), true, true, true, true);

                //Callbacks for the Reorderable List
                reorderableList.drawHeaderCallback = drawHeaderCallback;
                reorderableList.drawElementCallback = drawElementCallback;
                reorderableList.onAddCallback = onAddCallback;
                reorderableList.onChangedCallback = onChangeCallback;
            }

            EditorBuildSettings.sceneListChanged += SceneListChangeCallback;

            //Load Scenes into Array
            RefreshList();
        }

        private void OnDisable() {
            EditorBuildSettings.sceneListChanged -= SceneListChangeCallback;
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
            //Cache the Window Size
            windowPosition = position;

            #region TEMPLATE_DESIGN_DRAW
            //If Window Size is changed, Update the Template Size Variables
            if (TemplateBackgroundRect.width != windowPosition.width || TemplateBackgroundRect.height != windowPosition.height)
                TemplateSizeUpdate();

            //DrawGUITemplate
            DrawTemplate();
            #endregion

            //Toolbar Selection Design
            #region TOP_TOOLBAR_DESIGN
            //Toolbar Background Rect Design
            EditorGUI.DrawRect(new Rect(0, 47, windowPosition.width, 50), editorSkinColor);

            //ToolBar Y Pos Spacing
            GUILayout.Space(57); //47 + 25 - 15 //BackgroundRect_Y + BackgroundRect_H/2 - ToolBar_H/2

            //For X Post Spacing
            EditorGUILayout.BeginHorizontal();
            //ToolBar X Pos Spacing
            GUILayout.Space(25);
            //ToolBar GUI Create
            int _tempToolBarSelection = GUILayout.Toolbar(topToolbarSelection, topToolbarNames, GUILayout.Width(windowPosition.width - 50f), GUILayout.Height(30));
            //ToolBar X Pos Spacing
            GUILayout.Space(25);
            EditorGUILayout.EndHorizontal();
            #endregion

            #region BODY_DRAW


            //Based on the Toolbar Selection, UI is Drawn
            if (_tempToolBarSelection == 0) {
                //Draws the "All Scenes" List.
                DrawAllScene();

                //Repaints the GUI. Avoids Drawing Lag
                //Checks if Repaint is Required. This is simple Optimization Step to Avoid Unnecessary Repaints
                if (LastFrameRectLastIndex.y != ScenePathRects[ScenePathRects.Length - 1].y) {
                    //Repaints the GUI. Avoids Drawing Lag
                    Repaint();

                    //Stores the LastFrameRect of the element of the ScenePathRect Array
                    LastFrameRectLastIndex = ScenePathRects[ScenePathRects.Length - 1];
                }

            } else if (_tempToolBarSelection == 1) {
                //Draws the "Starred Scene" List.
                DrawStarredScene();

                //Repaints the GUI. Avoids Drawing Lag
                //Checks if Repaint is Required. This is simple Optimization Step to Avoid Unnecessary Repaints
                if (LastFrameRectLastIndex.y != ScenePathRects[SavedData.FavoriteScenes.Count - 1].y) {
                    //Repaints the GUI. Avoids Drawing Lag
                    Repaint();

                    //Stores the LastFrameRect of the element of the ScenePathRect Array
                    LastFrameRectLastIndex = ScenePathRects[SavedData.FavoriteScenes.Count - 1];
                }
            } else if (_tempToolBarSelection == 2) {
                //Draws The "Build List"
                DrawBuildList();
            }

            //Update Toolbar Selection
            if (_tempToolBarSelection != topToolbarSelection) {
                topToolbarSelection = _tempToolBarSelection;

                //A garbage value given to initiate Repaint on next frame
                LastFrameRectLastIndex.y = -100;

                //GUI repainted
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
            if (GUILayout.Button(RefreshButton, GUILayout.Height(20f))) {
                RefreshList();
                Repaint();
            }

            //A Spacing after the Refresh Button
            GUILayout.Space(3f);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(windowPosition.height - 104 - 37 - 23f));
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
                            HorizontalRects[i] = _tempRectHorizontal;
                        }

                        //Scene Element Background
                        EditorGUI.DrawRect(HorizontalRects[i], editorSkinColor);

                        //Center windowPosition for each element
                        float _horizontalCenter = HorizontalRects[i].height / 2f;

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
                            //TempBool taken to get the Updated Value
                            var _tempBool = EditorGUILayout.Toggle(starToggles[i], starStyle, GUILayout.Height(25), GUILayout.Width(25));

                            //If Toggle switched to On/Off
                            if (starToggles[i] != _tempBool) {
                                starToggles[i] = _tempBool;

                                //If Switched On, Scene Path is added
                                if (_tempBool)
                                    SavedData.FavoriteScenes.Add(scenePaths[i].text);
                                else //Else Scene Path Removed
                                    SavedData.FavoriteScenes.Remove(scenePaths[i].text);

                                //Scriptable Object Set Dirty and Saved
                                EditorUtility.SetDirty(SavedData);
                                AssetDatabase.SaveAssets();
                            }
                        }
                        EditorGUILayout.EndVertical();
                        #endregion

                        //Creates a small space before drawing the Text Labels
                        GUILayout.Space(3);
                        //Text Design
                        #region TEXT_LABELS_DESIGN
                        //The Width of the Text is defined as per the Window Width
                        float _TextWidth = windowPosition.width - 180f;
                        //Vertical Area Created To Draw Two Labels on top of each other
                        EditorGUILayout.BeginVertical();
                        {
                            //Spacing added to keep the Text Labels at Center of Background
                            GUILayout.Space(_horizontalCenter - 4f);
                            GUILayout.Space(-(SceneNameRects[i].height / 2f + ScenePathRects[i].height / 2f));

                            //Rect Create. A Space is reserved calculating the Name Length and Available Space.
                            //This is done to make the label Responsive.
                            var _tempRect = GUILayoutUtility.GetRect(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));
                            var _tempRect2 = GUILayoutUtility.GetRect(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));

                            //The Temp value is stored only when the Editor is Repainted. Otherwise gives wrong value
                            if (Event.current.type == EventType.Repaint) {
                                SceneNameRects[i] = _tempRect;
                                ScenePathRects[i] = _tempRect2;
                            }

                            //Scene Name Area Created, and Label Drawn
                            GUILayout.BeginArea(SceneNameRects[i]);
                            {
                                //Scene Name Label
                                EditorGUILayout.LabelField(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));
                            }
                            GUILayout.EndArea();

                            //Scene Path Area Created, and Label Drawn
                            GUILayout.BeginArea(ScenePathRects[i]);
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
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(windowPosition.height - 104 - 37));
            {
                //the ith Rects are Reused, so that Tab Changes will not require more repaints.
                //only jth StarToggles, Scene Name, Path and Buttons are used
                int i = 0;
                //Loops through the StarToggles that are true. Which is, the scenes that are starred
                for (int j = 0; j < starToggles.Length; j++) {
                    //If Not Starred, Skip
                    if (!starToggles[j])
                        continue;

                    //The main Horizontal Area of Each Elements. It's Rect Component is stored to calculate the Center.
                    //Helps for making the Editor Responsive
                    var _tempRectHorizontal = EditorGUILayout.BeginHorizontal("Box", GUILayout.MinHeight(46));
                    {
                        //Only when the Window is repainted, the Temp Rect Value will be stored for that Element.
                        //Otherwise, Gives wrong value
                        if (Event.current.type == EventType.Repaint) {
                            HorizontalRects[i] = _tempRectHorizontal;
                        }

                        //Scene Element Background
                        EditorGUI.DrawRect(HorizontalRects[i], editorSkinColor);

                        //Center windowPosition for each element
                        float _horizontalCenter = HorizontalRects[i].height / 2f;

                        //Creates a small space before drawing the Text Labels
                        GUILayout.Space(3);

                        //Text Design
                        #region TEXT_LABEL_DESIGN
                        //The Width of the Text is defined as per the Window Width
                        float _TextWidth = windowPosition.width - 146f;
                        //Vertical Area Created To Draw Two Labels on top of each other
                        EditorGUILayout.BeginVertical();
                        {
                            //Spacing added to keep the Text Labels at Center of Background
                            GUILayout.Space(_horizontalCenter - 4f);
                            GUILayout.Space(-(SceneNameRects[i].height / 2f + ScenePathRects[i].height / 2f));

                            //Rect Create. A Space is reserved calculating the Name Length and Available Space.
                            //This is done to make the label Responsive.
                            var _tempRect = GUILayoutUtility.GetRect(sceneNames[j], sceneNameStyle, GUILayout.Width(_TextWidth));
                            var _tempRect2 = GUILayoutUtility.GetRect(scenePaths[j], scenePathStyle, GUILayout.Width(_TextWidth));

                            //The Temp value is stored only when the Editor is Repainted. Otherwise gives wrong value
                            if (Event.current.type == EventType.Repaint) {
                                SceneNameRects[i] = _tempRect;
                                ScenePathRects[i] = _tempRect2;
                            }

                            //Scene Name Area Created, and Label Drawn
                            GUILayout.BeginArea(SceneNameRects[i]);
                            {
                                //Scene Name Label
                                EditorGUILayout.LabelField(sceneNames[j], sceneNameStyle, GUILayout.Width(_TextWidth));
                            }
                            GUILayout.EndArea();

                            //Scene Path Area Created, and Label Drawn
                            GUILayout.BeginArea(ScenePathRects[i]);
                            {
                                //Scene Path Label
                                EditorGUILayout.LabelField(scenePaths[j], scenePathStyle, GUILayout.Width(_TextWidth));
                            }
                            GUILayout.EndArea();

                            //Some Extra Space added below
                            GUILayout.Space(8f);
                        }
                        EditorGUILayout.EndVertical();
                        #endregion

                        //Draw Action Buttons
                        DrawSceneActionButtons(j, _horizontalCenter);
                    }
                    EditorGUILayout.EndHorizontal(); //End EachScene Horizontal Block

                    i++;
                } //End SceneList Loop
            }
            EditorGUILayout.EndScrollView(); //End ScrollView
        }

        /// <summary>
        /// Draws the Build List. Handles the Build Settings
        /// </summary>
        private void DrawBuildList() {
            //Creates an Initial Spacing from the Toolbar
            GUILayout.Space(15);

            reorderableList.DoLayoutList();

            GUILayout.Space(5);
            if (GUILayout.Button("Apply To Build Settings")) {
                SetEditorBuildSettingsScenes();
            }

            if (GUILayout.Button("Open Build Settings")) {
                GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"), true);
            }
        }
        #endregion

        #region HELPERS
        /// <summary>
        /// Callback for Drawing the Header On GUI
        /// </summary>
        /// <param name="rect"></param>
        void drawHeaderCallback(Rect rect) {
            EditorGUI.LabelField(rect, "Build Scenes");
        }

        /// <summary>
        /// CallBack for Drawing on GUI
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="isActive"></param>
        /// <param name="isFocused"></param>
        void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused) {
            Rect _buttonRect = rect;
            _buttonRect.width = 30f;

            GUI.enabled = SavedData.BuildScenes[index] != null;
            GUI.Button(_buttonRect, index.ToString());
            //GUI.Label(_buttonRect, index.ToString());

            Rect _objectRect = rect;
            _objectRect.width -= 30f;
            _objectRect.x += 32f;
            GUI.enabled = true;
            var _sceneObj = (SceneAsset)EditorGUI.ObjectField(_objectRect, SavedData.BuildScenes[index], typeof(SceneAsset), false);

            if (_sceneObj != SavedData.BuildScenes[index]) {
                Debug.Log("Scene Field Updated");
                SavedData.BuildScenes[index] = _sceneObj;

                EditorUtility.SetDirty(SavedData);
                AssetDatabase.SaveAssets();
                SetEditorBuildSettingsScenes();
            }
        }

        /// <summary>
        /// Callback when the add button on ReoderList is pressed.
        /// </summary>
        /// <param name="l"></param>
        void onAddCallback(ReorderableList l) {
            Debug.Log("onAddCallback");

            SavedData.BuildScenes.Add(null);
        }

        /// <summary>
        /// Callback when the ReorderList has been Updated. Add/Change/Delete
        /// </summary>
        /// <param name="l"></param>
        void onChangeCallback(ReorderableList l) {
            Debug.Log("onChangeCallback");
            _listChange = true;
            EditorUtility.SetDirty(SavedData);
            AssetDatabase.SaveAssets();
            SetEditorBuildSettingsScenes();
        }
        private bool _listChange = false;
        /// <summary>
        /// Callback when the Scene List from the EditorBuildSettings has been updated. Add/Change/Delete
        /// </summary>
        private void SceneListChangeCallback() {
            if (_listChange) {
                _listChange = false;
                return;
            }

            Debug.Log("SceneListChangeCallback");
            //Clears Old List
            SavedData.BuildScenes.Clear();

            //Load Updated Build Scenes into List
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++) {
                SavedData.BuildScenes.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[i].path));
            }

            EditorUtility.SetDirty(SavedData);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Updates the Build Settings Based on the Current List of Selected Scenes on the Editor
        /// </summary>
        public void SetEditorBuildSettingsScenes() {
            // Find valid Scene paths and make a list of EditorBuildSettingsScene
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            foreach (var sceneAsset in SavedData.BuildScenes) {
                string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                if (!string.IsNullOrEmpty(scenePath))
                    editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            // Set the Build Settings window Scene list
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }

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
                        GetWindow(System.Type.GetType("UnityEditor.ProjectBrowser,UnityEditor"));
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
                    //Creates Scene Asset from path
                    SceneAsset _scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePaths[i].text);

                    //If Scene is not added into Build List, It will be added
                    if (!SavedData.BuildScenes.Contains(_scene)) {
                        //Scene Added to List
                        SavedData.BuildScenes.Add(_scene);

                        //Data Saved
                        EditorUtility.SetDirty(SavedData);
                        AssetDatabase.SaveAssets();

                        //Build Settings Updated
                        SetEditorBuildSettingsScenes();
                    }
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
            //Searches for Scene Assets all over the project
            string[] _searchedPaths = Directory.GetFiles(projectPath, "*.unity", SearchOption.AllDirectories);
            int _sceneCount = _searchedPaths.Length;

            //Initializes all the Arrays
            sceneNames = new GUIContent[_sceneCount];
            scenePaths = new GUIContent[_sceneCount];

            starToggles = new bool[_sceneCount];
            SceneNameRects = new Rect[_sceneCount];
            ScenePathRects = new Rect[_sceneCount];
            LastFrameRectLastIndex = new Rect();
            HorizontalRects = new Rect[_sceneCount];

            //Iterates through all the scenes found from the Project
            for (int i = 0; i < _sceneCount; i++) {
                //The Scene Path Starting from Assets Folder is retrieved
                string _relativePath = _searchedPaths[i].Substring(Application.dataPath.Length - 6);
                //Scene Path Stored
                scenePaths[i] = new GUIContent(_relativePath);
                //Scene name retrieved as Substring from the Path
                sceneNames[i] = new GUIContent(_relativePath.Substring(_relativePath.LastIndexOfAny(new[] { '/', '\\' }) + 1).Replace(".unity", ""));

                //If Current Scene Path is Saved into Scriptable Object, The Bool is Set to True
                //It will keep switch on of Star Toggle for that Scene
                if (SavedData.FavoriteScenes.Contains(_relativePath)) {
                    starToggles[i] = true;
                }
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
            EditorGUI.DrawRect(TemplateBackgroundRect, editorSkinBGColor);

            //Top Design
            #region Top Design
            //float TopRectHeight = 40;
            //Label Background Rects
            EditorGUI.DrawRect(TitleBackgroundRect, editorSkinColor);

            //Tool Name Text
            GUI.Label(ToolTitleNameRect, toolTitle, ToolNameStyle);
            #endregion

            //Bottom Design TradeMark
            #region bottomdesign Trademark
            EditorGUI.DrawRect(TradeMarkBackgroundRect, editorSkinColor);
            GUI.Label(AuthorRect, authorInfo);
            GUI.Label(VersionRect, version);
            #endregion

            //#region ColorSettings
            //editorSkinColor = EditorGUI.ColorField(new Rect(170f, windowPosition.height - 22.25f, 60f, 15f), editorSkinColor);
            //editorSkinBGColor = EditorGUI.ColorField(new Rect(240f, windowPosition.height - 22.25f, 60f, 15f), editorSkinBGColor);
            //#endregion
        }

        /// <summary>
        /// Initializes the Variables Required To Draw The Template Design
        /// </summary>
        private void TemplateInit() {
            TemplateBackgroundRect = new Rect(0, 0, windowPosition.width, windowPosition.height);
            TitleBackgroundRect = new Rect(0, 0, windowPosition.width, 40);
            ToolTitleNameRect = new Rect(0, (40 / 2f - 10f), windowPosition.width, 20f);
            TradeMarkBackgroundRect = new Rect(0, windowPosition.height - 30f, windowPosition.width, 30f);
            AuthorRect = new Rect(10, windowPosition.height - 22.25f, 160f, 15f);
            VersionRect = new Rect(windowPosition.width - 95f, windowPosition.height - 22.25f, windowPosition.width, 15f);

            if (ToolNameStyle == null) {
                ToolNameStyle = new GUIStyle();

                //Tool Name Style
                ToolNameStyle.alignment = TextAnchor.UpperCenter;
                ToolNameStyle.fontStyle = FontStyle.Bold;
                ToolNameStyle.fontSize = 13;
            }
        }

        /// <summary>
        /// Updates Template Size based on Window Size
        /// </summary>
        private void TemplateSizeUpdate() {
            TemplateBackgroundRect.width = windowPosition.width;
            TemplateBackgroundRect.height = windowPosition.height;

            TitleBackgroundRect.width = windowPosition.width;

            ToolTitleNameRect.width = windowPosition.width;

            TradeMarkBackgroundRect.width = windowPosition.width;
            TradeMarkBackgroundRect.y = windowPosition.height - 30f;

            AuthorRect.y = windowPosition.height - 22.25f;

            VersionRect.x = windowPosition.width - 95f;
            VersionRect.y = windowPosition.height - 22.25f;
            VersionRect.width = windowPosition.width;
        }
        #endregion
    }
}
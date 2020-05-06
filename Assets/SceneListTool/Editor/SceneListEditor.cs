using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;
using UnityEditor.VersionControl;

namespace SceneListToolLibrary
{
    public class SceneListEditor : EditorWindow
    {
        #region TEMPLATE_VARIABLES
        private GUIStyle ToolNameStyle;
        private GUIStyle HelpButtonStyle;

        private Rect TemplateBackgroundRect;
        private Rect TitleBackgroundRect;
        private Rect ToolTitleNameRect;
        private Rect HelpRect;
        private Rect TradeMarkBackgroundRect;
        private Rect AuthorRect;
        private Rect VersionRect;

        private Color editorSkinColor = new Color(0.7607844f, 0.7607844f, 0.7607844f, 1f);
        private Color editorSkinBGColor = new Color(0.6352941f, 0.6352941f, 0.6352941f, 1f);

        private GUIContent authorInfo = new GUIContent("Author - Jisan Haider Joy", "Find me at,\nhttps://www.facebook.com/jisanhaiderjoy");
        private GUIContent toolTitle = new GUIContent("Scene List", "Title ToolTip");
        private GUIContent helpTitle = new GUIContent("", "Need Help?");

        private string version = "version - 1.1.0";
        #endregion

        #region TOOLBAR_VARIABLES
        private int topToolbarSelection = 0;
        private GUIContent[] topToolbarNames = {
        new GUIContent("All Scenes", "List of all available Scenes"),
        new GUIContent("Starred", "List of all Starred Scenes"),
        new GUIContent("Build List", "List of scenes added to build, can be managed from here")};
        #endregion

        #region ALLSCENE_VARIABLES
        GUIContent RefreshButton = new GUIContent("Refresh List", "Refresh the list, incase any scene can't be found in the list");
        GUIContent[] sceneNames;
        GUIContent[] scenePaths;

        //Star Icon Properties
        private GUIStyle starStyle;
        private bool[] starToggles;

        //SceneName Properties
        private GUIStyle sceneNameStyle;

        //Scene Path Properties
        private GUIStyle scenePathStyle;

        //Scroll View Properties
        private Vector2 scrollPos;
        #endregion

        #region STARRED_VARIABLES
        private Texture2D UnstarIcon;
        #endregion

        #region SEARCH
        //SearchField for v2019.2.x or Less
        UnityEditor.IMGUI.Controls.SearchField searchField;
        //Search Button String is stored here
        private string AllSearchString = "";
        private string StarredSearchString = "";
        //Search Cancel Button Skin
        private GUIStyle CancelButtonSkin;
        //Search TextBox Skin
        private GUIStyle SearchEmptySkin;
        //Search TextBox Skin
        private GUIStyle SearchBarSkin;
        #endregion

        #region GLOBAL_VARIABLES
        private SceneListData SavedData;

        private Rect windowPosition;

        private string projectPath;
        
        //The Relative Path of the Tool
        private string toolPath;

        private GUIStyle ScrollElementBackground;
        private Texture2D ScrollElementBGTexture;
        private bool isProSkin = false;
        private bool isEditorSkinChanged = false;

        private int UnityVersion = 0;
        private bool IS_2019_OR_NEWER = false;
        private bool IS_2019_3_OR_NEWER = false;
        #endregion

        #region BUTTON_GROUP_GUICONTENT
        GUIContent Open = new GUIContent("Open", "Open Scene Single");
        GUIContent Add = new GUIContent("Add", "Open Scene Additive");
        GUIContent Locate = new GUIContent("Locate", "Select file on the project window");
        GUIContent Delete = new GUIContent("Delete", "Delete the selected scene");
        GUIContent AddToBuild = new GUIContent("Add To Build", "Add to Build list and adjust if needed");
        #endregion

        #region BUILD_SETTINGS
        private ReorderableList reorderableList;
        private bool ReorderableListChanged = false;
        private int lastSelectedIndex = -1;
        #endregion

        #region UNITY_CALLBACKS
        /// <summary>
        /// Called by Unity to Open the Window
        /// </summary>
        [MenuItem("Tools/Headshot Games/Scene List &#s")]
        static void InitWindow()
        {
            // Get existing open window or if none, make a new one:
            SceneListEditor window = (SceneListEditor)GetWindow(typeof(SceneListEditor), false, "Scene List Tool", true);
            window.minSize = new Vector2(305, 305);
            window.Show();
        }

        /// <summary>
        /// Called by Unity For Initialization
        /// </summary>
        private void OnEnable()
        {
            //Searches for the Script Editor Path
            string _scriptPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(this.GetType().Name)[0]);
            //Gets the root path for the Tool 
            toolPath = _scriptPath.Substring(0, _scriptPath.Length - 26);
            
            //Gets Editor Version to Execute Version Specific Codes
#if DEBUG
            Debug.Log(Application.unityVersion);
#endif
            //Version is Calculated to prevent Version dependancies for Specific assets
            UnityVersion = int.Parse(Application.unityVersion.Substring(0, 4));
            if (UnityVersion >= 2019)
            {
                //Means Unity 2019.x.x or Above
                IS_2019_OR_NEWER = true;

                //Checks if version is 2019.3.x or 2019.2.x or Lower
                if (int.Parse(Application.unityVersion.Substring(5, 1)) >= 3)
                {
                    //Means Unity 2019.3.x 
                    IS_2019_3_OR_NEWER = true;
                }
                else
                {
                    //Means 2019.2.x or Lower
                    IS_2019_3_OR_NEWER = false;
                    //Selectively Initialized to reduce Unnecessary Initialization
                    //Search field is initialized
                    if (searchField == null)
                        searchField = new UnityEditor.IMGUI.Controls.SearchField();
                }
            }
            else
            {
                //Means lower than 2019.x.x
                IS_2019_OR_NEWER = false;
                IS_2019_3_OR_NEWER = false;
                //Selectively Initialized to reduce Unnecessary Initialization
                //Search field is initialized
                if (searchField == null)
                    searchField = new UnityEditor.IMGUI.Controls.SearchField();
            }
#if DEBUG
            Debug.Log("Substring Version : " + UnityVersion);
            Debug.Log("IS_2019 : " + IS_2019_OR_NEWER);
#endif

#if DEBUG
            version = "Debug - " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
#else
            version = "version - " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
#endif

            //Checks if there is a change in Skin on Editor
            isEditorSkinChanged = isProSkin != EditorGUIUtility.isProSkin;

#if DEBUG
            if (isEditorSkinChanged)
                Debug.Log("Change in Skin");
#endif

            //Checks If ProSkin
            isProSkin = EditorGUIUtility.isProSkin;
            //Cache the Window Size
            windowPosition = position;

            //Adjusts the color of Tool based on the Editor Skin
            if (isProSkin)
            {
                editorSkinColor = new Color(0.1764706f, 0.1764706f, 0.1764706f, 1f);
                editorSkinBGColor = new Color(0.2196078f, 0.2196078f, 0.2196078f, 1f);
            }
            else
            {
                editorSkinColor = new Color(0.7607844f, 0.7607844f, 0.7607844f, 1f);
                editorSkinBGColor = new Color(0.6352941f, 0.6352941f, 0.6352941f, 1f);
            }

            //Init Template Design Variables
            TemplateInit();

            if (IS_2019_OR_NEWER)
            {
                //Load Toolbar icon based on Pro/Personal Skin
                topToolbarNames[0].image = (Texture)EditorGUIUtility.Load(isProSkin ? "d_SceneAsset Icon" : "SceneAsset Icon");
                topToolbarNames[1].image = (Texture)EditorGUIUtility.Load(isProSkin ? "d_Favorite Icon" : "Favorite Icon");
                topToolbarNames[2].image = (Texture)EditorGUIUtility.Load(isProSkin ? "d_CustomSorting" : "CustomSorting");
            }
            else
            {
                //Load Toolbar icon based on Pro/Personal Skin
                topToolbarNames[0].image = (Texture)EditorGUIUtility.Load("SceneAsset Icon");
                topToolbarNames[1].image = (Texture)EditorGUIUtility.Load("Favorite Icon");
                topToolbarNames[2].image = (Texture)EditorGUIUtility.Load(isProSkin ? "d_CustomSorting" : "CustomSorting");
            }

            //Load Refresh Button Icon based on Pro/Personal Skin
            RefreshButton.image = (Texture)EditorGUIUtility.Load(isProSkin ? "d_RotateTool" : "RotateTool");

            //Load Button Asset Icon based on Pro/Personal Skin
            AddToBuild.image = (Texture)EditorGUIUtility.Load(isProSkin ? "d_UnityEditor.SceneHierarchyWindow" : "UnityEditor.SceneHierarchyWindow");

            //The icon for Starred Tab, to unstar a Scene. Loaded based on the Pro/Personal Skin
            UnstarIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(isProSkin ? "Assets/SceneListTool/Textures/unstar_pro.png" : "Assets/SceneListTool/Textures/unstar.png");

            //Gets the Project Path
            projectPath = Application.dataPath;
            
            // AssetDatabase.FindAssets()

            //Scriptable Data Loaded
            if (!AssetDatabase.IsValidFolder(toolPath + "/Data")) { AssetDatabase.CreateFolder(toolPath,"Data"); }
            SavedData = AssetDatabase.LoadAssetAtPath<SceneListData>(toolPath + "/Data/SceneListData.asset");

            //If Scriptable Data is not found, a new one is created and initialized for use
            if (SavedData == null)
            {
                SavedData = CreateInstance<SceneListData>();
                AssetDatabase.CreateAsset(SavedData, toolPath + "/Data/SceneListData.asset");

                //No Starred Scene. Empty List
                SavedData.FavoriteScenes = new List<string>();
                //No Build Scene List. Empty List
                SavedData.BuildScenes = new List<SceneAsset>();
                //As Empty BuildScene List, Empty Enabled List
                SavedData.BuildScenes_Enabled = new List<bool>();

                //Load Build Scenes into List
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    SavedData.BuildScenes.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[i].path));
                    SavedData.BuildScenes_Enabled.Add(EditorBuildSettings.scenes[i].enabled);
                }

                EditorUtility.SetDirty(SavedData);
                AssetDatabase.SaveAssets();
            }

            if (reorderableList == null || reorderableList.list == null)
            {
                //Setup Reorderable List for the Scene List
                reorderableList = new ReorderableList(SavedData.BuildScenes, typeof(SceneListData), true, true, true, true);

                //Callbacks for the Reorderable List
                reorderableList.drawHeaderCallback = drawHeaderCallback;
                reorderableList.drawElementCallback = drawElementCallback;
                reorderableList.onAddCallback = onAddCallback;
                reorderableList.onRemoveCallback = onRemoveCallback;
                reorderableList.onSelectCallback = onSelectCallback;
                reorderableList.onReorderCallback = onReorderCallback;

            }

            EditorBuildSettings.sceneListChanged += SceneListChangeCallback;

            //Load Scenes into Array
            RefreshList();
        }

        private void OnDisable()
        {
            EditorBuildSettings.sceneListChanged -= SceneListChangeCallback;
        }

        private void OnDestroy()
        {
            DestroyImmediate(ScrollElementBGTexture);
            ScrollElementBackground = null;
            System.GC.Collect();
        }

        /// <summary>
        /// Called by Unity, When the Project is changed in anyway.
        /// Refreshes the Scene List if a Scene is Added/Deleted/Modified
        /// </summary>
        private void OnProjectChange()
        {
            RefreshList();
            Repaint();
        }

        /// <summary>
        /// Called by Unity on every frame of GUI
        /// </summary>
        private void OnGUI()
        {
            //Styles Initializations
            #region StylesInitializations
            //If Editor Skin is Changed while the Editor Window is Open
            //Then all the Styles are updated
            if (isEditorSkinChanged)
            {
                InitStarToggleIcon();
                InitScrollElementBackground();
                InitSceneNameStyle();
                InitScenePathStyle();
                InitToolNameStyle();
                HelpButtonStyle = new GUIStyle("IconButton");
                CancelButtonSkin = new GUIStyle("ToolbarSeachCancelButton");
                SearchBarSkin = new GUIStyle("ToolbarSeachTextField");
                SearchEmptySkin = new GUIStyle("ToolbarSeachCancelButtonEmpty");

                System.GC.Collect();

                //Skin Change Reset
                isEditorSkinChanged = false;
            }

            if (starStyle == null)
                InitStarToggleIcon();

            if (ScrollElementBackground == null)
                InitScrollElementBackground();

            if (sceneNameStyle == null)
                InitSceneNameStyle();

            if (scenePathStyle == null)
                InitScenePathStyle();

            if (ToolNameStyle == null)
                InitToolNameStyle();

            if (HelpButtonStyle == null)
                HelpButtonStyle = new GUIStyle("IconButton");

            //Load Cancel Button Skin
            if (CancelButtonSkin == null)
            {
                CancelButtonSkin = new GUIStyle("ToolbarSeachCancelButton");
                //CancelButtonSkin.normal.
            }

            //Load Search Bar Skin
            if (SearchBarSkin == null)
                SearchBarSkin = new GUIStyle("ToolbarSeachTextField");

            //Load Search Empty Skin
            if (SearchEmptySkin == null)
                SearchEmptySkin = new GUIStyle("ToolbarSeachCancelButtonEmpty");
            #endregion

            //Cache the Window Size
            windowPosition = position;

            //If Window Size is changed, Update the Template Size Variables
            #region TEMPLATE_DESIGN_DRAW
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
            var _tSelection = GUILayout.Toolbar(topToolbarSelection, topToolbarNames, GUILayout.Width(windowPosition.width - 50f), GUILayout.Height(30));

            //If Toolbar menu Selection has been changed, Focus on any GUI from the previous menu will be taken off
            if (_tSelection != topToolbarSelection)
                GUI.FocusControl(null);

            //TopToolbar variable is updated
            topToolbarSelection = _tSelection;

            //ToolBar X Pos Spacing
            GUILayout.Space(25);
            EditorGUILayout.EndHorizontal();
            #endregion

            //Based on the Toolbar Selection, UI is Drawn
            #region BODY_DRAW
            if (topToolbarSelection == 0)
            {
                //Draws the "All Scenes" List.
                DrawAllScene();
            }
            else if (topToolbarSelection == 1)
            {
                //Draws the "Starred Scene" List.
                DrawStarredScene();
            }
            else if (topToolbarSelection == 2)
            {
                //Draws The "Build List"
                DrawBuildList();
            }
            else if (topToolbarSelection == 3)
            {
                //Creates an Initial Spacing from the Toolbar
                GUILayout.Space(15);
                //Draws The "Build List"
                DrawHelpGUI();
            }
            #endregion
        }
        #endregion

        #region MAINCONTENT_DRAW
        /// <summary>
        /// Draws the "All Scenes" List. In other terms, Draws the List of All available Scenes in the Project
        /// </summary>
        private void DrawAllScene()
        {
            //Creates an Initial Spacing from the Toolbar
            GUILayout.Space(15);

            //Refresh List Button
            if (GUILayout.Button(RefreshButton, GUILayout.Height(20f)))
            {
                RefreshList();
                Repaint();
            }

            //Draws Search Bar
            DrawSearchBar(ref AllSearchString);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(windowPosition.height - 188f)); //- 104 - 37 - 23f - 25f
            {
                //Loops through all the scenes found in the Project
                for (int i = 0; i < sceneNames.Length; i++)
                {
                    //If there's a Search Text, In that case each Scene names will be searched with the text
                    if (!sceneNames[i].text.ToLower().Contains(AllSearchString.ToLower()))
                        continue;

                    //The main Horizontal Area of Each Elements. It's Rect Component is stored to calculate the Center.
                    //Helps for making the Editor Responsive
                    EditorGUILayout.BeginHorizontal(ScrollElementBackground, GUILayout.MinHeight(70f));
                    {
                        //The Width of the Text is defined as per the Window Width
                        float _TextWidth = windowPosition.width - 180f;

                        EditorGUILayout.BeginVertical();
                        var _b4 = sceneNameStyle.CalcHeight(sceneNames[i], _TextWidth);
                        var _b5 = scenePathStyle.CalcHeight(scenePaths[i], _TextWidth);

                        float h2 = _b4 + _b5;
                        EditorGUILayout.EndVertical();

                        //Center windowPosition for each element
                        float _horizontalCenter = 60f > h2 ? (30f) : (h2 / 2f);

                        //Creates a small space before drawing the StarToggle
                        GUILayout.Space(3);
                        //Toggle Design
                        #region TOGGLE_DESIGN
                        //Creates Vertical Area to Draw Label
                        EditorGUILayout.BeginVertical();
                        {
                            //Spacing added to Keep the Toggle at Center
                            GUILayout.Space(_horizontalCenter - 12.5f);

                            //Toggle Create with size 25x25
                            //TempBool taken to get the Updated Value
                            var _tempBool = EditorGUILayout.Toggle(starToggles[i], starStyle, GUILayout.Height(25), GUILayout.Width(25));

                            //If Toggle switched to On/Off
                            if (starToggles[i] != _tempBool)
                            {
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
                        //Vertical Area Created To Draw Two Labels on top of each other
                        EditorGUILayout.BeginVertical();
                        {
                            //Spacing added to keep the Text Labels at Center of Background
                            GUILayout.Space(_horizontalCenter - h2 / 2f);

                            //Scene Name Label
                            EditorGUILayout.LabelField(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));

                            //Scene Path Label
                            EditorGUILayout.LabelField(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));
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
        private void DrawStarredScene()
        {
            //Creates an Initial Spacing from the Toolbar
            GUILayout.Space(12f);

            //Draws Search Bar
            DrawSearchBar(ref StarredSearchString);

            //Creates a ScrollView
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(windowPosition.height - 163f)); //-104 - 37 - 29f
            {
                //Loops through the StarToggles that are true. Which is, the scenes that are starred
                for (int i = 0; i < starToggles.Length; i++)
                {
                    //If Not Starred, Skip
                    if (!starToggles[i])
                        continue;

                    //If there's a Search Text, In that case each Scene names will be searched with the text
                    if (!sceneNames[i].text.ToLower().Contains(StarredSearchString.ToLower()))
                        continue;

                    //The main Horizontal Area of Each Elements. It's Rect Component is stored to calculate the Center.
                    //Helps for making the Editor Responsive
                    EditorGUILayout.BeginHorizontal(ScrollElementBackground, GUILayout.MinHeight(70f));
                    {
                        //The Width of the Text is defined as per the Window Width
                        float _TextWidth = windowPosition.width - 180f;

                        EditorGUILayout.BeginVertical();
                        var _b4 = sceneNameStyle.CalcHeight(sceneNames[i], _TextWidth);
                        var _b5 = scenePathStyle.CalcHeight(scenePaths[i], _TextWidth);

                        float h2 = _b4 + _b5;
                        EditorGUILayout.EndVertical();

                        //Center windowPosition for each element
                        float _horizontalCenter = 60f > h2 ? (30f) : (h2 / 2f);

                        //Creates a small space before drawing the StarToggle
                        GUILayout.Space(3);
                        //Toggle Design
                        #region UNSTAR_BUTTON_DESIGN
                        //Creates Vertical Area to Draw Label
                        EditorGUILayout.BeginVertical();
                        {
                            //Spacing added to Keep the Toggle at Center
                            GUILayout.Space(_horizontalCenter - 12.5f);

                            //Button to remove scene from Starred
                            if (GUILayout.Button(UnstarIcon,GUILayout.Height(22f), GUILayout.Width(22f)))
                            {
#if DEBUG
                                Debug.Log(scenePaths[i].text + " Removed");
#endif
                                //Toggle Removed
                                starToggles[i] = false;

                                //Scene Path Removed from Data
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
                        //Vertical Area Created To Draw Two Labels on top of each other
                        EditorGUILayout.BeginVertical();
                        {
                            //Spacing added to keep the Text Labels at Center of Background
                            GUILayout.Space(_horizontalCenter - h2 / 2f);

                            //Scene Name Label
                            EditorGUILayout.LabelField(sceneNames[i], sceneNameStyle, GUILayout.Width(_TextWidth));

                            //Scene Path Label
                            EditorGUILayout.LabelField(scenePaths[i], scenePathStyle, GUILayout.Width(_TextWidth));
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
        /// Draws the Build List. Handles the Build Settings
        /// </summary>
        private void DrawBuildList()
        {
            //Creates an Initial Spacing from the Toolbar
            GUILayout.Space(15);

            //Creates a ScrollView
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(windowPosition.height - 141f)); //-104 - 37
            {
                EditorGUILayout.BeginVertical(ScrollElementBackground);
                {
                    if (GUILayout.Button("Get Current Build Settings"))
                    {
                        //Clear Old Data
                        SavedData.BuildScenes.Clear();
                        SavedData.BuildScenes_Enabled.Clear();

                        //Load Build Scenes into List
                        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                        {
                            SavedData.BuildScenes.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[i].path));
                            SavedData.BuildScenes_Enabled.Add(EditorBuildSettings.scenes[i].enabled);
                        }

                        EditorUtility.SetDirty(SavedData);
                        AssetDatabase.SaveAssets();
                    }

                    GUILayout.Space(5);
                    reorderableList.DoLayoutList();

                    if (GUILayout.Button("Apply To Build Settings"))
                    {
                        SetEditorBuildSettingsScenes();
                    }

                    if (GUILayout.Button("Open Build Settings"))
                    {
                        GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"), true);
                    }
                }
                EditorGUILayout.EndVertical();

                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(ScrollElementBackground);
                {
                    EditorGUILayout.LabelField("Handy Shortcut Buttons", EditorStyles.boldLabel == null ? "Label" : EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Open Project Settings"))
                        {
                            EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
                        }

                        if (GUILayout.Button("Open Lighting Settings"))
                        {
                            EditorApplication.ExecuteMenuItem("Window/Rendering/Lighting Settings");
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (GUILayout.Button("Reset SceneList Data"))
                    {
                        if (EditorUtility.DisplayDialog("Reset Scene List Data?", "Are you sure you want to reset all the Data? \nYou can't Undo this action", "Yes", "No"))
                        {
                            //No Starred Scene. Empty List
                            SavedData.FavoriteScenes.Clear();
                            //No Build Scene List. Empty List
                            SavedData.BuildScenes.Clear();
                            SavedData.BuildScenes_Enabled.Clear();

                            //Saves into Scriptable Asset
                            EditorUtility.SetDirty(SavedData);
                            AssetDatabase.SaveAssets();

                            //Refresh the data for UI
                            RefreshList();
                        }
                    }
                }
                EditorGUILayout.EndVertical();

                GUILayout.Space(5);
                DrawHelpGUI();
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the Help Buttons GUI. Shows the ways of Contacting Dev
        /// </summary>
        private void DrawHelpGUI()
        {
            //VerticalArea Created to Show the Background
            EditorGUILayout.BeginVertical(ScrollElementBackground);
            {
                //Contact Me Label
                GUILayout.Label("Need help? Contact me via:");

                GUILayout.BeginHorizontal();
                {
                    //Contact Through Facebook
                    if (GUILayout.Button("Facebook")) { Facebook(); }

                    //Contact Through Official E-mail
                    if (GUILayout.Button("E-mail")) { Email(); }

                    //Contact Through Official Instagram
                    if (GUILayout.Button("Instagram")) { Instagram(); }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    //Contact Through Website
                    if (GUILayout.Button("Unity Connect")) { Website(); }

                    //Visit Asset Store
                    if (GUILayout.Button("Asset Store")) { VisitAssetStore(); }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region HELPERS
        /// <summary>
        /// Visit Facebook Page
        /// </summary>
        void Facebook()
        {
            Application.OpenURL("https://www.facebook.com/gamesheadshot/");
        }

        /// <summary>
        /// Setup Mail using default mail setup
        /// </summary>
        void Email()
        {
            Application.OpenURL("mailto:headshotgamesstudio@gmail.com?cc=jisanhaider76@gmail.com&subject=Scene%20List%20Tool%20Help%20Contact");
        }

        /// <summary>
        /// Visit Instagram 
        /// </summary>
        void Instagram()
        {
            Application.OpenURL("https://www.instagram.com/gamesheadshot/");
        }

        /// <summary>
        /// Visit Unity Connect Page
        /// </summary>
        void Website()
        {
            Application.OpenURL("https://connect.unity.com/t/headshot-games");
        }

        /// <summary>
        /// Visit Asset Store Page
        /// </summary>
        void VisitAssetStore()
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/46846");
        }

        /// <summary>
        /// Callback for Drawing the Header On GUI
        /// </summary>
        /// <param name="rect"></param>
        void drawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Build Scenes");
        }

        /// <summary>
        /// CallBack for Drawing on GUI
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="isActive"></param>
        /// <param name="isFocused"></param>
        void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            //ToggleRect
            Rect _toggleRect = rect;
            _toggleRect.width = 20f;

            //If Index Element is Null, then Toggle will be disabled
            GUI.enabled = SavedData.BuildScenes[index] != null;
            //If Toggle is Changed
            var _tempToggle = EditorGUI.Toggle(_toggleRect, SavedData.BuildScenes_Enabled[index]);
            if (_tempToggle != SavedData.BuildScenes_Enabled[index])
            {
                //ListChange Flagged, to prevent callback from BuildSettings
                ReorderableListChanged = true;
                //Data Updated
                SavedData.BuildScenes_Enabled[index] = _tempToggle;

                //Data Saved into the Editor
                EditorUtility.SetDirty(SavedData);
                AssetDatabase.SaveAssets();
                //BuildSettings Updated
                SetEditorBuildSettingsScenes();
            }

            //ObjectField Rect Setup
            Rect _objectRect = rect;
            _objectRect.width -= 20f;
            _objectRect.x += 20f;
            //_objectRect.width += 2f;

            //Make Sure Object Field is Enabled
            GUI.enabled = true;
            //Object Field Drawn, and Checked for Change
            var _sceneObj = (SceneAsset)EditorGUI.ObjectField(_objectRect, SavedData.BuildScenes[index], typeof(SceneAsset), false);
            if (_sceneObj != SavedData.BuildScenes[index])
            {
#if DEBUG
                Debug.Log("Scene Field Updated");
#endif

                //ListChange Flagged, to prevent callback from BuildSettings
                ReorderableListChanged = true;

                //If Updated Field is Not Null
                bool _isNotNull = _sceneObj != null;

                //If Updated Scene is Not Null, checks if the Scene is already available
                if (_isNotNull)
                {
                    if (!SavedData.BuildScenes.Contains(_sceneObj))
                    {
                        SavedData.BuildScenes[index] = _sceneObj;
                        SavedData.BuildScenes_Enabled[index] = _isNotNull;
                    }
                }
                else
                {
                    //IF null, List index is set null
                    SavedData.BuildScenes[index] = null;
                    SavedData.BuildScenes_Enabled[index] = _isNotNull;
                }

                //Data Saved into the Editor
                EditorUtility.SetDirty(SavedData);
                AssetDatabase.SaveAssets();
                //BuildSettings Updated
                SetEditorBuildSettingsScenes();
            }
        }

        /// <summary>
        /// Callback when the add button on ReoderList is pressed.
        /// </summary>
        /// <param name="l"></param>
        void onAddCallback(ReorderableList l)
        {
#if DEBUG
            Debug.Log("onAddCallback");
#endif

            //Null SceneAsset Added
            SavedData.BuildScenes.Add(null);
            //By Default, False
            SavedData.BuildScenes_Enabled.Add(false);

            //ListChange Flagged, to prevent callback from BuildSettings
            ReorderableListChanged = true;
            //Data Saved into the Editor
            EditorUtility.SetDirty(SavedData);
            AssetDatabase.SaveAssets();
            //BuildSettings Updated
            SetEditorBuildSettingsScenes();
        }

        /// <summary>
        /// Callback when the Remove button on ReoderList is pressed.
        /// </summary>
        /// <param name="l"></param>
        void onRemoveCallback(ReorderableList l)
        {
#if DEBUG
            Debug.Log("onRemoveCallback");
#endif

            //Scene Removed From Data at Selected Index
            SavedData.BuildScenes.RemoveAt(l.index);
            SavedData.BuildScenes_Enabled.RemoveAt(l.index);

            //ListChange Flagged, to prevent callback from BuildSettings
            ReorderableListChanged = true;
            //Data Saved into the Editor
            EditorUtility.SetDirty(SavedData);
            AssetDatabase.SaveAssets();
            //BuildSettings Updated
            SetEditorBuildSettingsScenes();
        }

        /// <summary>
        /// For Legacy Unity Version, On Any Element Selection store the Index
        /// </summary>
        /// <param name="l"></param>
        void onSelectCallback(ReorderableList l)
        {
#if DEBUG
            Debug.Log("Legacy Select Call");
#endif
            lastSelectedIndex = l.index;
        }

        /// <summary>
        /// For Legacy Unity Version, Update Reordered List
        /// </summary>
        /// <param name="l"></param>
        void onReorderCallback(ReorderableList l)
        {
#if DEBUG
            Debug.Log("Legacy Reorder Call");
            Debug.Log("Last Index : " + lastSelectedIndex + " - New Index : " + l.index);
#endif
            //Update List based on old index and current Index
            onReorderCallbackWithDetails(l, lastSelectedIndex, l.index);
        }

        /// <summary>
        /// Callback when the ReorderList has been Reordered
        /// </summary>
        /// <param name="l"></param>
        void onReorderCallbackWithDetails(ReorderableList l, int oldIndex, int newIndex)
        {
#if DEBUG
            Debug.Log("onReorderCallbackWithDetails");
#endif

            //Reorder Operation, BuildScenes are automatically Updated. Only Enabled States has to be swapped
            //Store oldIndex Value Into Temp
            var _tempSceneEnable = SavedData.BuildScenes_Enabled[oldIndex];

            SavedData.BuildScenes_Enabled.RemoveAt(oldIndex);
            SavedData.BuildScenes_Enabled.Insert(newIndex, _tempSceneEnable);

            //ListChange Flagged, to prevent callback from BuildSetting
            ReorderableListChanged = true;
            //Data Saved into the Editor
            EditorUtility.SetDirty(SavedData);
            AssetDatabase.SaveAssets();
            //BuildSettings Updated
            SetEditorBuildSettingsScenes();
        }

        /// <summary>
        /// Callback when the Scene List from the EditorBuildSettings has been updated. Add/Change/Delete
        /// </summary>
        private void SceneListChangeCallback()
        {
            //If ListChanged, This callback is ignored
            if (ReorderableListChanged)
            {
                ReorderableListChanged = false;
                return;
            }

#if DEBUG
            Debug.Log("SceneListChangeCallback");
#endif
            //Clears Old List
            SavedData.BuildScenes.Clear();
            SavedData.BuildScenes_Enabled.Clear();

            //Load Updated Build Scenes into List
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                SavedData.BuildScenes.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[i].path));
                SavedData.BuildScenes_Enabled.Add(EditorBuildSettings.scenes[i].enabled);
            }

            EditorUtility.SetDirty(SavedData);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Updates the Build Settings Based on the Current List of Selected Scenes on the Editor
        /// </summary>
        public void SetEditorBuildSettingsScenes()
        {
            // Find valid Scene paths and make a list of EditorBuildSettingsScene
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();

            for (int i = 0; i < SavedData.BuildScenes.Count; i++)
            {
                string scenePath = AssetDatabase.GetAssetPath(SavedData.BuildScenes[i]);
                //Null Scenes are avoided
                if (!string.IsNullOrEmpty(scenePath))
                    editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, SavedData.BuildScenes_Enabled[i]));
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
        private void DrawSceneActionButtons(int i, float bg_center)
        {
            //NOTE: In order to Change the buttons, Check the ButtonGroupGUIContent Region. OnEnable the icons are loaded

            #region ALL_BUTTONS
            //Creates a Vertical Group
            EditorGUILayout.BeginVertical();
            {
                //Sets the group to the Center from the Background, So that when the Background Height Increases
                //It will always stay in the Center.
                GUILayout.Space(bg_center - 29f);

                //Button Group 1 - "Open Button" & "Locate Button"
                #region BUTTON_GROUP_1
                //Creates a Horizontal Layout
                EditorGUILayout.BeginHorizontal();
                {
                    //Open Button
                    if (GUILayout.Button(Open, GUILayout.Width(51f), GUILayout.Height(18)))
                    {
                        //Save Diaglos box shown for Dirty Scene
                        if (EditorSceneManager.GetActiveScene().isDirty)
                        {
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        }

                        //Selected Scene Opened
                        EditorSceneManager.OpenScene(scenePaths[i].text, OpenSceneMode.Single);
                    }

                    //Locate Button
                    if (GUILayout.Button(Locate, GUILayout.Width(51f), GUILayout.Height(18)))
                    {
                        //Project Browser Window Showed if not Shown
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
                    if (GUILayout.Button(Add, GUILayout.Width(51f), GUILayout.Height(18)))
                    {
                        //Save Diaglos box shown for Dirty Scene
                        if (EditorSceneManager.GetActiveScene().isDirty)
                        {
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        }

                        EditorSceneManager.OpenScene(scenePaths[i].text, OpenSceneMode.Additive);
                    }

                    //Delete Button
                    if (GUILayout.Button(Delete, GUILayout.Width(51f), GUILayout.Height(18)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Selected Scene?", scenePaths[i].text + "\nAre you sure you want to delete the scene? \nYou can't Undo this action", "Yes", "No"))
                        {
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
                if (GUILayout.Button(AddToBuild, GUILayout.Width(105.5f), GUILayout.Height(18)))
                {
                    //Creates Scene Asset from path
                    SceneAsset _scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePaths[i].text);

                    //If Scene is not added into Build List, It will be added
                    if (!SavedData.BuildScenes.Contains(_scene))
                    {
                        //Scene Added to List
                        SavedData.BuildScenes.Add(_scene);
                        SavedData.BuildScenes_Enabled.Add(true);

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
        private void RefreshList()
        {
            //Searches for Scene Assets all over the project
            string[] _searchedPaths = Directory.GetFiles(projectPath, "*.unity", SearchOption.AllDirectories);
            int _sceneCount = _searchedPaths.Length;

            //Initializes all the Arrays
            sceneNames = new GUIContent[_sceneCount];
            scenePaths = new GUIContent[_sceneCount];

            starToggles = new bool[_sceneCount];

            //Iterates through all the scenes found from the Project
            for (int i = 0; i < _sceneCount; i++)
            {
                //The Scene Path Starting from Assets Folder is retrieved
                string _relativePath = _searchedPaths[i].Substring(Application.dataPath.Length - 6);
                //Scene Path Stored
                scenePaths[i] = new GUIContent(_relativePath);
                //Scene name retrieved as Substring from the Path
                sceneNames[i] = new GUIContent(_relativePath.Substring(_relativePath.LastIndexOfAny(new[] { '/', '\\' }) + 1).Replace(".unity", ""));

                //If Current Scene Path is Saved into Scriptable Object, The Bool is Set to True
                //It will keep switch on of Star Toggle for that Scene
                if (SavedData.FavoriteScenes.Contains(_relativePath))
                {
                    starToggles[i] = true;
                }
            }
        }

        /// <summary>
        /// Draws the Search Bar Whenever Called. Stores the Search Input into var searchString
        /// Due to a Bug from 2019.3.x, Search Bar style is not properly showing. That's why Version check is inserted
        /// </summary>
        private void DrawSearchBar(ref string SearchString)
        {
            //IS_2019_3_OR_NEWER will be true only for any version with 2019.3.x
            //For this version, Drawing is done manually
            if (IS_2019_3_OR_NEWER)
            {
                //Spacing After Refresh Button or Toolbar Design
                GUILayout.Space(3f);
                //Begins a horizontalScope
                EditorGUILayout.BeginHorizontal();
                //To keep proper Alignment, it is slightly moved to right
                GUILayout.Space(4f);
                //Search String is Stored
                SearchString = EditorGUILayout.TextField(SearchString, SearchBarSkin);

                //If there's any input in the SearchBar, Search Cancel button will be drawn
                if (SearchString.Length > 0)
                {
                    //The Cancel Button is Drawn
                    if (GUILayout.Button("", CancelButtonSkin))
                    {
                        //String is Cleared
                        SearchString = "";
                        //Remove focus if cleared
                        GUI.FocusControl(null);
                    }
                }

                GUILayout.Space(1f);
                EditorGUILayout.EndHorizontal();

                //Spacing for the Scene ScrollView List
                GUILayout.Space(3f);
            }
            else //IF versions is 2019.2.x or LESS
            {
                //Spacing After Refresh Button or Toolbar Design
                GUILayout.Space(3f);

                //Begins a horizontalScope
                EditorGUILayout.BeginHorizontal();
                //To keep proper Alignment, it is slightly moved to right
                GUILayout.Space(5f);

                //Search String is Stored
                SearchString = searchField.OnToolbarGUI(SearchString, GUILayout.ExpandWidth(true), GUILayout.Height(10f));

                //To keep proper Alignment, it is slightly moved to right
                GUILayout.Space(5f);

                EditorGUILayout.EndHorizontal();

                //Spacing for the Scene ScrollView List
                GUILayout.Space(10f);
            }
        }
        #endregion

        #region INITIALIZATION
        //The Background for Each Scene List Element
        private void InitScrollElementBackground()
        {
#if DEBUG
            Debug.Log("Init BG Color");
#endif

            //Box Template Taken From GUIStyle
            ScrollElementBackground = new GUIStyle();
            ScrollElementBackground.border = new RectOffset(6, 6, 6, 6);
            ScrollElementBackground.margin = new RectOffset(4, 4, 4, 4);
            ScrollElementBackground.padding = new RectOffset(4, 4, 4, 4);

            //New Texture is Created
            ScrollElementBGTexture = new Texture2D(64, 64);

            //Pixels are Set as per the Color inside the Array
            Color[] ColorBlock = ScrollElementBGTexture.GetPixels();
            for (int i = 0; i < ColorBlock.Length; i++)
            {
                ColorBlock[i] = editorSkinColor;
            }

            //Pixels are Applied into Texture
            ScrollElementBGTexture.SetPixels(ColorBlock);
            ScrollElementBGTexture.Apply();

            //Background Texture added into the Background Style
            ScrollElementBackground.normal.background = ScrollElementBGTexture;

            //Instructs the Editor to Not Consider it as an Asset of the Scene and Also not to UnloadIt.
            //WARNING:Manual Unloading Required. Done at OnDestroy
            ScrollElementBGTexture.hideFlags = HideFlags.DontSave;
        }

        //The Star Toggle Icon for "All Scenes" Tab
        private void InitStarToggleIcon()
        {
            starStyle = new GUIStyle();
            
            //Inactive Star Icon
            //Checks If ProSkin
            //Adjusts the color of Tool based on the Editor Skin
            if (isProSkin) {
                //Path for the Inactive Star Texture for Pro Editor
                string _inactivePath = toolPath + "/Textures/star_inactive_pro.png";
                starStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>(_inactivePath);
                starStyle.hover.background = AssetDatabase.LoadAssetAtPath<Texture2D>(_inactivePath);
                starStyle.active.background = AssetDatabase.LoadAssetAtPath<Texture2D>(_inactivePath);
            }
            else
            {
                //Path for the Inactive Star Texture
                string _inactivePath = toolPath + "/Textures/star_inactive.png";
                starStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>(_inactivePath);
                starStyle.hover.background = AssetDatabase.LoadAssetAtPath<Texture2D>(_inactivePath);
                starStyle.active.background = AssetDatabase.LoadAssetAtPath<Texture2D>(_inactivePath);
            }

            //Path for the active Star Texture
            string _activePath = toolPath + "/Textures/star_active.png";
            //Active Star Icon
            starStyle.onNormal.background = AssetDatabase.LoadAssetAtPath<Texture2D>(_activePath);
            starStyle.onHover.background = AssetDatabase.LoadAssetAtPath<Texture2D>(_activePath);
            starStyle.onActive.background = AssetDatabase.LoadAssetAtPath<Texture2D>(_activePath);
        }

        //The Scene Name Font Style is Initialized
        private void InitSceneNameStyle()
        {
            if (EditorStyles.boldLabel == null)
                sceneNameStyle = new GUIStyle("Label");
            else
                sceneNameStyle = new GUIStyle(EditorStyles.boldLabel);

            sceneNameStyle.margin = new RectOffset(0, 0, 0, 0);
            sceneNameStyle.fontSize = 14;
            sceneNameStyle.fontStyle = FontStyle.Bold;
            sceneNameStyle.wordWrap = true;
        }

        //The Scene Path Font Style is Initialized
        private void InitScenePathStyle()
        {
            if (EditorStyles.miniLabel == null)
                scenePathStyle = new GUIStyle("Label");
            else
                scenePathStyle = new GUIStyle(EditorStyles.miniLabel);

            scenePathStyle.margin = new RectOffset(0, 0, 0, 0);
            scenePathStyle.fontSize = 10;
            scenePathStyle.fontStyle = FontStyle.Normal;
            scenePathStyle.wordWrap = true;
        }
        #endregion

        #region DESIGNTEMPLATE
        //A Signature Template Design, for Jisan Haider Joy
        private void DrawTemplate()
        {
            //Background Color
            if (!isProSkin)
            {
                EditorGUI.DrawRect(TemplateBackgroundRect, editorSkinBGColor);
            }

            //Top Design
            #region Top Design
            //Label Background Rects
            EditorGUI.DrawRect(TitleBackgroundRect, editorSkinColor);

            //Tool Name Text
            GUI.Label(ToolTitleNameRect, toolTitle, ToolNameStyle);

            //Help Button on Top
            if (GUI.Button(HelpRect, helpTitle, HelpButtonStyle))
            {
                topToolbarSelection = 3;
            }
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
        private void TemplateInit()
        {
            //Main Background Rect
            TemplateBackgroundRect = new Rect(0, 0, windowPosition.width, windowPosition.height);

            //Title Name Background Rect
            TitleBackgroundRect = new Rect(0, 0, windowPosition.width, 40);
            //Title Name Rect
            ToolTitleNameRect = new Rect((windowPosition.width / 2f) - 161.5f, (40 / 2f - 10f), 323f, 20f);

            if (IS_2019_OR_NEWER)
            {
                //Load Title Scene Asset Icon based on Pro/Personal Skin
                toolTitle.image = (Texture)EditorGUIUtility.Load(isProSkin ? "d_SceneAsset Icon" : "SceneAsset Icon");
                //Help Button Icon based on Pro/Personal Skin
                helpTitle.image = (Texture)EditorGUIUtility.Load(isProSkin ? "d__Help@2x" : "_Help@2x");
            }
            else
            {
                //Load Title Scene Asset Icon based on Pro/Personal Skin
                toolTitle.image = (Texture)EditorGUIUtility.Load("SceneAsset Icon");
                //Help Button Icon based on Pro/Personal Skin
                helpTitle.image = (Texture)EditorGUIUtility.Load("_Help");
            }

            //Help Button Rect
            HelpRect = new Rect(windowPosition.width - 25f, (40 / 2f - 8f), 32f, 32f);

            //Bottom Trademark Background Rect
            TradeMarkBackgroundRect = new Rect(0, windowPosition.height - 30f, windowPosition.width, 30f);
            //Bottom Trademark Author Label Rect
            AuthorRect = new Rect(10, windowPosition.height - 22.25f, 160f, 15f);
            //Bottom Trademark Version Label Rect
            VersionRect = new Rect(windowPosition.width - 100f, windowPosition.height - 22.25f, windowPosition.width, 15f);
        }

        /// <summary>
        /// Initialize ToolName Label Style
        /// </summary>
        private void InitToolNameStyle()
        {
            if (EditorStyles.label == null)
                ToolNameStyle = new GUIStyle("Label");
            else
                ToolNameStyle = new GUIStyle(EditorStyles.label);

            ToolNameStyle.margin = new RectOffset(3, 3, 2, 2);
            ToolNameStyle.padding = new RectOffset(1, 1, 0, 0);
            ToolNameStyle.alignment = TextAnchor.UpperCenter;
            ToolNameStyle.fontStyle = FontStyle.Bold;
            ToolNameStyle.fontSize = 13;

        }

        /// <summary>
        /// Updates Template Size based on Window Size
        /// </summary>
        private void TemplateSizeUpdate()
        {
            TemplateBackgroundRect.width = windowPosition.width;
            TemplateBackgroundRect.height = windowPosition.height;

            TitleBackgroundRect.width = windowPosition.width;

            //ToolTitleNameRect.width = windowPosition.width;
            ToolTitleNameRect.x = (windowPosition.width / 2f) - 161.5f;
            HelpRect.x = windowPosition.width - 25f;

            TradeMarkBackgroundRect.width = windowPosition.width;
            TradeMarkBackgroundRect.y = windowPosition.height - 30f;

            AuthorRect.y = windowPosition.height - 22.25f;

            VersionRect.x = windowPosition.width - 100f;
            VersionRect.y = windowPosition.height - 22.25f;
            VersionRect.width = windowPosition.width;
        }
        #endregion
    }
}
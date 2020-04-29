using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SceneListToolLibrary {
    public class SceneListData : ScriptableObject {
        public List<string> FavoriteScenes;
        public List<SceneAsset> BuildScenes;
        public List<bool> BuildScenes_Enabled;
    }
}
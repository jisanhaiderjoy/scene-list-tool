using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Test : MonoBehaviour {
    [Button("Open Temporary Cache Path")]
    public void TestButton() {
        UnityEditor.EditorUtility.RevealInFinder(Application.temporaryCachePath);
    }


    [Button("Open Persistant Path")]
    public void PersistantPathOpen() {
        UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);
    }

    [Button("Delete SceneListInit EditorPrefs")]
    public void ClearEditorPrefs()
    {
        UnityEditor.EditorPrefs.DeleteKey("SceneListInit");
        Debug.Log("Cleared");
    }

    [Button("Get SceneListInit EditorPrefs")]
    public void GetEditorPrefs()
    {
        Debug.Log(UnityEditor.EditorPrefs.GetInt("SceneListInit"));
    }
}

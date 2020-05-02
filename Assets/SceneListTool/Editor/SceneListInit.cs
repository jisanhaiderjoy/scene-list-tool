using UnityEditor;

namespace SceneListToolLibrary
{
	[InitializeOnLoad]
	public static class SceneListInit
	{
		static SceneListInit()
		{
			if (!EditorPrefs.HasKey("SceneListInit"))
			{
				EditorPrefs.SetInt("SceneListInit", 1);
				EditorUtility.DisplayDialog("Greetings from Headshot Games!", "Thank you for purchasing Scene List Tool." +
					"\r\rIf you liked the tool, don't forget to give a review. It will help me to make it better!\r\rYou can contact me at headshotgamesstudio@gmail.com for any support.", "Got it!");
			}
		}
	}
}
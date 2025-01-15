using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "NewStorylineStash", menuName = "StorylineStash", order = 1)]
public class StorylineStash : SerializedScriptableObject
{
    public Dictionary<string, StorylineData> packedStoryline;
}

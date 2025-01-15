using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Localization;
using JetBrains.Annotations;

[System.Serializable]
public class ObjectiveUnit
{
    [LabelText("목표 이름")] public LocalizedString objectiveText;
    [LabelText("(선택) 시작 시 시퀀스")] public SequenceBundleAsset sequenceOnStart;
    [LabelText("(선택) 완료 시 시퀀스")] public SequenceBundleAsset sequenceOnFinished;
    [LabelText("목적지 오브젝트 이름")] public string destinationTransformName;
}


[CreateAssetMenu(fileName = "NewStorylineData", menuName = "새 스토리라인 에셋 추가", order = 1)]
public class StorylineData : SerializedScriptableObject
{
    [SerializeField, LabelText("퀘스트 이름")] private LocalizedString questNameText;
    public LocalizedString QuestNameText {  get { return questNameText; } }
    [SerializeField, LabelText("퀘스트 목표 텍스트들")] private ObjectiveUnit[] objectives;
    public ObjectiveUnit[] Objectives { get { return objectives; } }
    [SerializeField] private string nextStorylineKey;
}

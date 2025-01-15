using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceStarter : MonoBehaviour
{
    [SerializeField] private SequenceBundleAsset SequenceToStart;
    [SerializeField] private bool disableAfterInvoked = true;
    [SerializeField] private bool autoStart = false;
    [SerializeField] private bool skipOnEditor = true;
    [SerializeField] private SequenceBundleAsset skipSequence;

    bool invoked = false;

    private void Start()
    {
        if(autoStart)
        {
            StartSequence();
        }
    }

    public void StartSequence()
    {
        if (disableAfterInvoked)
        {
            if (invoked) return;
        }

        if (!SequenceInvoker.IsInstanceValid) return;

#if UNITY_EDITOR
        if(skipOnEditor)
            SequenceInvoker.Instance.StartSequence(skipSequence.SequenceBundles);
        else
            SequenceInvoker.Instance.StartSequence(SequenceToStart.SequenceBundles);
#else
        SequenceInvoker.Instance.StartSequence(SequenceToStart.SequenceBundles);
#endif
        invoked = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LukaBlinkEye : MonoBehaviour
{
    //================================================
    //
    // 캐릭터의 자연스러운 눈 깜박이기를 위해 만들어진 클래스 입니다.
    // 모델링의 Blendshape를 사용합니다.
    // 
    //================================================

    [SerializeField] private float blinkInterval = 4.0f;        // 눈을 깜박이는데 지나야 하는 평균적인 시간입니다. (랜덤)
    [SerializeField] private float blinkTime = 0.2f;            // 눈을 깜박이는 시간입니다.
    [SerializeField] private float intervalNoise = 2.5f;        // 눈을 깜박이게 하는 시간의 랜덤값에 의한 오차값입니다.
    [SerializeField] private int blinkBlendshapeIndex = 0;      // 모델링에서 눈을 깜박이는 값에 해당하는 blendshape를 이곳에 입력해야합니다.
    [SerializeField] private AnimationCurve blinkCurve;         // 눈을 어떻게 깜박일지 정하는 애니메이션 커브입니다.

    [SerializeField] private SkinnedMeshRenderer playerMesh;

    float nextblink = 0.0f;

    private void OnEnable()
    {
        nextblink = 0.0f;
        StartCoroutine(Cor_BlinkSequence());
    }

    IEnumerator Cor_BlinkSequence()
    {
        while (true)
        {
            nextblink = blinkInterval + Random.Range(-intervalNoise, intervalNoise);

            yield return new WaitForSeconds(nextblink);

            for (float t = 0; t < blinkTime; t += Time.deltaTime)
            {
                playerMesh.SetBlendShapeWeight(blinkBlendshapeIndex, blinkCurve.Evaluate(t / blinkTime)*100f);
                yield return null;
            }

            if (Random.Range(0, 2) == 0)
            {
                for (float t = 0; t < blinkTime; t += Time.deltaTime)
                {
                    playerMesh.SetBlendShapeWeight(blinkBlendshapeIndex, blinkCurve.Evaluate(t / blinkTime) * 100f);
                    yield return null;
                }
            }
            playerMesh.SetBlendShapeWeight(blinkBlendshapeIndex,0f);
        }
    }
}

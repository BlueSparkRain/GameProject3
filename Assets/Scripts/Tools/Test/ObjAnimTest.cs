using Core;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ObjAnimTest : MonoBehaviour
{
    [Header("����Ŀ��")]
    public GameObject nonSOTarget; // ����һ����������
    public Ease nonSOEase = Ease.OutBack;
    [Tooltip("���°ڶ�����")]
    public float swingDistance = 1f;
    [Tooltip("���ΰڶ�ʱ��")]
    public float swingDuration = 1f;
    MagicAnimationManager animManager;

    CoroutineManager coroutineManager;

    private void Start()
    {
        // ��ֵУ��
        if (nonSOTarget == null)
        {
            Debug.LogError("������������壡");
            return;
        }
        animManager = GameRoot.GetManager<MagicAnimationManager>();
        coroutineManager = GameRoot.GetManager<CoroutineManager>();
        //�ӳ�
        //coroutineManager.StartDelayedCoroutine(5,PlaySwingAnim());
        //�ظ�
        //coroutineManager.StartRepeatingCoroutine(1, 4, PlaySwingAnim,this);
        //coroutineManager.StartCoroutine(PlaySwingAnim());

    }

    IEnumerator PlaySwingAnim()
    {
        //�ƶ���������
        var swingParams = new AnimParams
        {
            Duration = swingDuration,
            Ease = nonSOEase,
            LoopMode = AnimationLoopType.Yoyo,
            LoopCount = 2,
            Interruptible = true
        };
        yield return animManager.PlayAnimation(
            MagicAnimationManager.GetAnimID(E_TweenType.Swing_Box),
            nonSOTarget.transform,
            (p) => nonSOTarget.transform.DOLocalMoveY(nonSOTarget.transform.localPosition.y + swingDistance, p.Duration)
            .SetRelative(false),
            swingParams
            );
        DebugManager.Log(EDebugCategory.General, "gangangan");

    }

    // ��ѡ������ֹͣ���ж������ɰ�UI��ť��
    public void StopAllAnims()
    {
        animManager.InterruptAnimation(MagicAnimationManager.GetAnimID(E_TweenType.Swing_Box));
        DebugManager.Log(EDebugCategory.General, "��ֹͣ���аڶ�����");
    }

    void OnDestroy()
    {
        // �Զ���������
        StopAllAnims();
    }
}
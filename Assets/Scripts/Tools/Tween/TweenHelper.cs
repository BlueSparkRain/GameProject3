using System;
using System.Collections;
using UnityEngine;

public static class TweenHelper
{
    public static IEnumerator MakeLerp(float startValue, float targetValue, float lerpTime,
    Action<float> updateX, Action<float> updateY = null, Action<float> updateZ = null)
    {
        float elapsedTime = 0f;
        while (elapsedTime < lerpTime)
        {
            float t = elapsedTime / lerpTime;
            float currentValue = Mathf.Lerp(startValue, targetValue, t);

            updateX?.Invoke(currentValue);
            updateY?.Invoke(currentValue);
            updateZ?.Invoke(currentValue);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null; // 等待下一帧
        }
        updateX?.Invoke(targetValue);
        updateY?.Invoke(targetValue);
        updateZ?.Invoke(targetValue);
    }
    public static IEnumerator MakeLerp(Vector3 startPosition, Vector3 targetPosition, float lerpTime,
    Action<Vector3> updatePosition, AnimationCurve animCurve = null)
    {

        float elapsedTime = 0f;
        Vector3 currentPosition = Vector3.zero;

        while (elapsedTime < lerpTime)
        {
            if (animCurve == null)
            {
                float t = elapsedTime / lerpTime; // 计算插值因子
                currentPosition = Vector3.Lerp(startPosition, targetPosition, t);
            }
            //if (updatePosition == null)
            //    yield break;
            else
            {
                currentPosition = Vector3.Lerp(startPosition, targetPosition, animCurve.Evaluate(elapsedTime / lerpTime));
            }

            updatePosition?.Invoke(currentPosition); // 更新位置
            elapsedTime += Time.unscaledDeltaTime;
            yield return null; // 等待下一帧
        }

        updatePosition?.Invoke(targetPosition); // 确保最终位置更新到目标位置
    }

}

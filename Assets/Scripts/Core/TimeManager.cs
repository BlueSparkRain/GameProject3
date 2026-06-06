using DG.Tweening;
using UnityEngine;

namespace Core
{
    public class TimeManager : MonoGlobalManager
    {
        Tweener _scaleTween;

        public override void MgrUpdate(float deltaTime) { }

        /// <summary>
        /// 平滑过渡 Time.timeScale 到目标值，过渡本身不受 timeScale 影响
        /// </summary>
        /// <param name="target">目标时间流速 (0~1)</param>
        /// <param name="duration">过渡时长(现实秒)</param>
        public void SetTimeScale(float target, float duration)
        {
            _scaleTween?.Kill();

            float from = Time.timeScale;
            _scaleTween = DOTween.To(
                () => from,
                v => Time.timeScale = v,
                target,
                duration
            ).SetUpdate(true).SetAutoKill(true);
        }

        /// <summary>立即设死，不等过渡</summary>
        public void SetTimeScaleImmediate(float value)
        {
            _scaleTween?.Kill();
            Time.timeScale = value;
        }

        protected override void MgrOnDispose()
        {
            _scaleTween?.Kill();
            Time.timeScale = 1f;
        }
    }
}

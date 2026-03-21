using Core;
using Core.Interfaces;
using UnityEngine;

public class MultiBgmTest : MonoBehaviour
{
    AudioManager auManager;
    public AudioClip bgmMain;    // 主BGM
    public AudioClip bgmAmbient; // 环境BGM(与主BGM共存)

    private void Start()
    {
        auManager = GameRoot.GetManager<AudioManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) {
            auManager.PlayBGM(bgmAmbient, "Ambient");
        }

        if (Input.GetKeyDown(KeyCode.N)) {
            auManager.PlayBGM(bgmMain, "Main");

        }
    }

    private void PlayAmbientBGM()
    {
        // 不同轨道名 → 独立播放、同时共存
        auManager.PlayBGM(bgmAmbient, "Ambient");
    }

    // 单独停止主BGM，环境BGM继续播放
    public void StopMainBGM() => auManager.StopBGM("Main");

    // 单独停止环境BGM，主BGM继续播放
    public void StopAmbientBGM() => auManager.StopBGM("Ambient");

    // 停止所有BGM
    public void StopAll() => auManager.StopAllBGM();
}
using UnityEngine;

/// <summary>
/// 背景音乐控制器，挂在静态画布上
/// 每次页面切换时被调用，根据当前页面决定播还是停
/// </summary>
public class BgmController : MonoBehaviour
{
    // 所有二级目录面板，任意一个显示时播放BGM
    public GameObject[] secondDirectoryPanels;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 由其他导航脚本在切换页面后调用
    /// 规则：在二级目录 → 播放音乐，其他页面 → 停止音乐
    /// </summary>

    public void CheckBgm()
    {
        bool isAtSecondLevel = false;
        if (secondDirectoryPanels != null)
        {
            foreach (var panel in secondDirectoryPanels)
            {
                if (panel != null && panel.activeSelf)
                {
                    isAtSecondLevel = true;
                    break;
                }
            }
        }

        if (isAtSecondLevel)
        {
            if (audioSource != null && !audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource != null && audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}

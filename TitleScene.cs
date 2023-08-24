using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    void Start()
    {

    }

    // --------------------------------------------------------
    /// <summary>
    /// 画面タップコールバック.
    /// </summary>
    // --------------------------------------------------------
    public void OnScreenTap()
    {
        SceneManager.LoadScene("MainScene");
    }
}
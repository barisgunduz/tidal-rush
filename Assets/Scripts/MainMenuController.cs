using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button playButton;

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
    }

    public void OnPlayClicked()
    {
        SceneManager.LoadScene("MainGame");
    }
}

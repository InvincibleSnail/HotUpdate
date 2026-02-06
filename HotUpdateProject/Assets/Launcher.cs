using UnityEngine;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;

public class Launcher : MonoBehaviour
{
    [SerializeField] private Button simple;
    [SerializeField] private Button addressable;
    [SerializeField] private Button xlua;
    [SerializeField] private Button hybridCLR;
    [SerializeField] private Button YooAsset;

    void Start()
    {
        var list = new[] { simple, addressable, xlua, hybridCLR, YooAsset };
        foreach (var button in list)
        {
            button.onClick.AddListener(() =>
            {
                string buttonText = button.gameObject.name;
                string scenePath = $"{buttonText}/Scenes/Entry";
                SceneManager.LoadScene(scenePath);
            });
        }
    }
}
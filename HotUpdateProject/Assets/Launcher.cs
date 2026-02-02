using UnityEngine;
using Button = UnityEngine.UI.Button;

public class Launcher : MonoBehaviour
{
    [SerializeField] private Button simple;
    [SerializeField] private Button addressable;
    [SerializeField] private Button xlua;
    [SerializeField] private Button hybridCLR;
    
    void Start()
    {
        var list = new[] { simple, addressable, xlua, hybridCLR };
        foreach (var button in list)
        {
            button.onClick.AddListener(() =>
            {
                Application.OpenURL(button.name);
            });
        }
    }
}

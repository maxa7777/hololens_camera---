using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class ObjectManager : MonoBehaviour, IInputClickHandler
{
    public GameObject obj1;
    public GameObject obj2;
    private bool toggle = true;

    private void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        var obj = toggle ? obj1 : obj2;
        toggle = !toggle;

        var pos = Camera.main.transform.position;
        var forword = Camera.main.transform.forward;

        Instantiate(obj, pos + forword, new Quaternion());
    }
}
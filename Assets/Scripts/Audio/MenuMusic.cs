using UnityEngine;
public class MenuMusic : MonoBehaviour
{
    static MenuMusic instance;
    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }
}
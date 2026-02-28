using UnityEngine;
using UnityEngine.Android;

public class MicPermissionTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Has mic permission: " +
            Permission.HasUserAuthorizedPermission(Permission.Microphone));

        Permission.RequestUserPermission(Permission.Microphone);
    }
}

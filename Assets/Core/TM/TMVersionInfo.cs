using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TMVersionInfo : MonoBehaviour
{
    public static string TMVersion = "Version 0.3.5 Beta";

    public TMP_Text VersionTextObject;

    private void Start()
    {
        if (VersionTextObject != null)
        {
            VersionTextObject.text = TMVersion;
        }
    }
}

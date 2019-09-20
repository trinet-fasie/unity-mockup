using UnityEngine;

public class WorldDescriptor : MonoBehaviour
{
    public Transform PlayerSpawnPoint;
    public string Name;
    public string Guid;
    public string Description;
    public string Image;
    public string AssetBundleLabel;
    public string[] DllNames;
    
    public string AuthorName;
    public string AuthorEmail;
    public string AuthorUrl;

    public string LicenseCode;
    public string LicenseVersion;

    public long CreatedAt;
}

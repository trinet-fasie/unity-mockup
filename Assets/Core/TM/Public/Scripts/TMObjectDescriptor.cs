using System;
using UnityEngine;

namespace TM.Public
{
    public class TMObjectDescriptor : MonoBehaviour
    {
        [TextArea] public string ConfigBlockly;

        [TextArea] public string ConfigAssetBundle;

        public Texture2D Icon;

        public bool DeveloperMode;

        public bool ShowDebug;

        public string Name;

        public string Prefab;

        public string Guid;

        public bool Embedded;

        public string AuthorName;
        public string AuthorEmail;
        public string AuthorUrl;

        public string LicenseCode;
        public string LicenseVersion;

        public int BuildVersion;

        public long CreatedAt;
        public long UpdatedAt;

        private void Reset()
        {
            Validate();
        }

        private void OnValidate()
        {
            Validate();
        }
        
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(AuthorName))
            {
                AuthorName = "Anonymous";
            }

            if (string.IsNullOrWhiteSpace(LicenseCode))
            {
                LicenseCode = "cc-by";
            }

            if (string.IsNullOrWhiteSpace(LicenseVersion))
            {
                LicenseVersion = "4.0";
            }

            if (CreatedAt == 0)
            {
                if (UpdatedAt != 0)
                {
                    CreatedAt = UpdatedAt;
                }
                else
                {
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    UpdatedAt = CreatedAt;
                }
            }
        }
    }

}
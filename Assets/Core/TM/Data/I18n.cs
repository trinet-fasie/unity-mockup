using System;
using System.Collections.Generic;
using System.Linq;
using TM;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Config : ILocalizable
{
    public string type { get; set; }
    public I18n i18n { get; set; }
}

[Serializable]
public class I18n
{
    public string ru { get; set; }
    public string en { get; set; }
}

public interface ILocalizable
{
    I18n i18n { get; set; }
}

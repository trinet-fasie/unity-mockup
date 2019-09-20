**************************************
*     HIGHLIGHT & SEE THROUGH         *
* Created by Ramiro Oliva (Kronnect) * 
*            README FILE             *
**************************************


How to use this asset
---------------------

Highlighting/customizing specific objects:

1) Add HighlightEffect.cs script to any GameObject. Customize the appearance options.
2) Optionally add HighlightTrigger.cs script to the GameObject. It will activate highlight on the gameobject when mouse pass over it.

Highlighting any object:

1) Select top menu GameObject -> Effects -> Highlight Plus -> Create Manager.
2) Customize behaviour of Highlight Manager.

Make transparent shaders compatible with See-Through effect:
If you want the See-Through effect be seen through other transparent objects, they need to be modified so they write to depth buffer (by default transparent objects do not write to z-buffer).
To do so, select top menu GameObject -> Effects -> Highlight Plus -> Add Depth To Transparent Object.


Events
------

When an object is highlighted you can use the HighlightStart or HighlightEnd events. Check SphereHighlightEventSample.cs script for an example.



Support & Feedback
------------------

If you like Highlight Plus, please rate it on the Asset Store. It encourages us to keep improving it! Thanks!

Have any question or issue?
* Email: contact@kronnect.me
* Support Forum: http://kronnect.me
* Twitter: @KronnectGames



Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Highlight Plus will be eventually available on the Asset Store.



More Cool Assets!
-----------------
Check out our other assets here:
https://assetstore.unity.com/publishers/15018



Version history
---------------

Version 1.3
- Added option to add depth compatibility for transparent shaders

Version 1.2.4
- [Fix] Fix for multiple skinned models
- [Fix] Fix for scaled skinned models

Version 1.2.3
- [Fix] Fixes for Steam VR

Version 1.2.1
- Internal improvements and fixes

Version 1.2.1
- [Fix] Fixed script execution order issue with scripts changing transform in LateUpdate()

Version 1.2
- Support for LOD groups

Version 1.1
- Redesigned editor inspector
- Minor improvements

Version 1.0.4
- Supports meshes with negative scales

Version 1.0.3
- Support for multiple submeshes

Version 1.0.2
- [Fix] Fixed scale issue with grouped objects

Version 1.0.1
- Supports combined meshes

Version 1.0 - Nov/2018
- Initial release

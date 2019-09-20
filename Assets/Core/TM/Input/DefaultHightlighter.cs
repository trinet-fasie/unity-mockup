using UnityEngine;
using TM.Public;

namespace TM
{
	public class DefaultHightlighter : MonoBehaviour, IHighlightAware
	{

		public HightLightConfig HightLightConfig()
		{
			return new HightLightConfig(false, 0.35f, Color.black, true, 0.5f, 1.5f, 0.17f, Color.cyan);
		}
	}
}
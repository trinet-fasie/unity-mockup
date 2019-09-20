﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightPlus;

namespace TM.Public
{
	public class TMHighlightEffect : MonoBehaviour
	{

		public HightLightConfig Configuration;
		
		#if UNITY_EDITOR
		[SerializeField]
		private bool _enableHighlight;
		#endif

		private HighlightEffect _effectHolder;
		private HighlightEffect _effect
		{
			get
			{
				if (_effectHolder == null)
				{
					_effectHolder = gameObject.GetComponent<HighlightEffect>();

					if (!_effectHolder)
					{
						_effectHolder = gameObject.AddComponent<HighlightEffect>();
					}
				}

				return _effectHolder;
			}
		}
		
		void Start()
		{
			SetupWithConfig(Configuration);
		}

		private void OnDestroy()
		{
			Destroy(_effectHolder);
		}

#if UNITY_EDITOR

		private void Update()
		{
			SetHighlightEnabled(_enableHighlight);
		}

		#endif

		private void SetupWithConfig(HightLightConfig config, HighlightEffect newEffect = null)
		{
			if (newEffect != null)
			{
				_effectHolder = newEffect;
			}
			
			if (config.Outline)
			{
				_effect.outline = 1;
				_effect.outlineWidth = config.OutlineWidth;
				_effect.outlineColor = config.OutlineColor;
			}
			else
			{
				_effect.outline = 0;
			}

			if (config.Glow)
			{
				_effect.glow = 1;
				_effect.glowWidth = config.GlowWidth;

				HighlightEffect.GlowPassData glowPass = new HighlightEffect.GlowPassData();
				glowPass.alpha = config.GlowAlpha;
				glowPass.offset = config.GlowOffset;
				glowPass.color = config.GlowColor;
                
				_effect.glowPasses = new [] { glowPass };
                
				_effect.glowDithering = false;
				_effect.glowAnimationSpeed = 0;
			}
			else
			{
				_effect.glow = 0;
			}

			if (config.SeeThrough)
			{
				_effect.seeThrough = HighlightEffect.SeeThroughMode.WhenHighlighted;
				_effect.seeThroughIntensity = config.SeeThroughIntensity;
				_effect.seeThroughTintAlpha = config.SeeThroughAlpha;
				_effect.seeThroughTintColor = config.SeeThroughColor;
			}
			else
			{
				_effect.seeThrough = HighlightEffect.SeeThroughMode.Never;
			}

			if (config.Overlay)
			{
				_effect.overlayColor = config.OverlayColor;
				_effect.overlay = config.OverlayAlpha;
				_effect.overlayAnimationSpeed = config.OverlayAnimationSpeed;
			}
			else
			{
				_effect.overlay = 0.0f;
			}
			
			_effect.Refresh();
		}
		
		public void SetConfiguration(HightLightConfig config, HighlightEffect newEffect = null)
		{
			Configuration = config;
			SetupWithConfig(config, newEffect);
		}

		public void SetHighlightEnabled(bool enabled)
		{
#if UNITY_EDITOR
			_enableHighlight = enabled;
#endif			
			_effect.highlighted = enabled;
		}
		
	}
}
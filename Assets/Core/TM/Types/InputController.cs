using System;
using System.Diagnostics.Eventing.Reader;
using Entitas;
using NLog;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.SecondaryControllerGrabActions;
using Object = UnityEngine.Object;
using UnityEngine.Experimental.XR.Interaction;
using TM.Data.ServerData;
using TM.Models.Data;
using TM.Public;

#pragma warning disable 618

namespace TM
{
    public class InputController
    {
        private readonly VRTK_InteractableObject _vio;
        private readonly GameObject _gameObject;
        public VRTK_ControllerEvents ControllerEvents;

        private IGrabStartAware _grabStart;
        private IGrabEndAware _grabEnd;
        private IGrabPointAware _grabPoint;
        private IUseStartAware _useStart;
        private IUseEndAware _useEnd;
        private IPointerClickAware _pointerClick;
        private IPointerInAware _pointerIn;
        private IPointerOutAware _pointerOut;
        private IHighlightAware _highlight;
        private ITouchStartAware _touchStart;
        private ITouchEndAware _touchEnd;
        private IHapticsAware _haptics;

        public Action OnUseStart = delegate { };
        public Action OnUseEnd = delegate { };
        public Action OnEditorGrabStart = delegate { };
        public Action OnEditorGrabEnd = delegate { };
        public Action OnPointerClick = delegate { };
        public Action OnPointerIn = delegate { };
        public Action OnPointerOut = delegate { };
        public Action OnEditorTouchStart = delegate { };
        public Action OnEditorTouchEnd = delegate { };

        private TransformDT _onInitTransform;
        
        private TransformDT _saveTransform;
        private readonly bool _isRoot;
        private readonly bool _isObject;
        private bool _enabled = true;
        private readonly ObjectController _objectController;

        private static InputController _lastHighlightedInputController;
        private static InputController _nextToHighlightInputController;
        private GameObject _highlightOverriderGameObject;

        private readonly InteractableObjectBehaviour _ioBehavior;

        public InputController(ObjectController objectController, GameObject gameObject, 
            bool isRoot = false, bool isObject = false)
        {
            _objectController = objectController;
            _gameObject = gameObject;
            _vio = _gameObject.AddComponent<VRTK_InteractableObject>();
            _isRoot = isRoot;
            _isObject = isObject;
            OnEditorGrabStart += OnGrabInit;
            OnEditorGrabEnd += OnUngrabInit;
            Init();

            _ioBehavior = gameObject.GetComponent<InteractableObjectBehaviour>();

            _vio.InteractableObjectGrabbed += OnAnyGrabStart;
            _vio.InteractableObjectUngrabbed += OnAnyGrabEnd;

        }

        public void ReturnPosition()
        {
            _onInitTransform.ToTransformUnity(_gameObject.transform);
        }

        public void Destroy()
        {
            Object.Destroy(_gameObject);
        }

        public void Init()
        {
            IUseStartAware useStartAware = _gameObject.GetComponent<IUseStartAware>();

            if (useStartAware != null)
            {
                _useStart = useStartAware;
            }

            IUseEndAware useEndAware = _gameObject.GetComponent<IUseEndAware>();

            if (useEndAware != null)
            {
                _useEnd = useEndAware;
            }

            IGrabStartAware grabStartAware = _gameObject.GetComponent<IGrabStartAware>();

            if (grabStartAware != null)
            {
                _grabStart = grabStartAware;
            }

            IGrabEndAware grabEndAware = _gameObject.GetComponent<IGrabEndAware>();

            if (grabEndAware != null)
            {
                _grabEnd = grabEndAware;
            }

            IGrabPointAware grabPointAware = _gameObject.GetComponent<IGrabPointAware>();

            if (grabPointAware != null)
            {
                _grabPoint = grabPointAware;
            }

            IPointerClickAware pointerClickAware = _gameObject.GetComponent<IPointerClickAware>();

            if (pointerClickAware != null)
            {
                AddPointerBehaviour();
                _pointerClick = pointerClickAware;
            }

            IPointerInAware pointerInAware = _gameObject.GetComponent<IPointerInAware>();

            if (pointerInAware != null)
            {
                AddPointerBehaviour();
                _pointerIn = pointerInAware;
            }

            IPointerOutAware pointerOutAware = _gameObject.GetComponent<IPointerOutAware>();

            if (pointerOutAware != null)
            {
                AddPointerBehaviour();
                _pointerOut = pointerOutAware;
            }

            ITouchStartAware touchStartAware = _gameObject.GetComponent<ITouchStartAware>();

            if (touchStartAware != null)
            {
                _touchStart = touchStartAware;
            }

            ITouchEndAware touchEndAware = _gameObject.GetComponent<ITouchEndAware>();

            if (touchEndAware != null)
            {
                _touchEnd = touchEndAware;
            }

            IHighlightAware highlightAware = _gameObject.GetComponent<IHighlightAware>();

            if (Settings.Instance().HighlightEnabled)
            {
                highlightAware = _gameObject.AddComponent<DefaultHightlighter>();
            }

            HighlightOverrider highlightOverrider = _gameObject.GetComponent<HighlightOverrider>();

            if (highlightOverrider != null)
            {
                _highlightOverriderGameObject = highlightOverrider.ObjectToHightlight;
            }

            if (highlightAware != null)
            {
                _highlight = highlightAware;
                AddHighLighter(highlightAware);
            }

            if (Settings.Instance().HighlightEnabled)
            {
                EnableHighlight();
            }

            IHapticsAware hapticsAware = _gameObject.GetComponent<IHapticsAware>();

            if (hapticsAware == null &&
                (Settings.Instance().TouchHapticsEnabled ||
                 Settings.Instance().GrabHapticsEnabled ||
                 Settings.Instance().UseHapticsEnabled))
            {
                hapticsAware = _gameObject.AddComponent<DefaultHaptics>();
            }

            if (hapticsAware != null)
            {
                AddHaptics(hapticsAware);
                _haptics = hapticsAware;
            }
        }

        private void AddPointerBehaviour()
        {
            var pointerBehaviour = _gameObject.GetComponent<ObjectPointerBehaviour>();

            if (pointerBehaviour != null)
            {
                return;
            }

            pointerBehaviour = _gameObject.AddComponent<ObjectPointerBehaviour>();
            pointerBehaviour.Init(this);
        }

        private void OnAnyGrabStart(object sender, InteractableObjectEventArgs e)
        {
            if (!_isRoot)
            {
                return;
            }
            

            ControllerEvents = _vio.GetGrabbingObject().GetComponent<VRTK_ControllerEvents>();
        }

        private void OnAnyGrabEnd(object sender, InteractableObjectEventArgs e)
        {
            if (!_isRoot)
            {
                return;
            }
        }

        private void OnUngrabInit()
        {
            TransformDT newTransform = _gameObject.transform.ToTransformDT();

            WorldData.ObjectsAreChanged = true;
        }

        private void OnGrabInit()
        {
            _saveTransform = _gameObject.transform.ToTransformDT();
            
            _onInitTransform = _objectController.gameObject.transform.ToTransformDT();
            
        }

        public void DisableInputGrab()
        {
            _vio.isGrabbable = false;

            if (_grabStart != null)
            {
                _vio.InteractableObjectGrabbed -= OnGrabStartVoid;
            }

            if (_grabEnd != null)
            {
                _vio.InteractableObjectUngrabbed -= OnGrabEndVoid;
            }
        }

        public void EnableInputGrab()
        {
            _vio.isGrabbable = true;

            if (_grabStart != null)
            {
                _vio.InteractableObjectGrabbed += OnGrabStartVoid;
            }

            if (_grabEnd != null)
            {
                _vio.InteractableObjectUngrabbed += OnGrabEndVoid;
            }
        }

        public void EnableHighlight()
        {
            if (_highlight != null)
            {
                _vio.InteractableObjectTouched += HighlightObject;
                _vio.InteractableObjectUntouched += UnHighlightObject;
                _vio.InteractableObjectGrabbed += UnHighlightObject;
                _vio.InteractableObjectUsed += UnHighlightObject;
                _vio.InteractableObjectUnused += HighlightObject;
            }
        }

        public void DisableHighlight()
        {
            if (_highlight != null)
            {
                UnHighlightObject(null, new InteractableObjectEventArgs());
                _vio.InteractableObjectTouched -= HighlightObject;
                _vio.InteractableObjectUntouched -= UnHighlightObject;
                _vio.InteractableObjectGrabbed -= UnHighlightObject;
                _vio.InteractableObjectUsed -= UnHighlightObject;
                _vio.InteractableObjectUnused -= HighlightObject;
            }
        }

        public void EnableDrop()
        {
            _vio.validDrop = VRTK_InteractableObject.ValidDropTypes.DropAnywhere;
            _vio.secondaryGrabActionScript.enabled = true;
        }

        public void DisableDrop()
        {
            _vio.validDrop = VRTK_InteractableObject.ValidDropTypes.NoDrop;
            _vio.secondaryGrabActionScript.enabled = false;
        }

        public void ForceDropIfNeeded()
        {
            if (!ControllerEvents.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress)) {
                _vio.ForceStopInteracting();
            }
        }

        public void ApplyInput()
        {
            if (_useStart != null)
            {
                AddUseStartAction();
            }

            if (_useEnd != null)
            {
                AddUseEndAction();
            }

            if (_grabStart != null)
            {
                AddGrabStartAction();
            }

            if (_grabEnd != null)
            {
                AddGrabEndAction();
            }

            if (_pointerClick != null)
            {
                AddPointerClickAction();
            }

            if (_pointerIn != null)
            {
                AddPointerInAction();
            }

            if (_pointerOut != null)
            {
                AddPointerOutAction();
            }

            if (_touchStart != null)
            {
                AddTouchStartAction();
            }

            if (_touchEnd != null)
            {
                AddTouchEndtAction();
            }

            if (_grabStart == null && _grabEnd == null && _useStart == null && _useEnd == null)
            {
                DisableHighlight();
            }

            if (!_ioBehavior)
            {
                return;
            }
            
            if (!_ioBehavior.IsUsable)
            {
                DisableInputUsing();
            }

            if (!_ioBehavior.IsGrabbable)
            {
                DisableInputGrab();
            }

            if (!_ioBehavior.IsTouchable)
            {
                DisableTouch();
            }
        }

        public void AddUseStartAction()
        {
            _vio.isUsable = true;
            _vio.useOverrideButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
            _vio.InteractableObjectUsed += OnUseStartVoid;
        }

        public void AddUseEndAction()
        {
            _vio.isUsable = true;
            _vio.useOverrideButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
            _vio.InteractableObjectUnused += OnUseEndVoid;
        }

        public void AddGrabStartAction()
        {
            _vio.isGrabbable = true;
            var grab = _gameObject.GetComponent<VRTK_InteractControllerAppearance>();

            if (grab == null)
            {
                grab = _gameObject.AddComponent<VRTK_InteractControllerAppearance>();
            }

            grab.hideControllerOnGrab = true;

            _vio.secondaryGrabActionScript = _gameObject.GetComponent<VRTK_SwapControllerGrabAction>();

            if (_vio.secondaryGrabActionScript == null)
            {
                _vio.secondaryGrabActionScript = _gameObject.AddComponent<VRTK_SwapControllerGrabAction>();
            }

            VRTK_FixedJointGrabAttach vrtkFixedJointGrab = _gameObject.GetComponent<VRTK_FixedJointGrabAttach>();

            if (vrtkFixedJointGrab == null)
            {
                vrtkFixedJointGrab = _gameObject.AddComponent<VRTK_FixedJointGrabAttach>();
                vrtkFixedJointGrab.breakForce = Single.PositiveInfinity;
            }

            if (_grabPoint != null)
            {
                vrtkFixedJointGrab.precisionGrab = false;
                vrtkFixedJointGrab.leftSnapHandle = _grabPoint.GetLeftGrabPoint();
                vrtkFixedJointGrab.rightSnapHandle = _grabPoint.GetRightGrabPoint();
            }
            else
            {
                vrtkFixedJointGrab.precisionGrab = true;
            }

            _vio.grabOverrideButton = VRTK_ControllerEvents.ButtonAlias.GripPress;
            _vio.InteractableObjectGrabbed += OnGrabStartVoid;
        }

        public void AddGrabEndAction()
        {
            _vio.isGrabbable = true;
            var grab = _gameObject.GetComponent<VRTK_InteractControllerAppearance>();

            if (grab == null)
            {
                grab = _gameObject.AddComponent<VRTK_InteractControllerAppearance>();
            }

            grab.hideControllerOnGrab = true;

            _vio.secondaryGrabActionScript = _gameObject.GetComponent<VRTK_SwapControllerGrabAction>();

            if (_vio.secondaryGrabActionScript == null)
            {
                _vio.secondaryGrabActionScript = _gameObject.AddComponent<VRTK_SwapControllerGrabAction>();
            }

            VRTK_FixedJointGrabAttach vrtkFixedJointGrab = _gameObject.GetComponent<VRTK_FixedJointGrabAttach>();

            if (vrtkFixedJointGrab == null)
            {
                vrtkFixedJointGrab = _gameObject.AddComponent<VRTK_FixedJointGrabAttach>();
                vrtkFixedJointGrab.breakForce = Single.PositiveInfinity;
            }

            if (_grabPoint != null)
            {
                vrtkFixedJointGrab.precisionGrab = false;
                vrtkFixedJointGrab.leftSnapHandle = _grabPoint.GetLeftGrabPoint();
                vrtkFixedJointGrab.rightSnapHandle = _grabPoint.GetRightGrabPoint();
            }
            else
            {
                vrtkFixedJointGrab.precisionGrab = true;
            }

            _vio.grabOverrideButton = VRTK_ControllerEvents.ButtonAlias.GripPress;
            _vio.InteractableObjectUngrabbed += OnGrabEndVoid;
        }

        public void AddPointerClickAction()
        {
            OnPointerClick = _pointerClick.OnPointerClick;
        }

        public void AddPointerInAction()
        {
            OnPointerIn = _pointerIn.OnPointerIn;
        }

        public void AddPointerOutAction()
        {
            OnPointerOut = _pointerOut.OnPointerOut;
        }

        public void AddTouchStartAction()
        {
            _vio.InteractableObjectTouched += OnTouchStartVoid;
        }

        public void AddTouchEndtAction()
        {
            _vio.InteractableObjectUntouched += OnTouchEndVoid;
        }

        public void AddHighLighter(IHighlightAware highlight)
        {
            HightLightConfig config = highlight.HightLightConfig();
            
            TMHighlightEffect highlighter = _highlightOverriderGameObject ? _highlightOverriderGameObject.GetComponent<TMHighlightEffect>() : _gameObject.GetComponent<TMHighlightEffect>();
            
            if (highlighter == null)
            {
                highlighter = _highlightOverriderGameObject ? _highlightOverriderGameObject.AddComponent<TMHighlightEffect>() : _gameObject.AddComponent<TMHighlightEffect>();
            }

            try
            {
                highlighter.SetConfiguration(config);
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error($"Can not add highlight to game object = {_gameObject}");
            }
           
        }

        public void AddHaptics(IHapticsAware haptics)
        {
            HapticsConfig onUse = haptics.HapticsOnUse();
            HapticsConfig onTouch = haptics.HapticsOnTouch();
            HapticsConfig onGrab = haptics.HapticsOnGrab();

            VRTK_InteractHaptics interactHaptic = _gameObject.GetComponent<VRTK_InteractHaptics>();

            if (interactHaptic == null)
            {
                interactHaptic = _gameObject.AddComponent<VRTK_InteractHaptics>();
            }

            if (onUse != null)
            {
                interactHaptic.strengthOnUse = onUse.Strength;
                interactHaptic.intervalOnUse = onUse.Interval;
                interactHaptic.durationOnUse = onUse.Duration;
                //TODO:проверить, нужен ли этот метод в принципе
                //Убрано при обновлении VRTK
                //interactHaptic.cancelOnUnuse = onUse.CancelOnUnaction;
            }
            else
            {
                interactHaptic.strengthOnUse = 0;
                interactHaptic.durationOnUse = 0;
            }

            if (onTouch != null)
            {
                interactHaptic.strengthOnTouch = onTouch.Strength;
                interactHaptic.intervalOnTouch = onTouch.Interval;
                interactHaptic.durationOnTouch = onTouch.Duration;
                //TODO:проверить, нужен ли этот метод в принципе
                //Убрано при обновлении VRTK
                //interactHaptic.cancelOnUntouch = onTouch.CancelOnUnaction;
            }
            else
            {
                interactHaptic.strengthOnTouch = 0;
                interactHaptic.durationOnTouch = 0;
            }

            if (onGrab != null)
            {
                interactHaptic.strengthOnGrab = onGrab.Strength;
                interactHaptic.intervalOnGrab = onGrab.Interval;
                interactHaptic.durationOnGrab = onGrab.Duration;
                //TODO:проверить, нужен ли этот метод в принципе
                //Убрано при обновлении VRTK
                //interactHaptic.cancelOnUngrab = onGrab.CancelOnUnaction;
            }
            else
            {
                interactHaptic.strengthOnGrab = 0;
                interactHaptic.durationOnGrab = 0;
            }
        }

        public void Vibrate(GameObject controllerObject, float strength, float duration,
            float interval)
        {
            if (!_vio.IsGrabbed() && !_vio.IsUsing())
            {
                Debug.LogWarning("Can't vibrate with object not grabbed or in use");

                return;
            }

            VRTK_ControllerReference controller = VRTK_ControllerReference.GetControllerReference(controllerObject);

            if (controller == null)
            {
                Debug.LogWarning("Can't vibrate: " + controllerObject + " is not a controller");

                return;
            }

            VRTK_ControllerHaptics.TriggerHapticPulse(controller,
                strength,
                duration,
                interval);
        }

        private void UnHighlightObject(object sender, InteractableObjectEventArgs e)
        {
            if (_gameObject == null)
            {
                return;
            }
            TMHighlightEffect highlighter = _highlightOverriderGameObject ? _highlightOverriderGameObject.GetComponent<TMHighlightEffect>()  :  _gameObject.GetComponent<TMHighlightEffect>();

            if (highlighter != null)
            {
                highlighter.SetHighlightEnabled(false);

                if (_lastHighlightedInputController == this)
                {
                    _lastHighlightedInputController = null;
                }
                else
                {
                    _nextToHighlightInputController = this;
                }

                if (_nextToHighlightInputController != null)
                {
                    if (_nextToHighlightInputController != this)
                    {
                        _nextToHighlightInputController.HighlightObject(this, e);
                    }

                    _nextToHighlightInputController = null;
                }

                if (sender != null)
                {
                    if (sender.GetType() == typeof(InputController))
                    {
                        _nextToHighlightInputController = this;
                    }
                }
            }
        }

        private void HighlightObject(object sender, InteractableObjectEventArgs e)
        {
            if (_gameObject == null)
            {
                return;
            }
            TMHighlightEffect highlighter = _highlightOverriderGameObject ? _highlightOverriderGameObject.GetComponent<TMHighlightEffect>() : _gameObject.GetComponent<TMHighlightEffect>();

            if (highlighter != null)
            {
                if (_lastHighlightedInputController != null)
                {
                    _lastHighlightedInputController.UnHighlightObject(this, e);
                }

                highlighter.SetHighlightEnabled(true);
                _lastHighlightedInputController = this;
            }
        }

        public void EnableEditorInput(VRTK_ControllerEvents.ButtonAlias button)
        {
            _vio.isGrabbable = true;
            var grab = _gameObject.GetComponent<VRTK_InteractControllerAppearance>();

            if (grab == null)
            {
                grab = _gameObject.AddComponent<VRTK_InteractControllerAppearance>();
            }

            grab.hideControllerOnGrab = true;

            _vio.secondaryGrabActionScript = _gameObject.GetComponent<VRTK_SwapControllerGrabAction>();

            if (_vio.secondaryGrabActionScript == null)
            {
                _vio.secondaryGrabActionScript = _gameObject.AddComponent<VRTK_SwapControllerGrabAction>();
            }

            VRTK_FixedJointGrabAttach vrtkFixedJointGrab = _gameObject.GetComponent<VRTK_FixedJointGrabAttach>();

            if (vrtkFixedJointGrab == null)
            {
                vrtkFixedJointGrab = _gameObject.AddComponent<VRTK_FixedJointGrabAttach>();
                vrtkFixedJointGrab.breakForce = Single.PositiveInfinity;
            }

            vrtkFixedJointGrab.precisionGrab = true;

            _vio.grabOverrideButton = button;
            _vio.InteractableObjectGrabbed += EditorGrabbed;
            _vio.InteractableObjectUngrabbed += EditorUngrabbed;
            _vio.InteractableObjectTouched += EditorToched;
            _vio.InteractableObjectTouched += EditorUnToched;
        }

        public void DisableEditorInput()
        {
            _vio.isGrabbable = false;
            _vio.InteractableObjectGrabbed -= EditorGrabbed;
            _vio.InteractableObjectUngrabbed -= EditorUngrabbed;
            _vio.InteractableObjectTouched -= EditorToched;
            _vio.InteractableObjectTouched -= EditorUnToched;
        }

        public void DisableInputUsing()
        {
            _vio.isUsable = false;

            if (_useStart != null)
            {
                _vio.InteractableObjectUsed -= OnUseStartVoid;
            }

            if (_useEnd != null)
            {
                _vio.InteractableObjectUnused -= OnUseEndVoid;
            }
        }

        public void EnableInputUsing()
        {
            _vio.isUsable = true;

            if (_useStart != null)
            {
                _vio.InteractableObjectUsed += OnUseStartVoid;
            }

            if (_useEnd != null)
            {
                _vio.InteractableObjectUnused += OnUseEndVoid;
            }
        }

        private void OnUseStartVoid(object sender, InteractableObjectEventArgs interactableObjectEventArgs)
        {
                var context = new UsingContext {GameObject = _vio.GetUsingObject()};
                _useStart?.OnUseStart(context);
            
        }

        private void OnUseEndVoid(object sender, InteractableObjectEventArgs interactableObjectEventArgs)
        {
                _useEnd?.OnUseEnd();
            
        }

        private void OnGrabStartVoid(object sender, InteractableObjectEventArgs interactableObjectEventArgs)
        {
                GrabingContext context = new GrabingContext {GameObject = _vio.GetGrabbingObject()};
                _grabStart?.OnGrabStart(context);
            
        }

        private void OnGrabEndVoid(object sender, InteractableObjectEventArgs interactableObjectEventArgs)
        {
                _grabEnd?.OnGrabEnd();
            
        }

        private void EditorUngrabbed(
            object sender,
            InteractableObjectEventArgs interactableObjectEventArgs)
        {
                OnEditorGrabEnd();
        }

        private void EditorToched(
            object sender,
            InteractableObjectEventArgs interactableObjectEventArgs)
        {
                OnEditorTouchStart();
           
        }

        private void EditorUnToched(
            object sender,
            InteractableObjectEventArgs interactableObjectEventArgs)
        {
                OnEditorTouchEnd();
           
        }

        private void OnTouchStartVoid(object sender, InteractableObjectEventArgs interactableObjectEventArgs)
        {
            
                _touchStart?.OnTouchStart();
        }

        private void OnTouchEndVoid(object sender, InteractableObjectEventArgs interactableObjectEventArgs)
        {
                _touchEnd?.OnTouchEnd();
            
        }

        public bool CanGrab() => _grabEnd != null || _grabStart != null;

        public bool IsRoot() => _isRoot;

        public bool IsObject() => _isObject;

        private void EditorGrabbed(object sender, InteractableObjectEventArgs interactableObjectEventArgs)
        {
                OnEditorGrabStart();
            
        }

      
        public TransformDto GetTransform()
        {
            if (_gameObject.GetComponent<ObjectId>() == null)
            {
                return null;
            }

            int id = _gameObject.GetComponent<ObjectId>().Id;
            TransformDT transform = _gameObject.transform.ToTransformDT();

            return new TransformDto {Id = id, Transform = transform};
        }

        public void Enable()
        {
            if (_enabled)
            {
                return;
            }

            ApplyInput();
            _enabled = true;
        }

        public void Disable()
        {
            if (!_enabled)
            {
                return;
            }

            DisableInputGrab();
            DisableInputUsing();
            DisableHighlight();
            _enabled = false;
        }


        public void DisableTouch()
        {
            if (_touchStart != null)
            {
                _vio.InteractableObjectTouched -= OnTouchStartVoid;
            }

            if (_touchEnd != null)
            {
                _vio.InteractableObjectUntouched -= OnTouchEndVoid;
            }
        }

        public void EnableTouch()
        {
            if (_touchStart != null)
            {
                _vio.InteractableObjectTouched += OnTouchStartVoid;
            }

            if (_touchEnd != null)
            {
                _vio.InteractableObjectUntouched += OnTouchEndVoid;
            }
        }

        public bool IsConnectedToGameObject(GameObject go)
        {
            return go == _gameObject;
        }
    }
}

public class TransformDto
{
    public int Id;
    public TransformDT Transform;
}
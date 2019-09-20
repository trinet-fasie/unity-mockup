//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class GameEntity {

    public TM.ECS.Components.UI.UIObjectComponent uIObject { get { return (TM.ECS.Components.UI.UIObjectComponent)GetComponent(GameComponentsLookup.UIObject); } }
    public bool hasUIObject { get { return HasComponent(GameComponentsLookup.UIObject); } }

    public void AddUIObject(UnityEngine.GameObject newValue) {
        var index = GameComponentsLookup.UIObject;
        var component = CreateComponent<TM.ECS.Components.UI.UIObjectComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceUIObject(UnityEngine.GameObject newValue) {
        var index = GameComponentsLookup.UIObject;
        var component = CreateComponent<TM.ECS.Components.UI.UIObjectComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveUIObject() {
        RemoveComponent(GameComponentsLookup.UIObject);
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentMatcherApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public sealed partial class GameMatcher {

    static Entitas.IMatcher<GameEntity> _matcherUIObject;

    public static Entitas.IMatcher<GameEntity> UIObject {
        get {
            if (_matcherUIObject == null) {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.UIObject);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherUIObject = matcher;
            }

            return _matcherUIObject;
        }
    }
}

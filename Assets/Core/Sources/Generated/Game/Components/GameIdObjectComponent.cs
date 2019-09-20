//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class GameEntity {

    public TM.ECS.Components.IdObjectComponent idObject { get { return (TM.ECS.Components.IdObjectComponent)GetComponent(GameComponentsLookup.IdObject); } }
    public bool hasIdObject { get { return HasComponent(GameComponentsLookup.IdObject); } }

    public void AddIdObject(int newValue) {
        var index = GameComponentsLookup.IdObject;
        var component = CreateComponent<TM.ECS.Components.IdObjectComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceIdObject(int newValue) {
        var index = GameComponentsLookup.IdObject;
        var component = CreateComponent<TM.ECS.Components.IdObjectComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveIdObject() {
        RemoveComponent(GameComponentsLookup.IdObject);
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

    static Entitas.IMatcher<GameEntity> _matcherIdObject;

    public static Entitas.IMatcher<GameEntity> IdObject {
        get {
            if (_matcherIdObject == null) {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.IdObject);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherIdObject = matcher;
            }

            return _matcherIdObject;
        }
    }
}

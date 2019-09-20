//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class GameEntity {

    public TM.ECS.Components.IconComponent icon { get { return (TM.ECS.Components.IconComponent)GetComponent(GameComponentsLookup.Icon); } }
    public bool hasIcon { get { return HasComponent(GameComponentsLookup.Icon); } }

    public void AddIcon(UnityEngine.Sprite newValue) {
        var index = GameComponentsLookup.Icon;
        var component = CreateComponent<TM.ECS.Components.IconComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceIcon(UnityEngine.Sprite newValue) {
        var index = GameComponentsLookup.Icon;
        var component = CreateComponent<TM.ECS.Components.IconComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveIcon() {
        RemoveComponent(GameComponentsLookup.Icon);
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

    static Entitas.IMatcher<GameEntity> _matcherIcon;

    public static Entitas.IMatcher<GameEntity> Icon {
        get {
            if (_matcherIcon == null) {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.Icon);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherIcon = matcher;
            }

            return _matcherIcon;
        }
    }
}

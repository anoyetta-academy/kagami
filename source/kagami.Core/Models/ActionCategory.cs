namespace kagami.Models
{
    public enum ActionCategory
    {
        Unknown = 0,
        AutoAttack,
        Spell,
        Weaponskill,
        Ability,
        Item,
        DoLAbility,
        DoHAbility,
        Event,
        LimitBreak,
        System,
        Artillery,
        Mount,
        Glamour,
        ItemManipulaction,
        AdrenalineRush,

        GainsEffectSelf = 128,
        LosesEffectSelf,
        GainsEffectParty,
        LosesEffectParty,
    }
}

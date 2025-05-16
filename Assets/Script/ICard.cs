using UnityEngine;

public interface ICard
{
    void Select();
    void Deselect();
    RuleTile.TilingRuleOutput.Transform Transform { get; }
}

public interface ICardGroupManager
{
    void AssignCardToGroup(ICard card);
    void RemoveCardFromGroup(ICard card);
}
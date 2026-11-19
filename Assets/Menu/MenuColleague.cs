using UnityEngine;

public abstract class MenuColleague : MonoBehaviour
{
    protected IMenuMediator Mediator;
    public void SetMediator(IMenuMediator mediator) => Mediator = mediator;
}
using UnityEngine;

public abstract class LocationColleague : MonoBehaviour
{
    protected ILocationMediator Mediator;
    public void SetMediator(ILocationMediator mediator) => Mediator = mediator;
}

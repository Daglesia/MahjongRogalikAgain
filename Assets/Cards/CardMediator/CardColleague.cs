using UnityEngine;

    public abstract class CardColleague : MonoBehaviour
    {
        protected ICardMediator Mediator;
        public void SetMediator(ICardMediator mediator) => Mediator = mediator;
    }

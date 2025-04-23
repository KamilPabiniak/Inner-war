using UnityEngine;

namespace Enemy.State
{
    public interface IEnemyState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        void EnterState(EnemyBrain context);

        /// <summary>
        /// Called every frame while this state is active.
        /// </summary>
        void UpdateState(EnemyBrain context);

        /// <summary>
        /// Called when exiting the state.
        /// </summary>
        void ExitState(EnemyBrain context);
    }
}
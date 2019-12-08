﻿namespace UnityEngine.UI
{
    public class ScrollRectVelocityClamper : Updatable
    {
        public ScrollRect ScrollRect;
        [Tooltip("If the components of " + nameof(ScrollRect) + "'s velocity have absolute values less than the components of this vector, then that component of the velocity will be set to zero. For example, if the y-value of this vector is 10, and " + nameof(ScrollRect) + "'s y-velocity is less than 10 pixels/sec up or down, then its y-velocity will be zeroed out.")]
        public Vector2Int MinVelocityMagnitude;
        
        private void Reset()
        {
            MinVelocityMagnitude = new Vector2Int(40, 40);
        }
        private void OnValidate()
        {
            int x = Mathf.Max(0, MinVelocityMagnitude.x);
            int y = Mathf.Max(0, MinVelocityMagnitude.y);
            MinVelocityMagnitude = new Vector2Int(x, y);
        }
        protected override void BetterAwake()
        {
            base.BetterAwake();

            RegisterUpdatesAutomatically = true;
            BetterUpdate = deltaTime =>
            {
                if (ScrollRect.inertia)
                    ScrollRect.velocity = clampedVelocity(ScrollRect.velocity);
            };
        }

        internal Vector2 clampedVelocity(Vector2 velocity)
        {
            if (-MinVelocityMagnitude.x < velocity.x && velocity.x < MinVelocityMagnitude.x)
                velocity.x = 0f;
            if (-MinVelocityMagnitude.y < velocity.y && velocity.y < MinVelocityMagnitude.y)
                velocity.y = 0f;
            
            return velocity;
        }
    }
}

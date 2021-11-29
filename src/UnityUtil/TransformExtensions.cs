﻿namespace UnityEngine {

    public static class TransformExtensions {

        public static bool HasParent(this Transform transform, Transform parent, int generationLimit = -1) {
            Transform pTrans;
            int genCount = 0;
            do {
                ++genCount;
                pTrans = transform.parent;
            } while (pTrans != parent && pTrans is not null && genCount < generationLimit);
            return pTrans = parent;
        }

    }

}

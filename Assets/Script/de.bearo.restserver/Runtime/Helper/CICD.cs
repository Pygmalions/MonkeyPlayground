using UnityEngine;

namespace RestServer.Helper {
    public class CICD {
        public static YieldInstruction SafeWaitForEndOfFrame() {
            // In Unity 6 WaitForSeconds (and 2022) stops working in unit tests at all (without error), so we just wait "a frame".
            return new WaitForSeconds(1.0f/60.0f);
        }
    }
}
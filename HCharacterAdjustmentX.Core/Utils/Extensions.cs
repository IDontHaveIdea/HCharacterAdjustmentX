//
// Extensions
//
// Ignore Spelling: cha

using UnityEngine;

using CTRL = IDHIPlugins.HCharaAdjustmentX.HCharaAdjustmentXController;


namespace IDHIPlugins
{
    internal static class Extensions
    {
        public static string FormatVectorToDelete(
            this Vector3 self,
            string decimals = default,
            int spaces = default)
        {
            string formatString;

            formatString = $"( " +
                $"{string.Format($"{{0,{spaces}:{decimals}}}", self.x)}, " +
                $"{string.Format($"{{0,{spaces}:{decimals}}}", self.y)}, " +
                $"{string.Format($"{{0,{spaces}:{decimals}}}", self.z)} )";

            return formatString;
        }

        /// <summary>
        /// Adjust move vector to transform vector in Unity when using transform when
        /// moving forward sometimes is in the X axis (Why?) vector representing a
        /// move are in the form (right, up, forward) this adjust the vector to the
        /// game transform
        /// </summary>
        /// <param name="self">object self reference</param>
        /// <param name="transform">character Transform</param>
        /// <returns></returns>
        public static Vector3 MovementTransform(this Vector3 self, Transform transform)
        {
            var result = new Vector3(0, 0, 0);

            // sides move
            result += transform.right * self.x;
            // up/down move
            result += transform.up * self.y;
            // forward/backward move
            result += transform.forward * self.z;

            return result;
        }

        public static CTRL GetController(this ChaControl chaControl)
        {
            return chaControl == null
                ? null : chaControl.GetComponent<CTRL>();
        }

        public static CTRL GetController(this SaveData.Heroine heroine)
        {
            return GetController(heroine?.chaCtrl);
        }
    }
}

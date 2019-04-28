using Unity.Mathematics;

namespace Bhell.Utils {

    public static class Bezier {

        public static float3 GetPoint(float3 p0, float3 p1, float3 p2, float t) {
            return
                (1f - t) * (1f - t) * p0 +
                2f * (1f - t) * t * p1 +
                t * t * p2;
        }

        public static float3 GetFirstDerivative(float3 p0, float3 p1, float3 p2, float t) {
            return
                2f * (1f - t) * (p1 - p0) +
                2f * t * (p2 - p1);
        }

        public static float3 GetPoint(float3 p0, float3 p1, float3 p2, float3 p3, float t) {
            return
                (1f - t) * (1f - t) * (1f - t) * p0 +
                3f * (1f - t) * (1f - t) * t * p1 +
                3f * (1f - t) * t * t * p2 +
                t * t * t * p3;
        }

        public static float3 GetFirstDerivative(float3 p0, float3 p1, float3 p2, float3 p3, float t) {
            return
                3f * (1f - t) * (1f - t) * (p1 - p0) +
                6f * (1f - t) * t * (p2 - p1) +
                3f * t * t * (p3 - p2);
        }

        public static float GetLength(float3 p0, float3 p1, float3 p2, int steps) {
            float3 point, prevPoint = p0;
            float sum = 0;
            for(int i = 1; i <= steps; ++i) {
                point = GetPoint(p0, p1, p2, ((float)i) / ((float)steps));
                sum += math.length(point - prevPoint);
                prevPoint = point;
            }
            return sum;
        }

        public static float GetLength(float3 p0, float3 p1, float3 p2, float3 p3, int steps) {
            float3 point, prevPoint = p0;
            float sum = 0;
            for(int i = 1; i <= steps; ++i) {
                point = GetPoint(p0, p1, p2, p3, ((float)i) / ((float)steps));
                sum += math.length(point - prevPoint);
                prevPoint = point;
            }
            return sum;
        }
    }
}
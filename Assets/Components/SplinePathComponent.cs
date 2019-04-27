using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Bhell.Components {

    [Serializable]
    public struct SplinePathAnimation : IComponentData {
        public float time;
        public Entity target;
    }

    [Serializable]
    public struct SplinePathElement : IBufferElementData {
        public float3 point;
        public float3 forward;
        public float rotation;
        public float time;

        public float3 backward {
            get { return - forward; }
            set { forward = -value; }
        }
    }
}

/*
public class SplinePathProxy : DynamicBufferProxy<SplinePathElement> {}

[CustomEditor(typeof(SplinePathProxy))]
public class SplinePathEditor : Editor {
    private void OnSceneGUI() {
        SplinePathProxy proxy = (SplinePathProxy)target;
        List<SplinePathElement> points = new List<SplinePathElement>(proxy.Value);
        for (int i = 0; i < points.Count; ++i) {
            DrawPoint(points[i]);
            if (i > 0) DrawSegment(points[i - 1], points[i]);
        }
    }

    protected void DrawPoint(SplinePathElement point) {
        float size = HandleUtility.GetHandleSize(point.point) * 0.1f;
        Handles.Button(point.point, Quaternion.identity, size, size, Handles.CubeHandleCap);
    }

    protected void DrawSegment(SplinePathElement start, SplinePathElement end) {
        Handles.DrawBezier(start.point, end.point, start.point + start.forwardControl, end.point + end.backwardControl, Color.red, null, 2f);
    }
}
*/
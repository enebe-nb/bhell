using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Bhell.Components {

    [Serializable]
    [WriteGroup(typeof(LocalToWorld))]
    [WriteGroup(typeof(LocalToParent))]
    public struct SplineAnimationSpeed : IComponentData {
        public Entity spline;
        public int index;
        public int segIndex;
        public float t;
        public float speed;
        public bool isLoop;
    }

    public struct SplineElement : IBufferElementData {
        public float3 point;
        public float3 forward;
        public quaternion rotation;

        public float3 backward {
            get { return - forward; }
            set { forward = -value; }
        }
    }

    public struct SplineSegment : IBufferElementData {
        public float start;
        public float end;
        public float length;
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
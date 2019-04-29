using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class BufferEnumerableAttribute : Attribute {
    public Type enumerableType;
    public BufferEnumerableAttribute(Type enumerableType) {
        this.enumerableType = enumerableType;
        if (!typeof(IBufferElementData).IsAssignableFrom(enumerableType)) throw new ArgumentException("Type must implement IBufferElementData");
    }
}

public class ScriptableEntity : ScriptableObject {

    private struct Pair { public FieldInfo field; public Type type; }
    private LinkedList<Pair> componentTypes = new LinkedList<Pair>();
    private LinkedList<Pair> bufferTypes = new LinkedList<Pair>();
    private ComponentType[] archetype;

    public ScriptableEntity() {
        List<ComponentType> tmpArray = new List<ComponentType>();

        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        foreach (FieldInfo info in this.GetType().GetFields(flags)) {
            if (info.IsPrivate && info.GetCustomAttribute<SerializeField>(true) == null) continue;
            if (typeof(IComponentData).IsAssignableFrom(info.FieldType)) {
                componentTypes.AddLast(new Pair{field = info, type = info.FieldType});
                tmpArray.Add(info.FieldType);
            } else {
                BufferEnumerableAttribute attr = info.GetCustomAttribute<BufferEnumerableAttribute>(true);
                if (attr != null && typeof(IEnumerable).IsAssignableFrom(info.FieldType)) {
                    bufferTypes.AddLast(new Pair{field = info, type = attr.enumerableType});
                    tmpArray.Add(attr.enumerableType);
                }
            }
        }

        archetype = tmpArray.ToArray();
    }

    public EntityArchetype CreateArchetype(EntityManager manager) {
        return manager.CreateArchetype(archetype);
    }

    public Entity CreateEntity(EntityManager manager) {
        Entity entity = manager.CreateEntity(archetype);
        SetEntity(manager, entity);
        return entity;
    }

    public void SetEntity(EntityManager manager, Entity entity) {
        foreach(Pair pair in componentTypes) {
            MethodInfo setComponentMethod = manager.GetType().GetMethod("SetComponentData").MakeGenericMethod(pair.type);
            setComponentMethod.Invoke(manager, new object[]{entity, pair.field.GetValue(this)});
        }

        foreach(Pair pair in bufferTypes) {
            MethodInfo getBufferMethod = manager.GetType().GetMethod("GetBuffer").MakeGenericMethod(pair.type);
            object buffer = getBufferMethod.Invoke(manager, new object[]{entity});
            MethodInfo clearMethod = buffer.GetType().GetMethod("Clear");
            MethodInfo addMethod = buffer.GetType().GetMethod("Add");
            clearMethod.Invoke(buffer, null);
            foreach(object data in (IEnumerable)pair.field.GetValue(this)) {
                addMethod.Invoke(buffer, new object[]{data});
            }
        }
    }
}
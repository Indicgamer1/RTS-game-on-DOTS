using Unity.Entities;
using UnityEngine;

public class FreindlyAuthoring : MonoBehaviour
{
    class FreindlyAuthoringBaker : Baker<FreindlyAuthoring>
    {
        public override void Bake(FreindlyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Freindly());
        }
    }

}

public struct Freindly : IComponentData
{

}
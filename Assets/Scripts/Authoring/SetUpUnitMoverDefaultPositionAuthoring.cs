using Unity.Entities;
using UnityEngine;

public class SetUpUnitMoverDefaultPositionAuthoring : MonoBehaviour
{
    class SetUpUnitMoverDefaultPositionAuthoringBaker : Baker<SetUpUnitMoverDefaultPositionAuthoring>
    {
        public override void Bake(SetUpUnitMoverDefaultPositionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SetupUnitMoverDefaultPosition());
        }
    }

}

public struct SetupUnitMoverDefaultPosition : IComponentData
{

}

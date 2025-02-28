using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct SetUpUnitMoverDefaultPositionSystem : ISystem
{   
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged); ;
        foreach ((RefRO<LocalTransform> localTransform, RefRW<UnitMover> unitMover, RefRO<SetupUnitMoverDefaultPosition> setUpMoverDefaultPosition, Entity entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<UnitMover>, RefRO<SetupUnitMoverDefaultPosition>>().WithEntityAccess()) 
        {
            unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
            entityCommandBuffer.RemoveComponent<SetupUnitMoverDefaultPosition>(entity);
        }
    }
}

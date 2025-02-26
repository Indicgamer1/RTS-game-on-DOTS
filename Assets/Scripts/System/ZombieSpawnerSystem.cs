using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct ZombieSpawnerSystem : ISystem
{  
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntetiesRefrences entetiesRefrences = SystemAPI.GetSingleton<EntetiesRefrences>();  
        EntityCommandBuffer entityCommandBuffer = 
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach((RefRO<LocalTransform> localTransform, RefRW<ZombieSpawner> zombieSpawner )in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>())
        {
            zombieSpawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if(zombieSpawner.ValueRO .timer > 0)
            {
                continue;
            }
            zombieSpawner.ValueRW.timer = zombieSpawner.ValueRW.timerMax;

            Entity zombieEntity = state.EntityManager.Instantiate(entetiesRefrences.zombiePrefabEntity);
            SystemAPI.SetComponent(zombieEntity,LocalTransform.FromPosition(localTransform.ValueRO.Position));

            entityCommandBuffer.AddComponent(zombieEntity, new RandomWalking
            {
                originPosition = localTransform.ValueRO.Position,
                targetPosition = localTransform.ValueRO.Position,
                distanceMin = zombieSpawner.ValueRO.randomWalkingDistanceMin,
                distanceMax = zombieSpawner.ValueRO.randomWalkingDistanceMax,
                random = new Unity.Mathematics.Random((uint)zombieEntity.Index)
            }) ;
        }
    }

}

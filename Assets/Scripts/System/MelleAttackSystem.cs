using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MelleAttackSystem : ISystem
{
   
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingeltion = SystemAPI.GetSingleton<PhysicsWorldSingleton>(); 
        CollisionWorld collisionWorld = physicsWorldSingeltion.CollisionWorld;
        NativeList<RaycastHit> rayCastHitList = new NativeList<RaycastHit>(Allocator.Temp);
        foreach ((RefRO<LocalTransform> localTransform,RefRW<MeleeAttack>melleAttack,RefRO<Target> target,RefRW<UnitMover> unitMover) in SystemAPI.Query<RefRO<LocalTransform>,RefRW<MeleeAttack>,RefRO<Target>,RefRW<UnitMover>>().WithDisabled<MoveOverride>())
        {
            if(target.ValueRO.targetEntity ==Entity.Null)
            {
                continue;
            }
            LocalTransform targetlocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            float meleeAttackDistanceSq = 2f;
            bool isCloseEnoughToAttack = math.distancesq(localTransform.ValueRO.Position, targetlocalTransform.Position) < meleeAttackDistanceSq;
            bool isTouchingTarget = false;

            if(!isCloseEnoughToAttack)
            {
                float3 dirToTarget = targetlocalTransform.Position - localTransform.ValueRO.Position;
                dirToTarget =math.normalize(dirToTarget);
                float distanceExtraToTestCast = .4f;
                RaycastInput rayCastInput = new RaycastInput             
                {
                    Start  = localTransform.ValueRO.Position,
                    End = localTransform.ValueRO.Position +dirToTarget*(melleAttack.ValueRO.colliderSize +distanceExtraToTestCast),
                    Filter = CollisionFilter.Default,
                };
                rayCastHitList.Clear();
               if( collisionWorld.CastRay(rayCastInput,ref rayCastHitList))
                {
                    foreach(RaycastHit raycastHit in rayCastHitList)
                    {
                        if(raycastHit.Entity == target.ValueRO.targetEntity)
                        {
                            isTouchingTarget = true;
                            break;
                        }
                    }
                }
            }
            if( !isCloseEnoughToAttack && !isTouchingTarget)
            {

                unitMover.ValueRW.targetPosition = targetlocalTransform.Position;
            }
            else
            {
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
                melleAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if(melleAttack.ValueRO.timer >0)
                {
                    continue;
                }
                melleAttack.ValueRW.timer = melleAttack.ValueRO.timerMax;

                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.healthAmount -= melleAttack.ValueRO.damageAmount;
                targetHealth.ValueRW.onHealthChanged = true;
            }
        }
    }

   
}

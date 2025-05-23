using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Vector3 cameraForward = Vector3.zero;
        if(Camera.main != null)
        {
            cameraForward= Camera.main.transform.forward;
        }
        foreach((RefRW<LocalTransform> localTransform,  RefRO<HealthBar> healthbar) in SystemAPI.Query<RefRW<LocalTransform>,   RefRO<HealthBar>>())
        {
            
           LocalTransform parentLocalTransform = SystemAPI.GetComponent<LocalTransform>(healthbar.ValueRO.healthEntity);
            if(localTransform.ValueRO.Scale == 1f)
            {
                // Health Bar is Visible
                localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
            }        
            Health health = SystemAPI.GetComponent<Health>(healthbar.ValueRO.healthEntity);

            if(!health.onHealthChanged)
            {
                continue; 
            }
            float healthNormalized = (float)health.healthAmount/health.healthAmountMax;

            if(healthNormalized == 1f) 
            {
                localTransform.ValueRW.Scale = 0f;
            }
            else
            {
                localTransform.ValueRW.Scale = 1f;
            }
            RefRW<PostTransformMatrix> barVisualPostTransformMatrix = SystemAPI.GetComponentRW<PostTransformMatrix>(healthbar.ValueRO.barVisualEntity);
            barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
          
        }
    }

   
}

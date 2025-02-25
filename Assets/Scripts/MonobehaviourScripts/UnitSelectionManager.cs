using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using System;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
public class UnitSelectionManager : MonoBehaviour    
{
    public static UnitSelectionManager Instance { get; private set; }

    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;

    private Vector2 selectedStartMousePosition;

    private void Awake()
    {
        Instance = this;    
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectedStartMousePosition = Input.mousePosition;
            OnSelectionAreaStart?.Invoke(this,EventArgs.Empty); 
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 selectionEndMousePosition = Input.mousePosition;

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);   
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i],false);
                  Selected selected = selectedArray[i];
                selected.onsDeselected = true;
                entityManager.SetComponentData(entityArray[i],selected);
            }

            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width * selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;

            bool isMultipleSelection = selectionAreaSize> multipleSelectionSizeMin;

            if (isMultipleSelection)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

                entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                for (int i = 0; i < localTransformArray.Length; i++)
                {
                    LocalTransform unitLocalTransform = localTransformArray[i];
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);
                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        // Unit Inside the Selection Area
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                        Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                        selected.onSelected = true;
                        entityManager.SetComponentData(entityArray[i], selected);
                    }

                }
            }
            else
            {
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));   
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();    
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
               
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNITS_LAYER,
                        GroupIndex =0,
                    }
                };

                if(collisionWorld.CastRay(raycastInput,out Unity.Physics.RaycastHit raycastHit))
                {
                    if(entityManager.HasComponent<Unit>(raycastHit.Entity) && entityManager.HasComponent<Selected>(raycastHit.Entity))
                    {
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);   
                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.onSelected = true;
                        entityManager.SetComponentData(raycastHit.Entity, selected);    
                    }
                }
            }
            OnSelectionAreaEnd?.Invoke(this,EventArgs.Empty);   
        }
        if(Input.GetMouseButtonDown(1))
        {
            Vector3 moseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover,Selected >().Build(entityManager);
            NativeArray<Entity> entityArray  = entityQuery.ToEntityArray(Allocator.Temp);    
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
            NativeArray<float3> movePositionArray = GenerateMovePosition(moseWorldPosition, entityArray.Length);
            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                UnitMover unitMover = unitMoverArray[i];
                unitMover.targetPosition = movePositionArray[i];
                unitMoverArray[i] = unitMover;

            }
            entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Input.mousePosition;

        Vector2 lowerLeftCorner = new Vector2(
            Mathf.Min(selectedStartMousePosition.x,selectionEndMousePosition.x),
            Mathf.Min(selectedStartMousePosition.y,selectionEndMousePosition.y)
            );
        Vector2 upperRightCorner = new Vector2(
           Mathf.Max(selectedStartMousePosition.x, selectionEndMousePosition.x),
           Mathf.Max(selectedStartMousePosition.y, selectionEndMousePosition.y)
           );
        return new Rect(lowerLeftCorner.x, lowerLeftCorner.y, upperRightCorner.x - lowerLeftCorner.x, upperRightCorner.y - lowerLeftCorner.y);
    }

    private NativeArray<float3> GenerateMovePosition(float3 targetPosition, int positionCount)
    {
     NativeArray<float3> PositionArray = new NativeArray<float3>(positionCount,Allocator.Temp);

        if(positionCount == 0) 
        {
            return PositionArray;
        }
        PositionArray[0] = targetPosition;
        if(positionCount == 1)
        {
            return PositionArray;
        }
        float ringSize = 2.2f;
        int ring = 0;
        int positionIndex = 1;

        while (positionIndex < positionCount)
        {
            int ringPositionCount = 3 + ring * 2;
            for(int i = 0; i < ringPositionCount; i++)
            {
                float angle = i*(math.PI/2/ringPositionCount);
                float3 ringVector = math.rotate(quaternion.RotateY(angle) , new float3(ringSize * (ring + 1),0,0));
                float3 ringPosition = targetPosition +ringVector;

                PositionArray[positionIndex] = ringPosition;
                positionIndex++;

                if(positionIndex>= positionCount)
                {
                    break;
                }
            }
            ring++;
        }
        return PositionArray;
    }
}

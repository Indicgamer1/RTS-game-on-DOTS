using Unity.Entities;
using UnityEngine;

public class EntitiesRefrencesAuthoring : MonoBehaviour
{
    public GameObject bulletPrefabGameObject;
    public GameObject zombiePrefabGameObject;
    public GameObject shootLightPrefabGameObject;
    public class EntitiesRefrencesAuthoringBaker : Baker<EntitiesRefrencesAuthoring>
    {
        public override void Bake(EntitiesRefrencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntetiesRefrences
            {
                bulletPrefabEntity = GetEntity(authoring.bulletPrefabGameObject,TransformUsageFlags.Dynamic),
                zombiePrefabEntity = GetEntity(authoring.zombiePrefabGameObject, TransformUsageFlags.Dynamic),
                shootLightPrefabEntity = GetEntity(authoring.shootLightPrefabGameObject,TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct EntetiesRefrences : IComponentData
{
    public Entity bulletPrefabEntity;
    public Entity zombiePrefabEntity;
    public Entity shootLightPrefabEntity;
}

using Unity.Entities;

namespace Game.ECS
{
    public class GameManagerAutoring : AutoAuthoring.AutoAuthoring<GameManager> { }

    [System.Serializable]
    public struct GameManager : IComponentData
    {
        public Entity prefab;
        public Entity turret;
        public int countSpawn;
    }
}
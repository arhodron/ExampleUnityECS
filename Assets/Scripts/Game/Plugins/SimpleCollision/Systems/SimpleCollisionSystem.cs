using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Burst.Intrinsics;
using Unity.Profiling;

namespace Game.ECS.SimpleCollision
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    [UpdateInGroup(typeof(CollisionSystemGroup), OrderFirst = true)]
    public partial struct SimpleCollisionSystem : ISystem
    {
        static readonly ProfilerMarker profileCollisions = new ProfilerMarker(nameof(SimpleCollisionSystem));

        private EntityQuery collisionQuery;
        private EntityQuery coliderQuery;

        private ComponentLookup<LocalToWorld> localToWorldLookup;
        private ComponentLookup<Collider> colliderLookup;

        public ComponentTypeHandle<LocalToWorld> localToWorldTypeHandle;
        public ComponentTypeHandle<Collision> collisionsTypeHandle;
        public ComponentTypeHandle<Collider> colliderTypeHandle;
        public ComponentTypeHandle<Hit> hitTypeHandle;
        public EntityTypeHandle EntityTypeHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Collision, LocalToWorld>().WithPresent<Hit>().WithNone<DestroyEvent>();
            collisionQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Collider, LocalToWorld>().WithNone<DestroyEvent>();
            coliderQuery = state.GetEntityQuery(builder);

            localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
            colliderLookup = SystemAPI.GetComponentLookup<Collider>(true);

            localToWorldTypeHandle = SystemAPI.GetComponentTypeHandle<LocalToWorld>(true);
            collisionsTypeHandle = SystemAPI.GetComponentTypeHandle<Collision>(true);
            colliderTypeHandle = SystemAPI.GetComponentTypeHandle<Collider>();
            hitTypeHandle = SystemAPI.GetComponentTypeHandle<Hit>();
            EntityTypeHandle = SystemAPI.GetEntityTypeHandle();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            profileCollisions.Begin();

            if (collisionQuery.IsEmpty || coliderQuery.IsEmpty)
                return;

#if true
            localToWorldLookup.Update(ref state);
            colliderLookup.Update(ref state);

            var colliders = coliderQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.ToAllocator);

            var job = new CollisionJob
            {
                colliders = colliders,
                localToWorldLookup = localToWorldLookup,
                colliderLookup = colliderLookup,
            };
            job.ScheduleParallel(collisionQuery, state.Dependency).Complete();

            colliders.Dispose();
#else
            localToWorldTypeHandle.Update(ref state);
            collisionsTypeHandle.Update(ref state);
            colliderTypeHandle.Update(ref state);
            hitTypeHandle.Update(ref state);
            EntityTypeHandle.Update(ref state);

            new CollisionChunkJob
            {
                //localToWorldTypeHandle = SystemAPI.GetComponentTypeHandle<LocalToWorld>(true),
                //collisionsTypeHandle = SystemAPI.GetComponentTypeHandle<Collision>(true),
                //colliderTypeHandle = SystemAPI.GetComponentTypeHandle<Collider>(),
                //hitTypeHandle = SystemAPI.GetComponentTypeHandle<Hit>(),
                //EntityTypeHandle = SystemAPI.GetEntityTypeHandle(),

                localToWorldTypeHandle = localToWorldTypeHandle,
                collisionsTypeHandle = collisionsTypeHandle,
                colliderTypeHandle = colliderTypeHandle,
                hitTypeHandle = hitTypeHandle,
                EntityTypeHandle = EntityTypeHandle,

                colliderChunks = coliderQuery.ToArchetypeChunkArray(state.WorldUpdateAllocator)
            }.ScheduleParallel(collisionQuery, state.Dependency).Complete();
#endif

            profileCollisions.End();
        }

        [BurstCompile]
        public partial struct CollisionJob : IJobEntity
        {
            [ReadOnly]
            public NativeArray<Entity> colliders;
            [ReadOnly]
            public ComponentLookup<LocalToWorld> localToWorldLookup;
            [ReadOnly]
            public ComponentLookup<Collider> colliderLookup;

            [BurstCompile]
            public void Execute(in Entity entity, in Collision collision, ref Hit hit, EnabledRefRW<Hit> enabled, in LocalToWorld localToWorld)
            {
                float3 position = localToWorld.Position;
                float radius = collision.radius;

                for (int i = 0, count = colliders.Length; i < count; i++)
                {
                    Entity ent = colliders[i];
                    if (ent == entity)
                        break;

                    float3 pos = localToWorldLookup[ent].Position;
                    float rad = colliderLookup[ent].radius;

                    if (math.distance(position, pos) < (radius + rad))
                    {
                        hit.value = ent;
                        enabled.ValueRW = true;
                        return;
                    }
                }

                hit.value = default;
                enabled.ValueRW = false;
            }
        }

        [BurstCompile]
        public struct CollisionChunkJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> localToWorldTypeHandle;
            [ReadOnly] public ComponentTypeHandle<Collision> collisionsTypeHandle;
            [ReadOnly] public ComponentTypeHandle<Collider> colliderTypeHandle;
            public ComponentTypeHandle<Hit> hitTypeHandle;
            [ReadOnly] public EntityTypeHandle EntityTypeHandle;

            [ReadOnly] public NativeArray<ArchetypeChunk> colliderChunks;

            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                var transforms = chunk.GetNativeArray(ref localToWorldTypeHandle);
                var collisions = chunk.GetNativeArray(ref collisionsTypeHandle);
                var entities = chunk.GetNativeArray(EntityTypeHandle);
                var hits = chunk.GetNativeArray(ref hitTypeHandle);
                bool isHit;

                for (int i = 0; i < transforms.Length; i++)
                {
                    var transform = transforms[i];
                    var collision = collisions[i];
                    var entity = entities[i];
                    float radius = collision.radius;

                    var hit = hits[i];

                    isHit = false;
                    for (int j = 0; j < colliderChunks.Length; j++)
                    {
                        var colliderChunk = colliderChunks[j];
                        var colliderTransforms = colliderChunk.GetNativeArray(ref localToWorldTypeHandle);
                        var colliders = colliderChunk.GetNativeArray(ref colliderTypeHandle);
                        var colliderEntities = colliderChunk.GetNativeArray(EntityTypeHandle);

                        for (int k = 0; k < colliderChunk.Count; k++)
                        {
                            var otherTransformn = colliderTransforms[k];
                            var collider = colliders[k];
                            var colliderEntity = colliderEntities[k];
                            float rad = collider.radius;

                            if (entity != colliderEntity && math.distancesq(transform.Position, otherTransformn.Position) < radius + rad)
                            {
                                hit.value = colliderEntity;
                                hits[i] = hit;
                                chunk.SetComponentEnabled(ref hitTypeHandle, i, true);
                                isHit = true;
                                break;
                            }
                        }

                        if(isHit)
                            break;
                    }

                    if(!isHit)
                        chunk.SetComponentEnabled(ref hitTypeHandle, i, false);
                }
            }
        }
    }
}

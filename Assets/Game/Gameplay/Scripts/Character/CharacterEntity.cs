using Game.GameEngine.Ecs;
using SampleProject;
using UnityEngine;
using UnityEngine.AI;

namespace Entities
{
    public abstract class CharacterEntity : Entity
    {
        [SerializeField]
        private CharacterConfig config;

        protected override void Init()
        {
            this.SetData(new SmoothRotationComponent());

            this.SetData(new CombatComponent
            {
                damage = this.config.damage,
                minDistance = config.minDistance,
                animationTime = this.config.animationTime,
                timeBetweenAttack = this.config.timeBetweenAttack,
                damageType = this.config.damageType
            });
            
            this.SetData(new AnimatorComponent
            {
                value = this.GetComponentInChildren<AnimatorMachine>()
            });
            
            this.SetData(new HitPointsComponent
            {
                max = this.config.hitPoints,
                current = this.config.hitPoints
            });

            this.SetData(new MoveSpeedComponent
            {
                value = this.config.moveSpeed
            });

            this.SetData(new TransformComponent
            {
                value = this.transform,
                radius = this.config.radius
            });

            this.SetData(new GameObjectComponent
            {
                value = this.gameObject
            });

            this.SetData(new RigidbodyComponent
            {
                value = this.GetComponent<Rigidbody>()
            });

            NavMeshAgent navMeshAgent = this.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                this.SetData(new NavMeshAgentComponent
                {
                    value = navMeshAgent
                });

                // Настройка скорости из конфига
                ref var speed = ref this.GetData<MoveSpeedComponent>();
                navMeshAgent.speed = speed.value;

                // Автоповорот через NavMeshAgent
                navMeshAgent.autoBraking = true;
                navMeshAgent.updateRotation = true;
                navMeshAgent.updatePosition = true;
                navMeshAgent.angularSpeed = 360f;
                navMeshAgent.acceleration = 20f;
            }
            else
            {
                Debug.LogError($"NavMeshAgent not found on {this.name}!");
            }


            this.SetData(new RendererComponent
            {
                value = this.GetComponentInChildren<Renderer>()
            });

            InitCharacter();
        }

        protected virtual void InitCharacter() { }

        public virtual void TakeDamage(int damage)
        {
            if (this.HasData<HitPointsComponent>())
            {
                ref var hp = ref this.GetData<HitPointsComponent>();
                hp.current -= damage;

                if (hp.current <= 0)
                {
                    Die();
                }
            }
        }

        protected virtual void Die()
        {
            this.SendEvent(new DestroyEvent());
        }
    }
}
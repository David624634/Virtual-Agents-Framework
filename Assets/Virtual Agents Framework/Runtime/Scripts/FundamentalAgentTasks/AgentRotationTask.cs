using System.Collections;
using UnityEngine;

namespace i5.VirtualAgents.AgentTasks
{
    /// <summary>
    /// Defines rotation tasks for rotating the agent to a specific direction.
    /// The direction can be given as a target, coordinates or angle.
    /// </summary>
    public class AgentRotationTask : AgentBaseTask, ISerializable
    {
        /// <summary>
        /// The rotation as a quaternion which the agent should rotate to
        /// </summary>
        public Quaternion TargetRotation { get; protected set; }

        /// <summary>
        /// Used to determine if the agent should rotate by a specific angle
        /// </summary>
        public bool IsRotationByAngle { get; protected set; }

        /// <summary>
        /// Used to determine if the agent should rotate towards a specific angle.
        /// </summary>
        public bool IsRotationTowardsAngle { get; protected set; }

        /// <summary>
        /// The angle the agent should rotate by or towards
        /// </summary>
        public float Angle { get; protected set; }

        /// <summary>
        /// The transform the agent should rotate towards
        /// </summary>
        public Transform TargetTransform { get; protected set; }

        /// <summary>
        /// The speed at which the agent should rotate
        /// </summary>
        public float Speed { get; protected set; }

        /// <summary>
        /// The angle difference at which the task is considered finished
        /// </summary>
        public float AngleThreshold = 0.03f;

        /// <summary>
        /// Create an AgentRotationTask using a target object to turn towards, position will be evaluated when task is started
        /// </summary>
        /// <param name="target">Target object of the rotation task</param>
        public AgentRotationTask(GameObject target, float speed = 0.8f)
        {
            TargetTransform = target.transform;
            IsRotationByAngle = false;
            Speed = speed;
        }

        /// <summary>
        /// Create an AgentRotationTask using the destination coordinates
        /// </summary>
        /// <param name="coordinates">Coordinates of the rotation task</param>
        public AgentRotationTask(Vector3 coordinates, float speed = 0.8f)
        {
            TargetTransform = new GameObject().transform;
            TargetTransform.position = coordinates;
            IsRotationByAngle = false;
            Speed = speed;
        }

        /// <summary>
        /// Create an AgentRotationTask using the angle that the agent should rotate by.
        /// Positive angle turns right, negative angle turns left.
        /// When isRotationByAngle is set to false, the agents rotation attribute will be set to the angle specified instead.
        /// In this case the agent rotates in the direction that minimises the distance.
        /// </summary>
        /// <param name="angle">The angle to rotate by or towards, in degrees</param>
        /// <param name="isRotationByAngle">True if agent should rotate by "angle" degrees, false if the rotation value of the agent should be set to "angle"</param>
        public AgentRotationTask(float angle, bool isRotationByAngle = true, float speed = 0.8f)
        {
            IsRotationByAngle = isRotationByAngle;
            if (!isRotationByAngle)
            {
                TargetRotation = Quaternion.Euler(0, angle, 0);
                IsRotationTowardsAngle = true;
            }
            else
            {
                Angle = angle;
            }
            Speed = speed;
        }

        /// <summary>
        /// Start the rotation
        /// Called by the agent
        /// </summary>
        /// <param name="agent">The agent which executes this task</param>
        public override void StartExecution(Agent agent)
        {
            Animator animator = agent.GetComponent<Animator>();
            base.StartExecution(agent);

            // For target and coordinates rotation
            if (!IsRotationTowardsAngle && !IsRotationByAngle)
            {
                Vector3 newTargetPosition = new Vector3(TargetTransform.position.x, 0, TargetTransform.position.z);
                Vector3 newAgentPosition = new Vector3(agent.transform.position.x, 0, agent.transform.position.z);
                float angle = Vector3.SignedAngle(agent.transform.forward, newTargetPosition - newAgentPosition, Vector3.up);
                TargetRotation = agent.transform.rotation * Quaternion.Euler(0, angle, 0);
            }

            //For Angle rotation
            if (IsRotationByAngle)
            {
                TargetRotation = agent.transform.rotation * Quaternion.Euler(0, Angle, 0);
            }
            agent.StartCoroutine(Rotate(agent.transform));
        }

        private IEnumerator Rotate(Transform transform)
        {
            float time = 0;
            while (Quaternion.Angle(transform.rotation, TargetRotation) > AngleThreshold)
            {
                time += Time.deltaTime * Speed; //to control the speed of rotation
                // Rotate the agent a step closer to the target
                transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, time);
                yield return null;
            }
            if (Quaternion.Angle(transform.rotation, TargetRotation) <= AngleThreshold)
            {
                FinishTask();
            }
            else
            {
                FinishTaskAsFailed();
            }
        }

        public void Serialize(SerializationDataContainer serializer)
        {
            serializer.AddSerializedData("Target Rotation", TargetRotation);
            serializer.AddSerializedData("Is Rotation By Angle", IsRotationByAngle);
            serializer.AddSerializedData("Angle", Angle);
            serializer.AddSerializedData("Speed", Speed);
        }

        public void Deserialize(SerializationDataContainer serializer)
        {
            TargetRotation = serializer.GetSerializedQuaternion("Target Rotation");
            IsRotationByAngle = serializer.GetSerializedBool("Is Rotation By Angle");
            Angle = serializer.GetSerializedFloat("Angle");
            Speed = serializer.GetSerializedFloat("Speed");
        }
    }
}

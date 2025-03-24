using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RVO
{
    public class RVOSimulator
    {
        public const int RVO3D_ERROR = int.MaxValue;

        internal Agent defaultAgent_;
        internal KdTree kdTree_;
        internal float globalTime_ = 0.0f;
        internal float timeStep_;
        internal List<Agent> agents_;


        public RVOSimulator(float timeStep, float neighborDist, int maxNeighbors,
             float timeHorizon, float radius, float maxSpeed, Vector3 velocity = new Vector3())
        {
            agents_ = new List<Agent>();
            defaultAgent_ = new Agent(this);
            kdTree_ = new KdTree(this);
            timeStep_ = timeStep;
            defaultAgent_.maxNeighbors_ = maxNeighbors;
            defaultAgent_.maxSpeed_ = maxSpeed;
            defaultAgent_.neighborDist_ = neighborDist;
            defaultAgent_.radius_ = radius;
            defaultAgent_.timeHorizon_ = timeHorizon;
            defaultAgent_.velocity_ = velocity;
        }

        public float getGlobalTime()
        {
            return globalTime_;
        }

        public int getNumAgents()
        {
            return agents_.Count;
        }

        public float getTimeStep()
        {
            return timeStep_;
        }

        public int getAgentNumAgentNeighbors(int agentNo)
        {
            return agents_[agentNo].agentNeighbors_.Count;
        }

        public int getAgentAgentNeighbor(int agentNo, int neighborNo)
        {
            return agents_[agentNo].agentNeighbors_[neighborNo].Item2.id_;
        }

        public int getAgentNumORCAPlanes(int agentNo)
        {
            return agents_[agentNo].orcaPlanes_.Count;
        }

        public Plane getAgentORCAPlane(int agentNo, int planeNo)
        {
            return agents_[agentNo].orcaPlanes_[planeNo];
        }

        public void removeAgent(int agentNo)
        {
            agents_[agentNo] = agents_[agents_.Count - 1];
            agents_.RemoveAt(agents_.Count - 1);
        }

        public int addAgent(Vector3 position, Vector3 color)
        {
            if (defaultAgent_ == null)
            {
                return RVO3D_ERROR;
            }

            Agent agent = new Agent(this);

            agent.color_ = color;
            agent.position_ = position;
            agent.maxNeighbors_ = defaultAgent_.maxNeighbors_;
            agent.maxSpeed_ = defaultAgent_.maxSpeed_;
            agent.neighborDist_ = defaultAgent_.neighborDist_;
            agent.radius_ = defaultAgent_.radius_;
            agent.timeHorizon_ = defaultAgent_.timeHorizon_;
            agent.velocity_ = defaultAgent_.velocity_;

            agent.id_ = agents_.Count;

            agents_.Add(agent);

            return agents_.Count - 1;
        }

        public int addAgent(Vector3 position, float neighborDist,
                                           int maxNeighbors, float timeHorizon,
                                           float radius, float maxSpeed,
                                           Vector3 velocity, Vector3 color)
        {
            Agent agent = new Agent(this);

            agent.color_ = color;
            agent.position_ = position;
            agent.maxNeighbors_ = maxNeighbors;
            agent.maxSpeed_ = maxSpeed;
            agent.neighborDist_ = neighborDist;
            agent.radius_ = radius;
            agent.timeHorizon_ = timeHorizon;
            agent.velocity_ = velocity;

            agent.id_ = agents_.Count;

            agents_.Add(agent);

            return agents_.Count - 1;
        }

        public void doStep()
        {
            kdTree_.buildAgentTree();

            for (int i = 0; i < agents_.Count; ++i)
            {
                agents_[i].computeNeighbors();
                agents_[i].computeNewVelocity();
            }

            for (int i = 0; i < agents_.Count; ++i)
            {
                agents_[i].update();
            }

            globalTime_ += timeStep_;
        }

        public float getAgentFreezed(int agentNo)
        {
            return agents_[agentNo].freezed_;
        }

        public int getAgentMaxNeighbors(int agentNo)
        {
            return agents_[agentNo].maxNeighbors_;
        }

        public float getAgentMaxSpeed(int agentNo)
        {
            return agents_[agentNo].maxSpeed_;
        }

        public float getAgentNeighborDist(int agentNo)
        {
            return agents_[agentNo].neighborDist_;
        }

        public Vector3 getAgentPosition(int agentNo)
        {
            return agents_[agentNo].position_;
        }

        public Vector3 getAgentColor(int agentNo)
        {
            return agents_[agentNo].color_;
        }

        public Vector3 getAgentPrefVelocity(int agentNo)
        {
            return agents_[agentNo].prefVelocity_;
        }

        public float getAgentRadius(int agentNo)
        {
            return agents_[agentNo].radius_;
        }

        public float getAgentTimeHorizon(int agentNo)
        {
            return agents_[agentNo].timeHorizon_;
        }

        public Vector3 getAgentNewVelocity(int agentNo)
        {
            return agents_[agentNo].newVelocity_;
        }

        public Vector3 getAgentVelocity(int agentNo)
        {
            return agents_[agentNo].velocity_;
        }

        public Quaternion getAgentRotate(int agentNo)
        {
            return agents_[agentNo].rot_;
        }

        public Vector3 getAgentGoal(int agentNo)
        {
            return agents_[agentNo].goal_;
        }

        public void setAgentDefaults(float neighborDist,
                                            int maxNeighbors, float timeHorizon,
                                            float radius, float maxSpeed,
                                            Vector3 velocity)
        {
            if (defaultAgent_ == null)
            {
                defaultAgent_ = new Agent(this);
            }

            defaultAgent_.maxNeighbors_ = maxNeighbors;
            defaultAgent_.maxSpeed_ = maxSpeed;
            defaultAgent_.neighborDist_ = neighborDist;
            defaultAgent_.radius_ = radius;
            defaultAgent_.timeHorizon_ = timeHorizon;
            defaultAgent_.velocity_ = velocity;
        }

        public void setAgentMaxNeighbors(int agentNo, int maxNeighbors)
        {
            agents_[agentNo].maxNeighbors_ = maxNeighbors;
        }

        public void setAgentMaxSpeed(int agentNo, float maxSpeed)
        {
            agents_[agentNo].maxSpeed_ = maxSpeed;
        }

        public void setAgentNeighborDist(int agentNo, float neighborDist)
        {
            agents_[agentNo].neighborDist_ = neighborDist;
        }

        public void setAgentPosition(int agentNo, Vector3 position)
        {
            agents_[agentNo].position_ = position;
        }

        public void setAgentPrefVelocity(int agentNo, Vector3 prefVelocity)
        {
            agents_[agentNo].prefVelocity_ = prefVelocity;
        }

        public void setAgentRadius(int agentNo, float radius)
        {
            agents_[agentNo].radius_ = radius;
        }

        public void setAgentTimeHorizon(int agentNo, float timeHorizon)
        {
            agents_[agentNo].timeHorizon_ = timeHorizon;
        }

        public void setAgentVelocity(int agentNo, Vector3 velocity)
        {
            agents_[agentNo].velocity_ = velocity;
        }

        public void setAgentNewVelocity(int agentNo, Vector3 velocity)
        {
            agents_[agentNo].newVelocity_ = velocity;
        }

        public void setAgentFreezed(int agentNo, float freezed)
        {
            agents_[agentNo].freezed_ = freezed;
        }

        public void setAgentGoal(int agentNo, Vector3 goal)
        {
            agents_[agentNo].goal_ = goal;
        }

        public void setAgentBeginFreezedTime(int agentNo, float time)
        {
            agents_[agentNo].beginFreezedTime = time;
        }

        public float getAgentBeginFreezedTime(int agentNo)
        {
            return agents_[agentNo].beginFreezedTime;
        }
    }
}
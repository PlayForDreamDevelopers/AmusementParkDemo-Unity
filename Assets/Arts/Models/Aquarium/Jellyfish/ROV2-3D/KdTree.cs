using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RVO
{
    public class KdTree
    {

        public const int RVO3D_MAX_LEAF_SIZE = 10;

        private struct AgentTreeNode
        {
            public int begin;

            /**
             * @brief The ending node number.
             */
            public int end;

            /**
             * @brief The left node number.
             */
            public int left;

            /**
             * @brief The right node number.
             */
            public int right;

            /**
             * @brief The maximum coordinates.
             */
            public Vector3 maxCoord;

            /**
             * @brief The minimum coordinates.
             */
            public Vector3 minCoord;
        }

        private List<Agent> agents_;
        private AgentTreeNode[] agentTree_;
        private RVOSimulator sim_;

        public KdTree(RVOSimulator sim)
        {
            sim_ = sim;
        }

        internal void buildAgentTree()
        {
            agents_ = sim_.agents_;

            if (agents_.Count > 0)
            {
                if (agentTree_ == null)
                {
					agentTree_ = new AgentTreeNode[2 * agents_.Count - 1];
				}
                buildAgentTreeRecursive(0, agents_.Count, 0);
            }
        }

        internal void swapAgent(int a, int b)
        {
            var temp = agents_[a];
            agents_[a] = agents_[b];
            agents_[b] = temp;
        }

        internal void buildAgentTreeRecursive(int begin, int end,
                                     int node)
        {
            agentTree_[node].begin = begin;
            agentTree_[node].end = end;
            agentTree_[node].minCoord = agents_[begin].position_;
            agentTree_[node].maxCoord = agents_[begin].position_;

            for (int i = begin + 1; i < end; ++i)
            {
                agentTree_[node].maxCoord[0] =
                    Mathf.Max(agentTree_[node].maxCoord[0], agents_[i].position_.x());
                agentTree_[node].minCoord[0] =
                    Mathf.Min(agentTree_[node].minCoord[0], agents_[i].position_.x());
                agentTree_[node].maxCoord[1] =
                    Mathf.Max(agentTree_[node].maxCoord[1], agents_[i].position_.y());
                agentTree_[node].minCoord[1] =
                    Mathf.Min(agentTree_[node].minCoord[1], agents_[i].position_.y());
                agentTree_[node].maxCoord[2] =
                    Mathf.Max(agentTree_[node].maxCoord[2], agents_[i].position_.z());
                agentTree_[node].minCoord[2] =
                    Mathf.Min(agentTree_[node].minCoord[2], agents_[i].position_.z());
            }

            if (end - begin > RVO3D_MAX_LEAF_SIZE)
            {
                /* No leaf node. */
                int coord = 0;

                if (agentTree_[node].maxCoord[0] - agentTree_[node].minCoord[0] >
                        agentTree_[node].maxCoord[1] - agentTree_[node].minCoord[1] &&
                    agentTree_[node].maxCoord[0] - agentTree_[node].minCoord[0] >
                        agentTree_[node].maxCoord[2] - agentTree_[node].minCoord[2])
                {
                    coord = 0;
                }
                else if (agentTree_[node].maxCoord[1] - agentTree_[node].minCoord[1] >
                         agentTree_[node].maxCoord[2] - agentTree_[node].minCoord[2])
                {
                    coord = 1;
                }
                else
                {
                    coord = 2;
                }

                float splitValue = 0.5f * (agentTree_[node].maxCoord[coord] +
                                                 agentTree_[node].minCoord[coord]);

                int left = begin;

                int right = end;

                while (left < right)
                {
                    while (left < right && agents_[left].position_[coord] < splitValue)
                    {
                        ++left;
                    }

                    while (right > left &&
                           agents_[right - 1].position_[coord] >= splitValue)
                    {
                        --right;
                    }

                    if (left < right)
                    {
                        swapAgent(left, right - 1);
                        ++left;
                        --right;
                    }
                }

                int leftSize = left - begin;

                if (leftSize == 0U)
                {
                    ++leftSize;
                    ++left;
                }

                agentTree_[node].left = node + 1;
                agentTree_[node].right = node + 2 * leftSize;

                buildAgentTreeRecursive(begin, left, agentTree_[node].left);
                buildAgentTreeRecursive(left, end, agentTree_[node].right);
            }
        }


        internal void computeAgentNeighbors(Agent agent, float rangeSq)
        {
            queryAgentTreeRecursive(agent, ref rangeSq, 0);
        }

        internal void queryAgentTreeRecursive(Agent agent, ref float rangeSq, int node)
        {
            if (agentTree_[node].end - agentTree_[node].begin <= RVO3D_MAX_LEAF_SIZE)
            {
                for (int i = agentTree_[node].begin; i < agentTree_[node].end; ++i)
                {
                    agent.insertAgentNeighbor(agents_[i], ref rangeSq);
                }
            }
            else
            {
                float distSqLeftMinX =
                    Mathf.Max(0.0F, agentTree_[agentTree_[node].left].minCoord[0] -
                                   agent.position_.x());
                float distSqLeftMaxX =
                    Mathf.Max(0.0F, agent.position_.x() -
                                   agentTree_[agentTree_[node].left].maxCoord[0]);
                float distSqLeftMinY =
                    Mathf.Max(0.0F, agentTree_[agentTree_[node].left].minCoord[1] -
                                       agent.position_.y());
                float distSqLeftMaxY =
                    Mathf.Max(0.0F, agent.position_.y() -
                                       agentTree_[agentTree_[node].left].maxCoord[1]);
                float distSqLeftMinZ =
                    Mathf.Max(0.0F, agentTree_[agentTree_[node].left].minCoord[2] -
                                       agent.position_.z());
                float distSqLeftMaxZ =
                    Mathf.Max(0.0F, agent.position_.z() -
                                       agentTree_[agentTree_[node].left].maxCoord[2]);

                float distSqLeft =
                    distSqLeftMinX * distSqLeftMinX + distSqLeftMaxX * distSqLeftMaxX +
                    distSqLeftMinY * distSqLeftMinY + distSqLeftMaxY * distSqLeftMaxY +
                    distSqLeftMinZ * distSqLeftMinZ + distSqLeftMaxZ * distSqLeftMaxZ;

                float distSqRightMinX =
                    Mathf.Max(0.0F, agentTree_[agentTree_[node].right].minCoord[0] -
                                       agent.position_.x());
                float distSqRightMaxX =
                    Mathf.Max(0.0F, agent.position_.x() -
                                       agentTree_[agentTree_[node].right].maxCoord[0]);
                float distSqRightMinY =
                    Mathf.Max(0.0F, agentTree_[agentTree_[node].right].minCoord[1] -
                                       agent.position_.y());
                float distSqRightMaxY =
                    Mathf.Max(0.0F, agent.position_.y() -
                                       agentTree_[agentTree_[node].right].maxCoord[1]);
                float distSqRightMinZ =
                    Mathf.Max(0.0F, agentTree_[agentTree_[node].right].minCoord[2] -
                                       agent.position_.z());
                float distSqRightMaxZ =
                    Mathf.Max(0.0F, agent.position_.z() -
                                       agentTree_[agentTree_[node].right].maxCoord[2]);

                float distSqRight =
                    distSqRightMinX * distSqRightMinX + distSqRightMaxX * distSqRightMaxX +
                    distSqRightMinY * distSqRightMinY + distSqRightMaxY * distSqRightMaxY +
                    distSqRightMinZ * distSqRightMinZ + distSqRightMaxZ * distSqRightMaxZ;

                if (distSqLeft < distSqRight)
                {
                    if (distSqLeft < rangeSq)
                    {
                        queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].left);

                        if (distSqRight < rangeSq)
                        {
                            queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].right);
                        }
                    }
                }
                else
                {
                    if (distSqRight < rangeSq)
                    {
                        queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].right);

                        if (distSqLeft < rangeSq)
                        {
                            queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].left);
                        }
                    }
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RVO
{
    public struct Line
    {
        public Vector3 direction;
        public Vector3 point;
    }

    public struct Plane
    {
        public Vector3 point;
        public Vector3 normal;
    }

    public class RVOMath
    {
        public const float RVO3D_EPSILON = 0.00001F;

        public static float absSq(Vector3 vector3)
        {
            return vector3 * vector3;
        }

        public static float absSq(ref Vector3 vector3)
        {
            return vector3 * vector3;
        }

        public static float abs(ref Vector3 vector3)
        {
            return Mathf.Sqrt(vector3 * vector3);
        }

        public static float abs(Vector3 vector3)
        {
            return Mathf.Sqrt(vector3 * vector3);
        }

        public static Vector3 cross(Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(vector1[1] * vector2[2] - vector1[2] * vector2[1],
                               vector1[2] * vector2[0] - vector1[0] * vector2[2],
                               vector1[0] * vector2[1] - vector1[1] * vector2[0]);
        }

        public static Vector3 normalize(ref Vector3 vector)
        {
            return vector / abs(ref vector);
        }

        public static Vector3 normalize(Vector3 vector)
        {
            return vector / abs(ref vector);
        }

        public static bool linearProgram1(IList<Plane> planes, int planeNo,
                        Line line, float radius, Vector3 optVelocity,
                        bool directionOpt,
                        ref Vector3 result)
        {   /* NOLINT(runtime/references) */
            float dotProduct = line.point * line.direction;
            float discriminant =
                dotProduct * dotProduct + radius * radius - absSq(ref line.point);

            if (discriminant < 0.0f)
            {
                /* Max speed sphere fully invalidates line. */
                return false;
            }

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float tLeft = -dotProduct - sqrtDiscriminant;
            float tRight = -dotProduct + sqrtDiscriminant;

            for (int i = 0; i < planeNo; ++i)
            {
                float numerator = (planes[i].point - line.point) * planes[i].normal;
                float denominator = line.direction * planes[i].normal;

                if (denominator * denominator <= RVO3D_EPSILON)
                {
                    /* Lines line is (almost) parallel to plane i. */
                    if (numerator > 0.0f)
                    {
                        return false;
                    }

                    continue;
                }

                float t = numerator / denominator;

                if (denominator >= 0.0f)
                {
                    /* Plane i bounds line on the left. */
                    tLeft = Mathf.Max(tLeft, t);
                }
                else
                {
                    /* Plane i bounds line on the right. */
                    tRight = Mathf.Min(tRight, t);
                }

                if (tLeft > tRight)
                {
                    return false;
                }
            }

            if (directionOpt)
            {
                /* Optimize direction. */
                if (optVelocity * line.direction > 0.0f)
                {
                    /* Take right extreme. */
                    result = line.point + tRight * line.direction;
                }
                else
                {
                    /* Take left extreme. */
                    result = line.point + tLeft * line.direction;
                }
            }
            else
            {
                /* Optimize closest point. */
                float t = line.direction * (optVelocity - line.point);

                if (t < tLeft)
                {
                    result = line.point + tLeft * line.direction;
                }
                else if (t > tRight)
                {
                    result = line.point + tRight * line.direction;
                }
                else
                {
                    result = line.point + t * line.direction;
                }
            }

            return true;
        }

        /**
         * @brief      Solves a two-dimensional linear program on a specified plane
         *             subject to linear constraints defined by planes and a spherical
         *             constraint.
         * @param[in]  planes       Planes defining the linear constraints.
         * @param[in]  planeNo      The plane on which the two-dimensional linear
         *                          program is solved.
         * @param[in]  radius       The radius of the spherical constraint.
         * @param[in]  optVelocity  The optimization velocity.
         * @param[in]  directionOpt True if the direction should be optimized.
         * @param[out] result       A reference to the result of the linear program.
         * @return     True if successful.
         */
        public static bool linearProgram2(IList<Plane> planes, int planeNo,
                            float radius, Vector3 optVelocity, bool directionOpt,
                            ref Vector3 result)
        {   /* NOLINT(runtime/references) */
            float planeDist = planes[planeNo].point * planes[planeNo].normal;
            float planeDistSq = planeDist * planeDist;
            float radiusSq = radius * radius;

            if (planeDistSq > radiusSq)
            {
                /* Max speed sphere fully invalidates plane planeNo. */
                return false;
            }

            float planeRadiusSq = radiusSq - planeDistSq;

            Vector3 planeCenter = planeDist * planes[planeNo].normal;

            if (directionOpt)
            {
                /* Project direction optVelocity on plane planeNo. */
                Vector3 planeOptVelocity =
                    optVelocity -
                    (optVelocity * planes[planeNo].normal) * planes[planeNo].normal;
                float planeOptVelocityLengthSq = absSq(ref planeOptVelocity);

                if (planeOptVelocityLengthSq <= RVO3D_EPSILON)
                {
                    result = planeCenter;
                }
                else
                {
                    result =
                        planeCenter + Mathf.Sqrt(planeRadiusSq / planeOptVelocityLengthSq) *
                                          planeOptVelocity;
                }
            }
            else
            {
                /* Project point optVelocity on plane planeNo. */
                result = optVelocity +
                         ((planes[planeNo].point - optVelocity) * planes[planeNo].normal) *
                             planes[planeNo].normal;

                /* If outside planeCircle, project on planeCircle. */
                if (absSq(ref result) > radiusSq)
                {
                    Vector3 planeResult = result - planeCenter;
                    float planeResultLengthSq = absSq(ref planeResult);
                    result = planeCenter +
                             Mathf.Sqrt(planeRadiusSq / planeResultLengthSq) * planeResult;
                }
            }

            for (int i = 0; i < planeNo; ++i)
            {
                if (planes[i].normal * (planes[i].point - result) > 0.0f)
                {
                    /* Result does not satisfy constraint i. Compute new optimal result.
                     * Compute intersection line of plane i and plane planeNo.
                     */
                    Vector3 crossProduct = cross(planes[i].normal, planes[planeNo].normal);

                    if (absSq(ref crossProduct) <= RVO3D_EPSILON)
                    {
                        /* Planes planeNo and i are (almost) parallel, and plane i fully
                         * invalidates plane planeNo.
                         */
                        return false;
                    }

                    Line line;
                    line.direction = normalize(crossProduct);
                    Vector3 lineNormal = cross(line.direction, planes[planeNo].normal);
                    line.point =
                        planes[planeNo].point +
                        (((planes[i].point - planes[planeNo].point) * planes[i].normal) /
                         (lineNormal * planes[i].normal)) *
                            lineNormal;

                    if (!linearProgram1(planes, i, line, radius, optVelocity, directionOpt,
                                        ref result))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /**
         * @brief      Solves a three-dimensional linear program subject to linear
         *             constraints defined by planes and a spherical constraint.
         * @param[in]  planes       Planes defining the linear constraints.
         * @param[in]  radius       The radius of the spherical constraint.
         * @param[in]  optVelocity  The optimization velocity.
         * @param[in]  directionOpt True if the direction should be optimized.
         * @param[out] result       A reference to the result of the linear program.
         * @return     The number of the plane it fails on, and the number of planes if
         *             successful.
         */
        public static int linearProgram3(IList<Plane> planes, float radius,
                                   Vector3 optVelocity, bool directionOpt,
                                   ref Vector3 result)
        {   /* NOLINT(runtime/references) */
            if (directionOpt)
            {
                /* Optimize direction. Note that the optimization velocity is of unit length
                 * in this case.
                 */
                result = optVelocity * radius;
            }
            else if (absSq(ref optVelocity) > radius * radius)
            {
                /* Optimize closest point and outside circle. */
                result = normalize(optVelocity) * radius;
            }
            else
            {
                /* Optimize closest point and inside circle. */
                result = optVelocity;
            }

            for (int i = 0; i < planes.Count; ++i)
            {
                if (planes[i].normal * (planes[i].point - result) > 0.0f)
                {
                    /* Result does not satisfy constraint i. Compute new optimal result. */
                    Vector3 tempResult = result;

                    if (!linearProgram2(planes, i, radius, optVelocity, directionOpt,
                                        ref result))
                    {
                        result = tempResult;
                        return i;
                    }
                }
            }

            return planes.Count;
        }

		/**
         * @brief      Solves a four-dimensional linear program subject to linear
         *             constraints defined by planes and a spherical constraint.
         * @param[in]  planes     Planes defining the linear constraints.
         * @param[in]  beginPlane The plane on which the three-dimensional linear
         *                        program failed.
         * @param[in]  radius     The radius of the spherical constraint.
         * @param[out] result     A reference to the result of the linear program.
         */

		public static void linearProgram4(IList<Plane> planes, IList<Plane> projPlanes, int beginPlane,
                            float radius,
                            ref Vector3 result)
        {   /* NOLINT(runtime/references) */
            float distance = 0.0F;

            for (int i = beginPlane; i < planes.Count; ++i)
            {
                if (planes[i].normal * (planes[i].point - result) > distance)
                {
                    /* Result does not satisfy constraint of plane i. */
                    projPlanes.Clear();

					for (int j = 0; j < i; ++j)
                    {
                        Plane plane;

                        Vector3 crossProduct = cross(planes[j].normal, planes[i].normal);

                        if (absSq(ref crossProduct) <= RVO3D_EPSILON)
                        {
                            /* Plane i and plane j are (almost) parallel. */
                            if (planes[i].normal * planes[j].normal > 0.0f)
                            {
                                /* Plane i and plane j point in the same direction. */
                                continue;
                            }

                            /* Plane i and plane j point in opposite direction. */
                            plane.point = 0.5f * (planes[i].point + planes[j].point);
                        }
                        else
                        {
                            /* Plane.point is point on line of intersection between plane i and
                             * plane j.
                             */
                            Vector3 lineNormal = cross(crossProduct, planes[i].normal);
                            plane.point =
                                planes[i].point +
                                (((planes[j].point - planes[i].point) * planes[j].normal) /
                                 (lineNormal * planes[j].normal)) *
                                    lineNormal;
                        }

                        plane.normal = normalize(planes[j].normal - planes[i].normal);
                        projPlanes.Add(plane);
                    }

                    Vector3 tempResult = result;

                    if (linearProgram3(projPlanes, radius, planes[i].normal, true, ref result) <
                        projPlanes.Count)
                    {
                        /* This should in principle not happen. The result is by definition
                         * already in the feasible region of this linear program. If it fails,
                         * it is due to small floating point error, and the current result is
                         * kept.
                         */
                        result = tempResult;
                    }

                    distance = planes[i].normal * (planes[i].point - result);
                }
            }
        }
    }

    public class Agent
    {
        internal float freezed_ = 0;
        internal float beginFreezedTime = 0;
        internal Vector3 newVelocity_;
        internal Vector3 position_;
        internal Vector3 prefVelocity_;
        internal Vector3 velocity_;
        internal Quaternion rot_ = Quaternion.identity;
        internal Vector3 goal_;
        internal Vector3 color_;
        internal RVOSimulator sim_;
        internal int id_ = 0;
        internal int maxNeighbors_ = 0;
        internal float maxSpeed_ = 0.0f;
        internal float neighborDist_ = 0.0f;
        internal float radius_ = 0.0f;
        internal float timeHorizon_ = 0.0f;
        internal List<(float, Agent)> agentNeighbors_ = new List<(float, Agent)>();
        internal List<Plane> orcaPlanes_ = new List<Plane>();
		internal List<Plane> projPlanes_ = new List<Plane>();
		internal Agent(RVOSimulator sim)
        {
            sim_ = sim;
        }

        internal void computeNeighbors()
        {
            agentNeighbors_.Clear();
            if (freezed_ > 0) return;

            if (maxNeighbors_ > 0U)
            {
                sim_.kdTree_.computeAgentNeighbors(this, neighborDist_ * neighborDist_);
            }
        }

        internal void computeNewVelocity()
        {
            orcaPlanes_.Clear();
			if (freezed_ > 0) return;

			float invTimeHorizon = 1.0F / timeHorizon_;

            /* Create agent ORCA planes. */
            for (int i = 0; i < agentNeighbors_.Count; ++i)
            {
                Agent other = agentNeighbors_[i].Item2;
                Vector3 relativePosition = other.position_ - position_;
                Vector3 relativeVelocity = velocity_ - other.velocity_;
                float distSq = RVOMath.absSq(ref relativePosition);
                float combinedRadius = radius_ + other.radius_;
                float combinedRadiusSq = combinedRadius * combinedRadius;

                Plane plane;
                Vector3 u;

                if (distSq > combinedRadiusSq)
                {
                    /* No collision. */
                    Vector3 w = relativeVelocity - invTimeHorizon * relativePosition;
                    /* Vector from cutoff center to relative velocity. */
                    float wLengthSq = RVOMath.absSq(ref w);

                    float dotProduct = w * relativePosition;

                    if (dotProduct < 0.0F &&
                        dotProduct * dotProduct > combinedRadiusSq * wLengthSq)
                    {
                        /* Project on cut-off circle. */
                        float wLength = Mathf.Sqrt(wLengthSq);
                        Vector3 unitW = w / wLength;

                        plane.normal = unitW;
                        u = (combinedRadius * invTimeHorizon - wLength) * unitW;
                    }
                    else
                    {
                        /* Project on cone. */
                        float a = distSq;
                        float b = relativePosition * relativeVelocity;
                        Vector3 d = RVOMath.cross(relativePosition, relativeVelocity);
                        float c = RVOMath.absSq(ref relativeVelocity) -
                                        RVOMath.absSq(ref d) /
                                            (distSq - combinedRadiusSq);
                        float t = (b + Mathf.Sqrt(b * b - a * c)) / a;
                        Vector3 ww = relativeVelocity - t * relativePosition;
                        float wwLength = RVOMath.abs(ref ww);
                        Vector3 unitWW = ww / wwLength;

                        plane.normal = unitWW;
                        u = (combinedRadius * t - wwLength) * unitWW;
                    }
                }
                else
                {
                    /* Collision. */
                    float invTimeStep = 1.0F / sim_.timeStep_;
                    Vector3 w = relativeVelocity - invTimeStep * relativePosition;
                    float wLength = RVOMath.abs(ref w);
                    Vector3 unitW = w / wLength;

                    plane.normal = unitW;
                    u = (combinedRadius * invTimeStep - wLength) * unitW;
				}
                
                plane.point = velocity_ + 0.5F * u;
                orcaPlanes_.Add(plane);
            }

            int planeFail = RVOMath.linearProgram3(
                orcaPlanes_, maxSpeed_, prefVelocity_, false, ref newVelocity_);

            if (planeFail < orcaPlanes_.Count)
            {
                RVOMath.linearProgram4(orcaPlanes_, projPlanes_, planeFail, maxSpeed_, ref newVelocity_);
            }
        }

        internal void insertAgentNeighbor(Agent agent, ref float rangeSq) 
        {
            if (this != agent && agent.freezed_ == 0) 
            {
                float distSq = RVOMath.absSq(position_ - agent.position_);

                if (distSq < rangeSq)
                {
                    if (agentNeighbors_.Count < maxNeighbors_)
                    {
                        agentNeighbors_.Add((distSq, agent));
                    }

                    int i = agentNeighbors_.Count - 1;

                    while (i != 0 && distSq < agentNeighbors_[i - 1].Item1)
                    {
                        agentNeighbors_[i] = agentNeighbors_[i - 1];
                        --i;
                    }

                    agentNeighbors_[i] = (distSq, agent);

                    if (agentNeighbors_.Count == maxNeighbors_)
                    {
                        rangeSq = agentNeighbors_[agentNeighbors_.Count-1].Item1;
                    }
                }
            }
        }

        internal void update()
        {
			var dir = new UnityEngine.Vector3(newVelocity_.x(), newVelocity_.y(), newVelocity_.z());
            dir.Normalize();

            if (freezed_ > 0)
            {
				rot_ = Quaternion.Slerp(rot_, Quaternion.FromToRotation(UnityEngine.Vector3.up, dir), 0.025f);
			}
            else 
            {
				//if (dir.y > -0.25f)
				//{
				rot_ = Quaternion.Slerp(rot_, Quaternion.FromToRotation(UnityEngine.Vector3.up, dir), 0.005f);
				//}
			}

			velocity_ = newVelocity_ * (freezed_ + 1);
			position_ += velocity_ * sim_.timeStep_;
		}
    }
}
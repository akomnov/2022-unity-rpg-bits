using System.Collections.Generic;

namespace RPG.Components.AI.BehaviourRunners
{
    public partial class BaseBehaviourRunner : RPG.Core.Shared.AI.IActionContext
    {
        public float GetShortestDistance(object aGO, object bGO)
        {
            return UnityEngine.Mathf.Abs(
                (((UnityEngine.GameObject)aGO).transform.position - ((UnityEngine.GameObject)bGO).transform.position).magnitude
            );
        }
        public (object path, float undershoot) FindPath(object subjectGO, object toGO)
        {
            var _agent = LookUpAgent((UnityEngine.GameObject)subjectGO);
            var _path = new UnityEngine.AI.NavMeshPath();
            var _toTransform = ((UnityEngine.GameObject)toGO).transform.position;
            if (
                false == _agent.CalculatePath(_toTransform, _path)
                || _path.status == UnityEngine.AI.NavMeshPathStatus.PathInvalid
            ) {
                return (path: null, undershoot: float.PositiveInfinity);
            }
            if (_path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete) {
                return (path: _path, undershoot: 0f);
            }
            return (
                path: _path,
                undershoot: UnityEngine.Mathf.Abs((_path.corners[_path.corners.Length - 1] - _toTransform).magnitude)
            );
        }
        public bool SetPathDestination(object subjectGO, object targetGO, float stoppingDistance, bool autoRepath) {
            var _agent = LookUpAgent((UnityEngine.GameObject)subjectGO);
            _agent.stoppingDistance = stoppingDistance;
            _agent.autoBraking = true;
            _agent.autoRepath = autoRepath;
            return _agent.SetDestination(((UnityEngine.GameObject)targetGO).transform.position);
        }
        public bool SetPath(object subjectGO, object precalculatedPath, float stoppingDistance)
        {
            var _agent = LookUpAgent((UnityEngine.GameObject)subjectGO);
            _agent.stoppingDistance = stoppingDistance;
            _agent.autoBraking = true;
            _agent.autoRepath = true;
            return _agent.SetPath((UnityEngine.AI.NavMeshPath)precalculatedPath);
        }
        public void UnsetPath(object subjectGO)
        {
            LookUpAgent((UnityEngine.GameObject)subjectGO).ResetPath();
        }
        public bool StoppedPathing(object subjectGO)
        {
            var _agent = LookUpAgent((UnityEngine.GameObject)subjectGO);
            if (_agent.pathPending)
                return false;
            if (_agent.remainingDistance > _agent.stoppingDistance)
                return false;
            return (!_agent.hasPath || _agent.velocity.sqrMagnitude <= UnityEngine.Mathf.Epsilon);
        }
        protected static Dictionary<UnityEngine.GameObject, UnityEngine.AI.NavMeshAgent> agents = new Dictionary<UnityEngine.GameObject, UnityEngine.AI.NavMeshAgent>();
        protected UnityEngine.AI.NavMeshAgent LookUpAgent(UnityEngine.GameObject go)
        {
            if (!agents.ContainsKey(go))
                agents[go] = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
            return agents[go];
        }
    }
}
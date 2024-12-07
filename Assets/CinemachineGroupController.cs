using Cinemachine;
using Fusion;
using System.Linq;
using UnityEngine;

public class CinemachineGroupController : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;

    public void AddTargetGroupMember(Component sender, object data)
    {
        if (sender is PlayerSpawn spawn && data is GameObject obj)
        {
            var cameraLook = obj.transform.Find("CameraLook");
            if (DoesGroupContainTarget(cameraLook))
                return;

            var networkObject = spawn.GetComponent<NetworkObject>();
            if (networkObject == null)
                return;

            // Sprawdzamy, czy to obiekt gracza z autoryzacją wejścia
            if (networkObject.HasInputAuthority)
            {
                targetGroup.AddMember(cameraLook, 1, 10); // Wyższy priorytet dla lokalnego gracza
            }
            else
            {
                targetGroup.AddMember(cameraLook, 1, 1); // Inni gracze
            }
        }
    }

    public bool DoesGroupContainTarget(Transform target)
    {
        if (targetGroup == null || target == null)
        {
            Debug.LogWarning("TargetGroup or Target is null.");
            return false;
        }

        foreach (var member in targetGroup.m_Targets)
        {
            if (member.target == target)
                return true;
        }

        return false;
    }

    public void RemoveTargetGroupMember(Component sender, object data)
    {
        if (sender is PlayerSpawn spawn && data is GameObject obj)
        {
            var cameraLook = obj.transform.Find("CameraLook");
            if (DoesGroupContainTarget(cameraLook))
            {
                targetGroup.RemoveMember(cameraLook);
            }
        }
    }
}

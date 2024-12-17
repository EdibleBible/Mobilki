using Cinemachine;
using System.Linq;
using UnityEngine;

public class CinemachineGroupController : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;

    public void AddTargetgroupMember(Component sender, object data)
    {
        if (sender is PlayerSpawn spawn && data is GameObject obj)
        {
            var cameraLook = obj.transform.Find("CameraLook");
            targetGroup.AddMember(cameraLook, 5, 10);

        }
    }
    public void RemoveTargetgroupMember(Component sender, object data)
    {
        if (sender is PlayerSpawn spawn && data is GameObject obj)
        {
            var cameraLook = obj.transform.Find("CameraLook");
            targetGroup.RemoveMember(cameraLook);
        }

    }
}

using Cinemachine;
using System.Linq;
using UnityEngine;

public class TargetGroupController : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;


    public void AddTransformToTargetGroup(Component sender, object data)
    {
        // Sprawdzamy, czy sender jest typu NetworkController
        if (sender is NetworkController && data is (Transform tr, bool isMain))
        {
            // Przetwarzamy tuplet
            if (isMain)
            {
                Debug.Log($"Dodawanie głównego gracza: {tr.name}");
                AddToTargetGroupAsMain(tr);
            }
            else
            {
                Debug.Log($"Dodawanie innego gracza: {tr.name}");
                AddToTargetGroupAsSecond(tr);
            }
        }
        else
        {
            Debug.LogWarning("Nieprawidłowe dane lub nieobsługiwany typ sender.");
        }
    }

    public void RemoveTransformFromTargetGroup(Component sender, object data)
    {
        if(data is Transform tr && sender is NetworkController)
        {
            RemoveFromTargetGroup(tr);
        }
    }

    private void AddToTargetGroupAsMain(Transform objectTransform)
    {
        // Sprawdzamy, czy obiekt już istnieje w grupie
        if (!IsMemberInTargetGroup(objectTransform))
        {
            targetGroup.AddMember(objectTransform, 10f, 5f);
        }
        else
        {
            Debug.Log($"Obiekt {objectTransform.name} już istnieje w grupie.");
        }
    }

    private void AddToTargetGroupAsSecond(Transform objectTransform)
    {
        // Sprawdzamy, czy obiekt już istnieje w grupie
        if (!IsMemberInTargetGroup(objectTransform))
        {
            targetGroup.AddMember(objectTransform, 5f, 2f);
        }
        else
        {
            Debug.Log($"Obiekt {objectTransform.name} już istnieje w grupie.");
        }
    }

    private void RemoveFromTargetGroup(Transform objectTransform)
    {
        // Usuwamy obiekt z grupy, jeśli istnieje
        if (IsMemberInTargetGroup(objectTransform))
        {
            targetGroup.RemoveMember(objectTransform);
        }
        else
        {
            Debug.LogWarning($"Obiekt {objectTransform.name} nie jest członkiem grupy.");
        }
    }

    // Pomocnicza metoda, która sprawdza, czy dany Transform jest już członkiem grupy
    private bool IsMemberInTargetGroup(Transform objectTransform)
    {
        // Przechodzimy po wszystkich członkach grupy
        foreach (var member in targetGroup.m_Targets)
        {
            if (member.target == objectTransform)
            {
                return true; // Obiekt już jest członkiem grupy
            }
        }
        return false; // Obiekt nie jest członkiem grupy
    }

}

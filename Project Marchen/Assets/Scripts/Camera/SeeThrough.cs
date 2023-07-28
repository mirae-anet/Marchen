using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 지형, 지물에 의해 시야가 가려지는 것을 방지.
/// @details 아바타와 카메라 사이의 지형, 지물을 투명화한다. colider가 있는 지형, 지물에 한하여 동작.
public class SeeThrough : MonoBehaviour
{
    public Transform player;
    public Vector3 offest;
    /// @brief 투명화할 레이어 정보
    public LayerMask layerMask;
    private List<Transform> ObjectToHide = new List<Transform>();
    private List<Transform> ObjectToShow = new List<Transform>();

    private void LateUpdate()
    {

        if(LocalCameraHandler.Local != GetComponentInParent<LocalCameraHandler>())
            return;

        ManageBlockingView();
 
        foreach (var obstruction in ObjectToHide)
        {
            HideObstruction(obstruction);
        }
 
        foreach (var obstruction in ObjectToShow)
        {
            ShowObstruction(obstruction);
        }
    }

    /// @brief 투명화할 지형, 지물을 관리한다. 추가 혹은 제외.
    private void ManageBlockingView()
    {
        Vector3 playerPosition = player.transform.position + offest;
        float characterDistance = Vector3.Distance(transform.position, playerPosition);

        // RaycastHit[] hits = Physics.SphereCastAll(transform.position, 1,playerPosition - transform.position, characterDistance, layerMask);
        RaycastHit[] hits = Physics.RaycastAll(transform.position, playerPosition - transform.position, characterDistance, layerMask);
        if (hits.Length > 0)
        {
            foreach (var obstruction in ObjectToHide)
            {
                ObjectToShow.Add(obstruction);
            }
 
            ObjectToHide.Clear();
 
            foreach (var hit in hits)
            {
                Transform obstruction = hit.transform;
                ObjectToHide.Add(obstruction);
                ObjectToShow.Remove(obstruction);
            }
        }
        else
        {
            foreach (var obstruction in ObjectToHide)
            {
                ObjectToShow.Add(obstruction);
            }
 
            ObjectToHide.Clear();
        }
    }

    /// @brief 방해물을 투명화한다.
    private void HideObstruction(Transform obstruction)
    {
        if(obstruction != null)
            if(obstruction.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        // MeshRenderer meshRenderer = obstruction.GetComponentInChildren<MeshRenderer>();
        // if(meshRenderer != null)
        //     meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    }

    /// @brief 더이상 방해물이 아닌 경우 투명화를 해제한다.
    private void ShowObstruction(Transform obstruction)
    {
        if(obstruction != null)
            if(obstruction.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        // MeshRenderer meshRenderer = obstruction.GetComponentInChildren<MeshRenderer>();
        // if(meshRenderer != null)
        //     meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeeThrough : MonoBehaviour
{
    public Transform player;
    public Vector3 offest;
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
    private void HideObstruction(Transform obstruction)
    {
        if(obstruction != null)
            if(obstruction.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        // MeshRenderer meshRenderer = obstruction.GetComponentInChildren<MeshRenderer>();
        // if(meshRenderer != null)
        //     meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
}
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

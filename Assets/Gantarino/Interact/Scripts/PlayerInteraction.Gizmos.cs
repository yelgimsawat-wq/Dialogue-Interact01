using UnityEngine;

public partial class PlayerInteraction
{
    private void OnDrawGizmosSelected()
    {
        if (InteractionType == InteractionType.FirstPerson)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * Range);
        }
        else if (InteractionType == InteractionType.ThirdPerson)
        {
            Vector3 origin = player.transform.position + Vector3.up * 0.6f;
            Vector3 end = origin + player.transform.forward * Range;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(end, SphereCastRadius);
        }
    }
}
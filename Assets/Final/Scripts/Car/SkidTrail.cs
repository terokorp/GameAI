using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkidTrail : MonoBehaviour
{
    [SerializeField] private float m_PersistTime = 10f;
    private IEnumerator Start()
    {
        while (true)
        {
            yield return null;

            if (transform.parent == null || transform.parent.parent == null)
            {
                Destroy(gameObject, m_PersistTime);
            }
        }
    }
}

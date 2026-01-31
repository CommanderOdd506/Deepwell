using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Despawner : MonoBehaviour
{
    [SerializeField] private bool despawnOnStart;
    public float despawnTimer = 10f;

    // Start is called before the first frame update
    void Start()
    {
        if(despawnOnStart)
        {
            StartCoroutine(Despawn());
        }
    }

    // Update is called once per frame
    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(despawnTimer);

        Destroy(this.gameObject);
    }
}

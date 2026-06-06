using System.Collections;
using UnityEngine;

public class DelayActive : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitActive());
    }
    IEnumerator WaitActive(){
        SpriteRenderer.enabled = false;
        yield return new WaitForSeconds(5);
        SpriteRenderer.enabled = true;
    }
    
}

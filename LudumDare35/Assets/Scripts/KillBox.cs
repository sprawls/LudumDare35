using UnityEngine;
using System.Collections;

public class KillBox : MonoBehaviour {

    static GameManager manager;

    private Vector3 startPosition;
    private Vector3 endPosition;

	// Use this for initialization
	void Start () {
        if (manager == null) manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        startPosition = transform.position;
        endPosition = new Vector3(startPosition.x, startPosition.y, 0);
        StartCoroutine(ReachZeroZ());
	}
	

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            Debug.Log("boom");
        }
    }

    void ActivateCollisions() {
        Collider2D[] cols = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in cols) {
            col.enabled = true;
        }
    }

    IEnumerator ReachZeroZ() {
        for (float i = 0f; i < 1f; i += Time.deltaTime / manager.wallReachZeroTime) {
            transform.position = Vector3.Lerp(startPosition, endPosition, i);
            yield return null;
        }
        ActivateCollisions();
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }
}

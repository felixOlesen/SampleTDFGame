using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeObjectController : MonoBehaviour
{
    // Start is called before the first frame update

private CircleCollider2D aoeCollider;
public float aoeScalar;
public int aoeDamage;
public GameObject aoeParticles;
public ParticleSystem system;
public AudioSource aoeSfx;

void Start() {
    aoeCollider = gameObject.AddComponent(typeof(CircleCollider2D)) as CircleCollider2D;
    if(aoeParticles) {
        aoeParticles = Instantiate(aoeParticles, transform.position, Quaternion.identity);
        aoeParticles.SetActive(false);
        system = aoeParticles.GetComponent<ParticleSystem>();
    }
    this.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
}

public void InflictAoe(float radius, float duration, float scalar, int damage, string attackType) {
    aoeCollider.radius = radius;
    aoeScalar = scalar;
    aoeDamage = damage;
    gameObject.tag = attackType;
    if(aoeParticles) {
        this.transform.localScale = new Vector3(radius, radius, radius);
        var systemMain = system.main;
        aoeParticles.SetActive(true);
        aoeParticles.transform.position = transform.position;
        aoeSfx.Play();
        if( attackType == "explosive"){
            systemMain.startSize = radius*0.9f;
        } else if(attackType == "stun") {
            systemMain.startSize = radius/1.2f;
        }

    }
    system.Play();
    StartCoroutine(FinishAoe(duration));

}

IEnumerator FinishAoe(float duration) {
    yield return new WaitForSeconds(duration);
    aoeCollider.radius = 0.0f;
    this.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
    this.transform.position = transform.parent.position;
}

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using TMPro;
public class EnemyController : MonoBehaviour
{
public GameObject path;
public GameObject levelManager;
private SpriteShapeController pathController;
private Spline pathSpline;

[SerializeField]
public float speed;
private float initialSpeed;

[SerializeField]
private int currentHealth;
[SerializeField]
private int attackDamage = 10;
public int maxHealth = 100;
public HealthBar healthBar;
private Vector3 currentCheckpointPos;
private int currentCheckpointIndex;
public float armour;
public int moneyReward;
public bool stealthy;
public bool swarmHost;
public GameObject swarmChild;
public bool isSwarmChild;
public float aoeScalar = 1.0f;
public bool stunned;
public AudioSource deathSound;
public GameObject stunIcon;
public GameObject damagePopup;

private void Start() {
    path = GameObject.Find("WoodenPath");
    pathController = path.GetComponent<SpriteShapeController>();
    pathSpline = pathController.spline;
    levelManager = GameObject.Find("LevelManager");
    if(!isSwarmChild) {
        transform.position = pathSpline.GetPosition(0);
        currentCheckpointIndex = 1;
        currentCheckpointPos = pathSpline.GetPosition(currentCheckpointIndex);
    }
    currentHealth = maxHealth;
    healthBar.SetMaxHealth(maxHealth);
    initialSpeed = speed;
    stunIcon.SetActive(false);
    
}

private void Update() {
    currentCheckpointPos = UpdateCheckpoint();
    Aim(currentCheckpointPos);
    transform.position = Vector3.MoveTowards(transform.position, currentCheckpointPos, speed * Time.deltaTime);
}

private void Aim(Vector3 targetPos) {
    if(targetPos != null) {
        Vector3 dir = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.GetChild(0).rotation = Quaternion.Euler( 0, 0, angle - 90.0f); 
    }
}

private Vector3 UpdateCheckpoint() {
    if(currentCheckpointPos == transform.position) {
        if(currentCheckpointIndex < pathSpline.GetPointCount() && currentCheckpointIndex < pathSpline.GetPointCount()) {
            currentCheckpointPos = pathSpline.GetPosition(currentCheckpointIndex);
            currentCheckpointIndex += 1;
        }
        if(currentCheckpointIndex == pathSpline.GetPointCount()) {
            Destroy(gameObject);
            levelManager.GetComponent<LevelManager>().LevelDamage(attackDamage);
            levelManager.GetComponent<LevelManager>().ChangeMoneyTotal(Mathf.RoundToInt(moneyReward * 0.75f));
        }
    }
    return currentCheckpointPos;
}


public void SpawnSwarm() {
    for(int i = 0; i < 10; i++) {
        GameObject tempEnemy = Instantiate(swarmChild);
        tempEnemy.GetComponent<EnemyController>().SetCurrentCheckpoint(currentCheckpointPos, currentCheckpointIndex);
        tempEnemy.transform.position = transform.position;
    }
}

public void SetCurrentCheckpoint(Vector3 pos, int ind) {
    this.currentCheckpointPos = pos;
    this.currentCheckpointIndex = ind;
}


public void TakeDamage(int damage, bool pierce, bool armourDestroying, bool crit) {
    if(!pierce){
        damage = Mathf.RoundToInt((float)damage * armour);
    }
    if(armourDestroying) {
        armour = 1;
    }
    currentHealth -= damage;
    var dp = Instantiate(damagePopup, transform.position, Quaternion.identity);
    dp.GetComponent<TMP_Text>().text = damage.ToString();
    if(armour < 1 && !pierce) {
        dp.GetComponent<TMP_Text>().color = new Color(1f, 1f, 1f, 1f);
    }
    if(crit) {
        dp.GetComponent<TMP_Text>().color = new Color(1f, 0f, 0f, 1f);
    }
    healthBar.SetHealth(currentHealth);
    if(currentHealth <= 0) {
        if(swarmHost) {
            int nEnemy = 10;
            levelManager.GetComponent<LevelManager>().SwarmSpawning(nEnemy, swarmChild, currentCheckpointIndex, currentCheckpointPos, transform.position);
        }
        AudioSource.PlayClipAtPoint(deathSound.clip, transform.position, deathSound.volume);
        levelManager.GetComponent<LevelManager>().IncrementBugsKilled();
        levelManager.GetComponent<LevelManager>().ChangeMoneyTotal(moneyReward);
        Destroy(gameObject);
    }
}

private void OnTriggerEnter2D(Collider2D other) {
    if(other.CompareTag("stun")) {
        float scalarValue = other.gameObject.GetComponent<AoeObjectController>().aoeScalar;
        stunIcon.SetActive(true);
        if(!stunned) {
            stunned = true;
            speed *= scalarValue;
        }
    } else if(other.CompareTag("explosive")) {
        int damageValue = other.gameObject.GetComponent<AoeObjectController>().aoeDamage;
        float scalarValue = other.gameObject.GetComponent<AoeObjectController>().aoeScalar;
        damageValue = Mathf.RoundToInt((float)damageValue * scalarValue * armour);
        TakeDamage(damageValue, false, false, false);
    }
}
private void OnTriggerExit2D(Collider2D other) {
    if(other.CompareTag("stun")) {
        if(stunned) {
            stunned = false;
            speed = initialSpeed;
            stunIcon.SetActive(false);
        }
    }
}
}

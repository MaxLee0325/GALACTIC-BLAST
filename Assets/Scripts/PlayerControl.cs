using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    Animation anim;
    public float speed = 5f;
    public float originalSpeed;
    public float moveAmount = 1f;
    private bool isMoving = false;
    public bool isSlowed = false;

    public GameObject bombPrefab;        // assign your Bomb prefab
    public GameObject electricBombPrefab;        // assign your Bomb prefab
    public GameObject fireBombPrefab;        // assign your Bomb prefab
    public GameObject waterBombPrefab;        // assign your Bomb prefab
    public float bombCooldown = 20f;   // time between drops
    public float spawnForward = 0.6f;    // a bit in front of feet
    private float _lastBombTime = -999f;
    private enum BombType { Normal, Electric, Fire, Water }
    private BombType currentBombType = BombType.Normal;

    private int maxSpeedPowerUp = 5;
    private int countSpeedPowerUp = 0;
    private float powerUpSpeed = 0.5f;

    private int maxRangePowerUp = 6;
    private int rangePowerUpLevel = 0;
    private float rangePerLevel = 1f;

    private int maxBombPowerUp = 6;
    private int countBombPowerUp = 0;

    public bool isScout = false;
    public bool isMediv = false;
    public bool isTanya = false;
    public bool inCoolDown = false;
    public float skillCoolDown;

    [SerializeField] private SkillVisualization skillVisualization;
    [SerializeField] private PlayerHearts playerHearts;


    [Header("Audio")]
    [SerializeField] private AudioSource maxPowerUpAudio;

    void Start()
    {
        anim = GetComponent<Animation>();
        LoadHero();
    }

    private void LoadHero(){
        switch (HeroSelect.SelectedHero)
        {
            case HeroSelect.Hero.Scout:
                speed *= 1.2f;
                skillCoolDown = 15f;
                isScout = true;
                break;

            case HeroSelect.Hero.Tanya:
                skillCoolDown = 15f;
                speed *= 0.85f;
                isTanya = true;
                break;

            case HeroSelect.Hero.Mediv:
                skillCoolDown = 20f;
                isMediv = true;
                break;
        }
        originalSpeed = speed;
    }

    void Update()
    {
        if (isMoving) return;

        Vector3 direction = Vector3.zero;

        // cast special hero skills
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!inCoolDown)
            {
                if(isScout)
                {
                    StartCoroutine(ScoutDash());
                }
                else if(isMediv)
                {
                    StartCoroutine(MedicvProtect());
                }
                else if(isTanya)
                {
                    StartCoroutine(TanyaMegaBomb());
                }
                skillVisualization.startCountDown();
            } 
            else
            {
                skillVisualization.Pop();
            }
        }
        
        if (Input.GetKey(KeyCode.UpArrow))
        {
            direction = Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            direction = Vector3.back;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            direction = Vector3.left;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            direction = Vector3.right;
        }

        if (direction != Vector3.zero)
        {
            RotateToDirection(direction);
            if (CanMove(direction))
            {
                anim.Play();
                StartCoroutine(Move(direction));
            }
            else
            {
                // Stop animation when idle
                anim.Stop();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DropBomb();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Water power-up
        if (other.CompareTag("PowerUp_Water"))
        {
            currentBombType = BombType.Water;
            Debug.Log("Picked up WATER power-up! Now dropping Water Bombs.");
            Destroy(other.gameObject);
        }
        // Fire power-up
        else if (other.CompareTag("PowerUp_Fire"))
        {
            currentBombType = BombType.Fire;
            Debug.Log("Picked up FIRE power-up! Now dropping Fire Bombs.");
            Destroy(other.gameObject);
        }
        // Electric power-up
        else if (other.CompareTag("PowerUp_Electric"))
        {
            currentBombType = BombType.Electric;
            Debug.Log("Picked up ELECTRIC power-up! Now dropping Electric Bombs.");
            Destroy(other.gameObject);
        }
        
        // Heart Power-up
        if (other.CompareTag("PowerUp_Heart"))
        {
            var hearts = GetComponentInChildren<PlayerHearts>();
            if (hearts != null )
            {
                hearts.PickupHeart();
            }
            Destroy(other.gameObject);
        }

        // Speed Power-up
        if (other.CompareTag("PowerUp_Speed"))
        {
            if (countSpeedPowerUp < maxSpeedPowerUp)
            {
                speed += powerUpSpeed;
                originalSpeed += powerUpSpeed;
                countSpeedPowerUp += 1;
            }
            else
            {
                PlayMaxPowerUpAudio();
            }

                Destroy(other.gameObject);
        }

        // Range power-up
        if (other.CompareTag("PowerUp_Range"))
        {
            if (rangePowerUpLevel < maxRangePowerUp)
            {
                rangePowerUpLevel++;
            }
            else
            {
                PlayMaxPowerUpAudio();
            }
            Destroy(other.gameObject);
        }

        // Bomb power-up
        if (other.CompareTag("PowerUp_Bomb"))
        {
            if (countBombPowerUp < maxBombPowerUp)
            {
                countBombPowerUp += 1;
                Debug.Log("Bomb power up"+countBombPowerUp);
            }
            else
            {
                PlayMaxPowerUpAudio();
            }
            Destroy(other.gameObject);
        }
    }

    bool CanMove(Vector3 direction)
    {
        Vector3 targetPos = transform.position + direction * moveAmount;

        Collider[] hits = Physics.OverlapSphere(targetPos, 0.35f);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Wall") || hit.CompareTag("Destructible")) {
                return false;
            }
        }

        return true;
    }

    void RotateToDirection(Vector3 direction)
    {
        if (direction == Vector3.forward)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction == Vector3.back)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (direction == Vector3.left)
        {
            transform.rotation = Quaternion.Euler(0, -90, 0);
        }
        else if (direction == Vector3.right)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }

    IEnumerator Move(Vector3 direction)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 target = start + direction * moveAmount;

        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            transform.position = Vector3.Lerp(start, target, t);

            yield return null;
        }
        transform.position = target;
        isMoving = false;
    }

    void DropBomb(bool megaBomb = false)
    {

        if(!(Time.time - _lastBombTime >= bombCooldown)) return;
        _lastBombTime = Time.time;

        Vector3 spawnPos = new Vector3(Mathf.Round(transform.position.x), (float)(transform.position.y + 0.5), Mathf.Round(transform.position.z));
        Quaternion spawnRot = Quaternion.identity;

        GameObject selectedPrefab = null;

        switch (currentBombType)
        {
            case BombType.Normal:
                selectedPrefab = bombPrefab;
                break;
            case BombType.Electric:
                selectedPrefab = electricBombPrefab;
                break;
            case BombType.Fire:
                selectedPrefab = fireBombPrefab;
                break;
            case BombType.Water:
                selectedPrefab = waterBombPrefab;
                break;
        }

        if (selectedPrefab == null)
        {
            Debug.LogWarning("No prefab assigned for current bomb type: " + currentBombType);
            return;
        }

        for (int i = 0; i < 1 + countBombPowerUp; i++)
        {
            GameObject bombInstance = Instantiate(selectedPrefab, spawnPos, spawnRot);

            BombController bc = bombInstance.GetComponent<BombController>();
            if (bc != null)
            {
                bc.blastRange += rangePowerUpLevel * rangePerLevel;

                // tanya's skill
                if(megaBomb)
                {
                    bc.blastRange *= 2;
                }
            }
        }
        
    }

    private IEnumerator MedicvProtect()
    {
        inCoolDown = true;
        playerHearts.Protect();
        yield return new WaitForSeconds(skillCoolDown);
        inCoolDown = false;
    }

    private IEnumerator TanyaMegaBomb()
    {
        inCoolDown = true;
        DropBomb(true);
        yield return new WaitForSeconds(skillCoolDown);
        inCoolDown = false;
    }

    private IEnumerator ScoutDash()
    {
        inCoolDown = true;

        float dashSpeed = speed * 2.5f;     // dash is 2.5x faster
        float dashDuration = 0.2f;              // dash time

        speed = dashSpeed;

        float endTime = Time.time + dashDuration;

        // Dash forward using your existing Move() and collision system
        while (Time.time < endTime)
        {
            Vector3 forwardDir = transform.forward;

            // If the next tile is free, move
            if (CanMove(forwardDir))
            {
                yield return StartCoroutine(Move(forwardDir));
            }
            else
            {
                break;  // hit wall -> stop dash
            }
        }

        // Reset speed
        speed = originalSpeed;

        yield return new WaitForSeconds(skillCoolDown);

        inCoolDown = false;
    }

    private void PlayMaxPowerUpAudio()
    {
        if (maxPowerUpAudio != null && !maxPowerUpAudio.isPlaying)
        {
            maxPowerUpAudio?.Play();
        }
    }
}

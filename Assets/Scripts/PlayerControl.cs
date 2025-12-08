using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private Animation anim;
    private Rigidbody rb;
    public float speed = 5f;
    public float originalSpeed;
    public float moveAmount = 1f;
    public bool isSlowed = false;
    public GameObject bombPrefab;
    public GameObject electricBombPrefab;
    public GameObject fireBombPrefab;
    public GameObject waterBombPrefab;
    public float bombCooldown = 20f;
    public float spawnForward = 0.6f;
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
    private float skillCoolDown = 8f;
    private GameObject dashingSmoke;
    public GameObject protectionShield;
    public GameObject implosionPrefab;
    [SerializeField] private SkillVisualization skillVisualization;
    [SerializeField] private PlayerHearts playerHearts;

    [Header("Audio")]
    [SerializeField] private AudioSource maxPowerUpAudio;
    [SerializeField] private AudioSource dashAudio;
    [SerializeField] private AudioSource protectAudio;
    [SerializeField] private AudioSource megaBombAudio;
    [SerializeField] private AudioSource inCoolDownAudio;
    [SerializeField] private AudioSource powerUpAudio;


    void Start()
    {
        anim = GetComponent<Animation>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        LoadHero();
    }

    private void LoadHero()
    {
        switch (HeroSelect.SelectedHero)
        {
            case HeroSelect.Hero.Scout:
                speed *= 1.2f;
                isScout = true;
                dashingSmoke = transform.Find("vfx_Smoke_01").gameObject;
                break;
            case HeroSelect.Hero.Tanya:
                speed *= 0.85f;
                isTanya = true;
                break;
            case HeroSelect.Hero.Mediv:
                isMediv = true;
                protectionShield = transform.Find("vfx_Shield_01").gameObject;
                break;
        }
        originalSpeed = speed;
    }

    void Update()
    {
        Vector3 direction = Vector3.zero;

        // Hero skills
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!inCoolDown)
            {
                if (isScout)
                    StartCoroutine(ScoutDash());
                else if (isMediv)
                    StartCoroutine(MedicvProtect());
                else if (isTanya)
                    StartCoroutine(TanyaMegaBomb());
                skillVisualization.startCountDown();
            }
            else
            {
                skillVisualization.Pop();
                inCoolDownAudio.Play();
            }
        }

        // Movement input
        if (Input.GetKey(KeyCode.UpArrow))
            direction += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow))
            direction += Vector3.back;
        if (Input.GetKey(KeyCode.LeftArrow))
            direction += Vector3.left;
        if (Input.GetKey(KeyCode.RightArrow))
            direction += Vector3.right;

        // Normalize direction for diagonal movement
        if (direction != Vector3.zero)
            direction.Normalize();

        // Move player
        Move(direction);

        // Drop bomb
        if (Input.GetKeyDown(KeyCode.Space))
            DropBomb();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Water power-up
        if (other.CompareTag("PowerUp_Water"))
        {
            currentBombType = BombType.Water;
            powerUpAudio.Play();
            Destroy(other.gameObject);
        }
        // Fire power-up
        else if (other.CompareTag("PowerUp_Fire"))
        {
            currentBombType = BombType.Fire;
            powerUpAudio.Play();
            Destroy(other.gameObject);
        }
        // Electric power-up
        else if (other.CompareTag("PowerUp_Electric"))
        {
            currentBombType = BombType.Electric;
            powerUpAudio.Play();
            Destroy(other.gameObject);
        }
        // Heart Power-up
        if (other.CompareTag("PowerUp_Heart"))
        {
            var hearts = GetComponentInChildren<PlayerHearts>();
            if (hearts != null) hearts.PickupHeart();
            Destroy(other.gameObject);
        }
        // Speed
        if (other.CompareTag("PowerUp_Speed"))
        {
            if (countSpeedPowerUp < maxSpeedPowerUp)
            {
                speed += powerUpSpeed;
                originalSpeed += powerUpSpeed;
                countSpeedPowerUp += 1;
                powerUpAudio.Play();
            }
            else
            {
                maxPowerUpAudio.Play();
            }
            Destroy(other.gameObject);
        }
        // Range
        if (other.CompareTag("PowerUp_Range"))
        {
            if (rangePowerUpLevel < maxRangePowerUp)
            {
                rangePowerUpLevel++;
                powerUpAudio.Play();
            }
            else
                maxPowerUpAudio.Play();
            Destroy(other.gameObject);
        }
        // Bomb count
        if (other.CompareTag("PowerUp_Bomb"))
        {
            if (countBombPowerUp < maxBombPowerUp)
            {
                countBombPowerUp += 1;
                powerUpAudio.Play();
            }
            else
                maxPowerUpAudio.Play();
            Destroy(other.gameObject);
        }
    }

    void Move(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            anim.Stop();
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // Rotate player
        RotateToDirection(direction);

        // Move player
        rb.linearVelocity = direction * speed;

        // Play walking animation
        anim.Play();
    }

    void RotateToDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void DropBomb(bool megaBomb = false)
    {
        if (!(Time.time - _lastBombTime >= bombCooldown)) return;
        _lastBombTime = Time.time;
        Vector3 spawnPos = new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y + 0.5f,
            Mathf.Round(transform.position.z)
        );
        Quaternion spawnRot = Quaternion.identity;
        GameObject selectedPrefab = null;
        switch (currentBombType)
        {
            case BombType.Normal: selectedPrefab = bombPrefab; break;
            case BombType.Electric: selectedPrefab = electricBombPrefab; break;
            case BombType.Fire: selectedPrefab = fireBombPrefab; break;
            case BombType.Water: selectedPrefab = waterBombPrefab; break;
        }
        if (selectedPrefab == null) return;
        for (int i = 0; i < 1 + countBombPowerUp; i++)
        {
            GameObject bombInstance = Instantiate(selectedPrefab, spawnPos, spawnRot);
            BombController bc = bombInstance.GetComponent<BombController>();
            if (bc != null)
            {
                bc.blastRange += rangePowerUpLevel * rangePerLevel;
                if (megaBomb)
                {
                    GameObject implosion = Instantiate(implosionPrefab, spawnPos, spawnRot);
                    bombInstance.transform.localScale *= 1.3f;
                    Destroy(implosion, 4f);
                    bc.blastRange *= 2;
                }
            }
        }
    }

    private IEnumerator MedicvProtect()
    {
        inCoolDown = true;
        protectAudio.Play();
        playerHearts.Protect();
        yield return new WaitForSeconds(skillCoolDown);
        inCoolDown = false;
    }

    private IEnumerator TanyaMegaBomb()
    {
        inCoolDown = true;
        megaBombAudio.Play();
        DropBomb(true);
        yield return new WaitForSeconds(skillCoolDown);
        inCoolDown = false;
    }

    private IEnumerator ScoutDash()
    {
        inCoolDown = true;
        dashAudio.Play();
        float dashSpeed = speed * 2.5f;
        float dashDuration = 0.3f;
        speed = dashSpeed;
        float endTime = Time.time + dashDuration;
        dashingSmoke.SetActive(true);
        while (Time.time < endTime)
        {
            Vector3 forwardDir = transform.forward;
            rb.linearVelocity = forwardDir * speed;
            yield return null;
        }
        dashingSmoke.SetActive(false);
        speed = originalSpeed;
        rb.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(skillCoolDown);
        inCoolDown = false;
    }
}

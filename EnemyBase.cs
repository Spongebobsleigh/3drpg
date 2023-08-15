using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    // ----------------------------------------------------------
    /// <summary>
    /// ステータス.
    /// </summary>
    // ----------------------------------------------------------
    [System.Serializable]
    public class Status
    {
        // HP.
        public int Hp = 10;
        // 攻撃力.
        public int Power = 1;
    }

    // 基本ステータス.
    [SerializeField] Status DefaultStatus = new Status();
    // 現在のステータス.
    public Status CurrentStatus = new Status();

    [SerializeField] public bool IsBoss = false;

    // アニメーター.
    Animator animator = null;

    // 周辺レーダーコライダーコール.
    [SerializeField] ColliderCallReceiver aroundColliderCall = null;

    //! 自身のコライダー.
    [SerializeField] Collider myCollider = null;
    //! 攻撃ヒット時エフェクトプレハブ.
    [SerializeField] GameObject hitParticlePrefab = null;

    // 攻撃間隔.
    [SerializeField] float attackInterval = 3f;

    // 攻撃状態フラグ.
    public bool IsBattle = false;
    // 攻撃時間計測用.
    float attackTimer = 0f;

    //! 攻撃判定用コライダーコール.
    [SerializeField] protected ColliderCallReceiver attackHitColliderCall = null;
    // 現在の攻撃ターゲット.
    protected Transform currentAttackTarget = null;

    // 開始時位置.
    Vector3 startPosition = new Vector3();
    // 開始時角度.
    Quaternion startRotation = new Quaternion();

    //! HPバーのスライダー.
    [SerializeField] Slider hpBar = null;

    // 敵の移動イベント定義クラス.
    public class EnemyMoveEvent : UnityEvent<EnemyBase> { }
    // 目的地設定イベント.
    public EnemyMoveEvent ArrivalEvent = new EnemyMoveEvent();

    // ナビメッシュ.
    NavMeshAgent navMeshAgent = null;

    // 現在設定されている目的地.
    Transform currentTarget = null;

    // 死亡時イベント.
    public EnemyMoveEvent DestroyEvent = new EnemyMoveEvent();

    protected virtual void Start()
    {
        // Animatorを取得し保管.
        animator = GetComponent<Animator>();

        // 最初に現在のステータスを基本ステータスとして設定.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;

        // 周辺コライダーイベント登録.
        aroundColliderCall.TriggerEnterEvent.AddListener(OnAroundTriggerEnter);
        aroundColliderCall.TriggerStayEvent.AddListener(OnAroundTriggerStay);
        aroundColliderCall.TriggerExitEvent.AddListener(OnAroundTriggerExit);

        // 攻撃コライダーイベント登録.
        attackHitColliderCall.TriggerEnterEvent.AddListener(OnAttackTriggerEnter);

        attackHitColliderCall.gameObject.SetActive(false);

        // 開始時の位置回転を保管.
        startPosition = this.transform.position;
        startRotation = this.transform.rotation;

        // スライダーを初期化.
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;
    }

    protected virtual void Update()
    {
        // 攻撃できる状態の時.
        if (IsBattle == true)
        {
            attackTimer += Time.deltaTime;

            animator.SetBool("isRun", false);

            if (attackTimer >= 3f)
            {
                animator.SetTrigger("isAttack");
                attackTimer = 0;
            }
        }
        else
        {
            attackTimer = 0;

            if (currentTarget == null)
            {
                animator.SetBool("isRun", false);

                ArrivalEvent?.Invoke(this);
                Debug.Log(gameObject.name + "移動開始.");
            }
            else
            {
                animator.SetBool("isRun", true);

                var sqrDistance = (currentTarget.position - this.transform.position).sqrMagnitude;
                if (sqrDistance < 3f)
                {
                    ArrivalEvent?.Invoke(this);
                }
            }
        }

    }

    // ----------------------------------------------------------
    /// <summary>
    /// ナビメッシュの次の目的地を設定.
    /// </summary>
    /// <param name="target"> 目的地トランスフォーム. </param>
    // ----------------------------------------------------------
    public void SetNextTarget(Transform target)
    {
        if (target == null) return;
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.SetDestination(target.position);
        Debug.Log(gameObject.name + "ターゲットへ移動." + target.gameObject.name);
        currentTarget = target;
    }

    // ----------------------------------------------------------
    /// <summary>
    /// 攻撃ヒット時コール.
    /// </summary>
    /// <param name="damage"> 食らったダメージ. </param>
    // ----------------------------------------------------------
    public void OnAttackHit(int damage, Vector3 attackPosition)
    {
        CurrentStatus.Hp -= damage;
        hpBar.value = CurrentStatus.Hp;
        Debug.Log("Hit Damage " + damage + "/CurrentHp = " + CurrentStatus.Hp);

        var pos = myCollider.ClosestPoint(attackPosition);
        var obj = Instantiate(hitParticlePrefab, pos, Quaternion.identity);
        var par = obj.GetComponent<ParticleSystem>();
        StartCoroutine(WaitDestroy(par));

        if (CurrentStatus.Hp <= 0)
        {
            OnDie();
        }
        else
        {
            animator.SetTrigger("isHit");
        }
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// パーティクルが終了したら破棄する.
    /// </summary>
    /// <param name="particle"></param>
    // ---------------------------------------------------------------------
    IEnumerator WaitDestroy(ParticleSystem particle)
    {
        yield return new WaitUntil(() => particle.isPlaying == false);
        Destroy(particle.gameObject);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// 死亡時コール.
    /// </summary>
    // ----------------------------------------------------------
    void OnDie()
    {
        Debug.Log("死亡");
        animator.SetBool("isDie", true);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// 死亡アニメーション終了時コール.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_DieEnd()
    {
        // Destroy( gameObject );   // 確認のためコメントアウトしていますが削除してOKです。
        DestroyEvent?.Invoke(this);
    }
    // ------------------------------------------------------------
    /// <summary>
    /// 周辺レーダーコライダーエンターイベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            IsBattle = true;

            navMeshAgent.SetDestination(this.transform.position);
            currentTarget = null;
        }
    }

    // ------------------------------------------------------------
    /// <summary>
    /// 周辺レーダーコライダーステイイベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
    // ------------------------------------------------------------
    protected virtual void OnAroundTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {

            var _dir = (other.gameObject.transform.position - this.transform.position).normalized;
            _dir.y = 0;
            this.transform.forward = _dir;

            currentAttackTarget = other.gameObject.transform;
        }
    }

    // ------------------------------------------------------------
    /// <summary>
    /// 周辺レーダーコライダー終了イベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            IsBattle = false;

            currentAttackTarget = null;
        }
    }

    // ------------------------------------------------------------
    /// <summary>
    /// 攻撃コライダーエンターイベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
    // ------------------------------------------------------------
    void OnAttackTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var player = other.GetComponent<PlayerController>();
            player?.OnEnemyAttackHit(CurrentStatus.Power, this.transform.position);
            attackHitColliderCall.gameObject.SetActive(false);
        }
    }

    // ----------------------------------------------------------
    /// <summary>
    /// 攻撃Hitアニメーションコール.
    /// </summary>
    // ----------------------------------------------------------
    protected virtual void Anim_AttackHit()
    {
        attackHitColliderCall.gameObject.SetActive(true);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// 攻撃アニメーション終了時コール.
    /// </summary>
    // ----------------------------------------------------------
    protected virtual void Anim_AttackEnd()
    {
        attackHitColliderCall.gameObject.SetActive(false);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// プレイヤーリトライ時の処理.
    /// </summary>
    // ----------------------------------------------------------
    public void OnRetry()
    {
        // 現在のステータスを基本ステータスとして設定.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        hpBar.value = CurrentStatus.Hp;

        // 開始時の位置回転を保管.
        this.transform.position = startPosition;
        this.transform.rotation = startRotation;

        this.gameObject.SetActive(true);

    }



}

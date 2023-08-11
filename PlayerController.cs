using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------
    /// <summary>
    /// ステータス.
    /// </summary>
    // -------------------------------------------------------
    [System.Serializable]
    public class Status
    {
        // 体力.
        public int Hp = 10;
        // 攻撃力.
        public int Power = 1;
    }

    // 攻撃HitオブジェクトのColliderCall.
    [SerializeField] ColliderCallReceiver attackHitCall = null;
    // 基本ステータス.
    [SerializeField] Status DefaultStatus = new Status();
    // 現在のステータス.
    public Status CurrentStatus = new Status();

    // 攻撃判定用オブジェクト.
    [SerializeField] GameObject attackHit = null;
    // 設置判定用ColliderCall.
    [SerializeField] ColliderCallReceiver footColliderCall = null;
    // タッチマーカー.
    [SerializeField] GameObject touchMarker = null;
    // ジャンプ力.
    [SerializeField] float jumpPower = 20f;
    // アニメーター.
    Animator animator = null;
    // リジッドボディ.
    Rigidbody rigid = null;
    // 攻撃アニメーション中フラグ.
    bool isAttack = false;
    // 接地フラグ.
    bool isGround = false;

    // PCキー横方向入力.
    float horizontalKeyInput = 0;
    // PCキー縦方向入力.
    float verticalKeyInput = 0;

    bool isTouch = false;

    // 左半分タッチスタート位置.
    Vector2 leftStartTouch = new Vector2();
    // 左半分タッチ入力.
    Vector2 leftTouchInput = new Vector2();

    // カメラコントローラー.
    [SerializeField] PlayerCameraController cameraController = null;

    // 自身のコライダー.
    [SerializeField] Collider myCollider = null;
    // 攻撃を食らったときのパーティクルプレハブ.
    [SerializeField] GameObject hitParticlePrefab = null;
    // パーティクルオブジェクト保管用リスト.
    List<GameObject> particleObjectList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Animatorを取得し保管.
        animator = GetComponent<Animator>();
        // Rigidbodyの取得.
        rigid = GetComponent<Rigidbody>();
        // 攻撃判定用オブジェクトを非表示に.
        attackHit.SetActive(false);

        // FootSphereのイベント登録.
        footColliderCall.TriggerEnterEvent.AddListener(OnFootTriggerEnter);
        footColliderCall.TriggerExitEvent.AddListener(OnFootTriggerExit);

        // 攻撃判定用コライダーイベント登録.
        attackHitCall.TriggerEnterEvent.AddListener(OnAttackHitTriggerEnter);
        // 現在のステータスの初期化.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
    }

    
    // Update is called once per frame
    void Update()
    {
        // カメラをプレイヤーに向ける. 
        cameraController.UpdateCameraLook(this.transform);

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // スマホタッチ操作.
            // タッチしている指の数が０より多い.
            if (Input.touchCount > 0)
            {
                isTouch = true;
                // タッチ情報をすべて取得.
                Touch[] touches = Input.touches;

                // 全部のタッチを繰り返して判定.
                foreach (var touch in touches)
                {
                    bool isLeftTouch = false;
                    bool isRightTouch = false;

                    // タッチ位置のX軸方向がスクリーンの左側.
                    if (touch.position.x > 0 && touch.position.x < Screen.width / 2)
                    {
                        isLeftTouch = true;
                    }
                    // タッチ位置のX軸方向がスクリーンの右側.
                    else if (touch.position.x > Screen.width / 2 && touch.position.x < Screen.width)
                    {
                        isRightTouch = true;
                    }
                    
                    // 左タッチ.
                    if (isLeftTouch == true)
                    {
                        // タッチ開始.
                        if (touch.phase == TouchPhase.Began)
                        {
                            Debug.Log("タッチ開始");
                            // 開始位置を保管.
                            leftStartTouch = touch.position;
                            // 開始位置にマーカーを表示.
                            touchMarker.SetActive(true);
                            Vector3 touchPosition = touch.position;
                            touchPosition.z = 1f;
                            Vector3 markerPosition = Camera.main.ScreenToWorldPoint(touchPosition);
                            touchMarker.transform.position = markerPosition;
                        }
                        // タッチ中.
                        else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                        {
                            Debug.Log("タッチ中");
                            // 現在の位置を随時保管.
                            Vector2 position = touch.position;
                            // 移動用の方向を保管.
                            leftTouchInput = position - leftStartTouch;
                        }
                        // タッチ終了.
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            Debug.Log("タッチ終了");
                            leftTouchInput = Vector2.zero;
                            // マーカーを非表示.
                            touchMarker.gameObject.SetActive(false);
                        }
                    }

                    // 右タッチ.
                    if (isRightTouch == true)
                    {
                        cameraController.UpdateRightTouch(touch);
                        // 右半分をタッチした際の処理.
                    }
                }
            }
            else
            {
                isTouch = false;
            }
        }
        else
        {
            // PCキー入力取得.
            horizontalKeyInput = Input.GetAxis("Horizontal");
            verticalKeyInput = Input.GetAxis("Vertical");
        }
        // プレイヤーの向きを調整.
        bool isKeyInput = (horizontalKeyInput != 0 || verticalKeyInput != 0 || leftTouchInput != Vector2.zero);
        if (isKeyInput == true && isAttack == false)
        {
            bool currentIsRun = animator.GetBool("isRun");
            if (currentIsRun == false) animator.SetBool("isRun", true);
            Vector3 dir = rigid.velocity.normalized;
            dir.y = 0;
            this.transform.forward = dir;
        }
        else
        {
            bool currentIsRun = animator.GetBool("isRun");
            if (currentIsRun == true) animator.SetBool("isRun", false);
        }
    }

    void FixedUpdate()
    {
        if (isAttack == false)
        {
            Vector3 input = new Vector3();
            Vector3 move = new Vector3();
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                input = new Vector3(leftTouchInput.x, 0, leftTouchInput.y);
                move = input.normalized * 2f;
            }
            else
            {
                input = new Vector3(horizontalKeyInput, 0, verticalKeyInput);
                move = input.normalized * 2f;
            }
            Vector3 cameraMove = Camera.main.gameObject.transform.rotation * move;
            cameraMove.y = 0;
            Vector3 currentRigidVelocity = rigid.velocity;
            currentRigidVelocity.y = 0;

            rigid.AddForce(cameraMove - currentRigidVelocity, ForceMode.VelocityChange);
        }

        // カメラの位置をプレイヤーに合わせる.
        cameraController.FixedUpdateCameraPosition(this.transform);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// 攻撃ボタンクリックコールバック.
    /// </summary>
    // ---------------------------------------------------------------------
    public void OnAttackButtonClicked()
    {
        if (isAttack == false)
        {
            // AnimationのisAttackトリガーを起動.
            animator.SetTrigger("isAttack");
            // 攻撃開始.
            isAttack = true;
        }
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// ジャンプボタンクリックコールバック.
    /// </summary>
    // ---------------------------------------------------------------------
    public void OnJumpButtonClicked()
    {
        if (isGround == true)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// FootSphereトリガーエンターコール.
    /// </summary>
    /// <param name="col"> 侵入したコライダー. </param>
    // ---------------------------------------------------------------------
    void OnFootTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Ground")
        {
            isGround = true;
            animator.SetBool("isGround", true);
        }
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// FootSphereトリガーイグジットコール.
    /// </summary>
    /// <param name="col"> 侵入したコライダー. </param>
    // ---------------------------------------------------------------------
    void OnFootTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Ground")
        {
            isGround = false;
            animator.SetBool("isGround", false);
        }
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// 攻撃アニメーションHitイベントコール.
    /// </summary>
    // ---------------------------------------------------------------------
    void Anim_AttackHit()
    {
        Debug.Log("Hit");
        // 攻撃判定用オブジェクトを表示.
        attackHit.SetActive(true);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// 攻撃アニメーション終了イベントコール.
    /// </summary>
    // ---------------------------------------------------------------------
    void Anim_AttackEnd()
    {
        Debug.Log("End");
        // 攻撃判定用オブジェクトを非表示に.
        attackHit.SetActive(false);
        // 攻撃終了.
        isAttack = false;
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// 攻撃判定トリガーエンターイベントコール.
    /// </summary>
    /// <param name="col"> 侵入したコライダー. </param>
    // ---------------------------------------------------------------------
    void OnAttackHitTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Enemy")
        {
            var enemy = col.gameObject.GetComponent<EnemyBase>();
            enemy?.OnAttackHit(CurrentStatus.Power, this.transform.position);
            attackHit.SetActive(false);
        }
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// 敵の攻撃がヒットしたときの処理.
    /// </summary>
    /// <param name="damage"> 食らったダメージ. </param>
    // ---------------------------------------------------------------------
    public void OnEnemyAttackHit(int damage, Vector3 attackPosition)
    {
        CurrentStatus.Hp -= damage;

        var pos = myCollider.ClosestPoint(attackPosition);
        var obj = Instantiate(hitParticlePrefab, pos, Quaternion.identity);
        var par = obj.GetComponent<ParticleSystem>();
        StartCoroutine(WaitDestroy(par));
        particleObjectList.Add(obj);

        if (CurrentStatus.Hp <= 0)
        {
            OnDie();
        }
        else
        {
            Debug.Log(damage + "のダメージを食らった!!残りHP" + CurrentStatus.Hp);
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
        if (particleObjectList.Contains(particle.gameObject) == true) particleObjectList.Remove(particle.gameObject);
        Destroy(particle.gameObject);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// 死亡時処理.
    /// </summary>
    // ---------------------------------------------------------------------
    void OnDie()
    {
        Debug.Log("死亡しました。");

        StopAllCoroutines();
        if (particleObjectList.Count > 0)
        {
            foreach (var obj in particleObjectList) Destroy(obj);
            particleObjectList.Clear();
        }
    }
}
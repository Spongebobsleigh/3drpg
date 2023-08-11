using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------
    /// <summary>
    /// �X�e�[�^�X.
    /// </summary>
    // -------------------------------------------------------
    [System.Serializable]
    public class Status
    {
        // �̗�.
        public int Hp = 10;
        // �U����.
        public int Power = 1;
    }

    // �U��Hit�I�u�W�F�N�g��ColliderCall.
    [SerializeField] ColliderCallReceiver attackHitCall = null;
    // ��{�X�e�[�^�X.
    [SerializeField] Status DefaultStatus = new Status();
    // ���݂̃X�e�[�^�X.
    public Status CurrentStatus = new Status();

    // �U������p�I�u�W�F�N�g.
    [SerializeField] GameObject attackHit = null;
    // �ݒu����pColliderCall.
    [SerializeField] ColliderCallReceiver footColliderCall = null;
    // �^�b�`�}�[�J�[.
    [SerializeField] GameObject touchMarker = null;
    // �W�����v��.
    [SerializeField] float jumpPower = 20f;
    // �A�j���[�^�[.
    Animator animator = null;
    // ���W�b�h�{�f�B.
    Rigidbody rigid = null;
    // �U���A�j���[�V�������t���O.
    bool isAttack = false;
    // �ڒn�t���O.
    bool isGround = false;

    // PC�L�[����������.
    float horizontalKeyInput = 0;
    // PC�L�[�c��������.
    float verticalKeyInput = 0;

    bool isTouch = false;

    // �������^�b�`�X�^�[�g�ʒu.
    Vector2 leftStartTouch = new Vector2();
    // �������^�b�`����.
    Vector2 leftTouchInput = new Vector2();

    // �J�����R���g���[���[.
    [SerializeField] PlayerCameraController cameraController = null;

    // ���g�̃R���C�_�[.
    [SerializeField] Collider myCollider = null;
    // �U����H������Ƃ��̃p�[�e�B�N���v���n�u.
    [SerializeField] GameObject hitParticlePrefab = null;
    // �p�[�e�B�N���I�u�W�F�N�g�ۊǗp���X�g.
    List<GameObject> particleObjectList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Animator���擾���ۊ�.
        animator = GetComponent<Animator>();
        // Rigidbody�̎擾.
        rigid = GetComponent<Rigidbody>();
        // �U������p�I�u�W�F�N�g���\����.
        attackHit.SetActive(false);

        // FootSphere�̃C�x���g�o�^.
        footColliderCall.TriggerEnterEvent.AddListener(OnFootTriggerEnter);
        footColliderCall.TriggerExitEvent.AddListener(OnFootTriggerExit);

        // �U������p�R���C�_�[�C�x���g�o�^.
        attackHitCall.TriggerEnterEvent.AddListener(OnAttackHitTriggerEnter);
        // ���݂̃X�e�[�^�X�̏�����.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
    }

    
    // Update is called once per frame
    void Update()
    {
        // �J�������v���C���[�Ɍ�����. 
        cameraController.UpdateCameraLook(this.transform);

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // �X�}�z�^�b�`����.
            // �^�b�`���Ă���w�̐����O��葽��.
            if (Input.touchCount > 0)
            {
                isTouch = true;
                // �^�b�`�������ׂĎ擾.
                Touch[] touches = Input.touches;

                // �S���̃^�b�`���J��Ԃ��Ĕ���.
                foreach (var touch in touches)
                {
                    bool isLeftTouch = false;
                    bool isRightTouch = false;

                    // �^�b�`�ʒu��X���������X�N���[���̍���.
                    if (touch.position.x > 0 && touch.position.x < Screen.width / 2)
                    {
                        isLeftTouch = true;
                    }
                    // �^�b�`�ʒu��X���������X�N���[���̉E��.
                    else if (touch.position.x > Screen.width / 2 && touch.position.x < Screen.width)
                    {
                        isRightTouch = true;
                    }
                    
                    // ���^�b�`.
                    if (isLeftTouch == true)
                    {
                        // �^�b�`�J�n.
                        if (touch.phase == TouchPhase.Began)
                        {
                            Debug.Log("�^�b�`�J�n");
                            // �J�n�ʒu��ۊ�.
                            leftStartTouch = touch.position;
                            // �J�n�ʒu�Ƀ}�[�J�[��\��.
                            touchMarker.SetActive(true);
                            Vector3 touchPosition = touch.position;
                            touchPosition.z = 1f;
                            Vector3 markerPosition = Camera.main.ScreenToWorldPoint(touchPosition);
                            touchMarker.transform.position = markerPosition;
                        }
                        // �^�b�`��.
                        else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                        {
                            Debug.Log("�^�b�`��");
                            // ���݂̈ʒu�𐏎��ۊ�.
                            Vector2 position = touch.position;
                            // �ړ��p�̕�����ۊ�.
                            leftTouchInput = position - leftStartTouch;
                        }
                        // �^�b�`�I��.
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            Debug.Log("�^�b�`�I��");
                            leftTouchInput = Vector2.zero;
                            // �}�[�J�[���\��.
                            touchMarker.gameObject.SetActive(false);
                        }
                    }

                    // �E�^�b�`.
                    if (isRightTouch == true)
                    {
                        cameraController.UpdateRightTouch(touch);
                        // �E�������^�b�`�����ۂ̏���.
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
            // PC�L�[���͎擾.
            horizontalKeyInput = Input.GetAxis("Horizontal");
            verticalKeyInput = Input.GetAxis("Vertical");
        }
        // �v���C���[�̌����𒲐�.
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

        // �J�����̈ʒu���v���C���[�ɍ��킹��.
        cameraController.FixedUpdateCameraPosition(this.transform);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �U���{�^���N���b�N�R�[���o�b�N.
    /// </summary>
    // ---------------------------------------------------------------------
    public void OnAttackButtonClicked()
    {
        if (isAttack == false)
        {
            // Animation��isAttack�g���K�[���N��.
            animator.SetTrigger("isAttack");
            // �U���J�n.
            isAttack = true;
        }
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �W�����v�{�^���N���b�N�R�[���o�b�N.
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
    /// FootSphere�g���K�[�G���^�[�R�[��.
    /// </summary>
    /// <param name="col"> �N�������R���C�_�[. </param>
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
    /// FootSphere�g���K�[�C�O�W�b�g�R�[��.
    /// </summary>
    /// <param name="col"> �N�������R���C�_�[. </param>
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
    /// �U���A�j���[�V����Hit�C�x���g�R�[��.
    /// </summary>
    // ---------------------------------------------------------------------
    void Anim_AttackHit()
    {
        Debug.Log("Hit");
        // �U������p�I�u�W�F�N�g��\��.
        attackHit.SetActive(true);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �U���A�j���[�V�����I���C�x���g�R�[��.
    /// </summary>
    // ---------------------------------------------------------------------
    void Anim_AttackEnd()
    {
        Debug.Log("End");
        // �U������p�I�u�W�F�N�g���\����.
        attackHit.SetActive(false);
        // �U���I��.
        isAttack = false;
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �U������g���K�[�G���^�[�C�x���g�R�[��.
    /// </summary>
    /// <param name="col"> �N�������R���C�_�[. </param>
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
    /// �G�̍U�����q�b�g�����Ƃ��̏���.
    /// </summary>
    /// <param name="damage"> �H������_���[�W. </param>
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
            Debug.Log(damage + "�̃_���[�W��H�����!!�c��HP" + CurrentStatus.Hp);
        }
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �p�[�e�B�N�����I��������j������.
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
    /// ���S������.
    /// </summary>
    // ---------------------------------------------------------------------
    void OnDie()
    {
        Debug.Log("���S���܂����B");

        StopAllCoroutines();
        if (particleObjectList.Count > 0)
        {
            foreach (var obj in particleObjectList) Destroy(obj);
            particleObjectList.Clear();
        }
    }
}
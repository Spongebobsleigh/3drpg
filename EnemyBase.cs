using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyBase : MonoBehaviour
{
    // ----------------------------------------------------------
    /// <summary>
    /// �X�e�[�^�X.
    /// </summary>
    // ----------------------------------------------------------
    [System.Serializable]
    public class Status
    {
        // HP.
        public int Hp = 10;
        // �U����.
        public int Power = 1;
    }

    // ��{�X�e�[�^�X.
    [SerializeField] Status DefaultStatus = new Status();
    // ���݂̃X�e�[�^�X.
    public Status CurrentStatus = new Status();

    // �A�j���[�^�[.
    Animator animator = null;

    // ���Ӄ��[�_�[�R���C�_�[�R�[��.
    [SerializeField] ColliderCallReceiver aroundColliderCall = null;

    // �U���Ԋu.
    [SerializeField] float attackInterval = 3f;
    // �U����ԃt���O.
    bool isBattle = false;
    // �U�����Ԍv���p.
    float attackTimer = 0f;

    //! �U������p�R���C�_�[�R�[��.
    [SerializeField] ColliderCallReceiver attackHitColliderCall = null;

    //! ���g�̃R���C�_�[.
    [SerializeField] Collider myCollider = null;
    //! �U���q�b�g���G�t�F�N�g�v���n�u.
    [SerializeField] GameObject hitParticlePrefab = null;

    void Start()
    {
        // Animator���擾���ۊ�.
        animator = GetComponent<Animator>();
        // �ŏ��Ɍ��݂̃X�e�[�^�X����{�X�e�[�^�X�Ƃ��Đݒ�.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // ���ӃR���C�_�[�C�x���g�o�^.
        aroundColliderCall.TriggerEnterEvent.AddListener(OnAroundTriggerEnter);
        aroundColliderCall.TriggerStayEvent.AddListener(OnAroundTriggerStay);
        aroundColliderCall.TriggerExitEvent.AddListener(OnAroundTriggerExit);
        // �U���R���C�_�[�C�x���g�o�^.
        attackHitColliderCall.TriggerEnterEvent.AddListener(OnAttackTriggerEnter);

        attackHitColliderCall.gameObject.SetActive(false);
    }

    void Update()
    {
        // �U���ł����Ԃ̎�.
        if (isBattle == true)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= 3f)
            {
                animator.SetTrigger("isAttack");
                attackTimer = 0;
            }
        }
        else
        {
            attackTimer = 0;
        }
    }

    // ------------------------------------------------------------
    /// <summary>
    /// ���Ӄ��[�_�[�R���C�_�[�G���^�[�C�x���g�R�[��.
    /// </summary>
    /// <param name="other"> �ڋ߃R���C�_�[. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isBattle = true;
        }
    }

    

    // ------------------------------------------------------------
    /// <summary>
    /// ���Ӄ��[�_�[�R���C�_�[�X�e�C�C�x���g�R�[��.
    /// </summary>
    /// <param name="other"> �ڋ߃R���C�_�[. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var _dir = (other.gameObject.transform.position - this.transform.position).normalized;
            _dir.y = 0;
            this.transform.forward = _dir;
        }
    }

    // ------------------------------------------------------------
    /// <summary>
    /// ���Ӄ��[�_�[�R���C�_�[�I���C�x���g�R�[��.
    /// </summary>
    /// <param name="other"> �ڋ߃R���C�_�[. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isBattle = false;
        }
    }

    // ------------------------------------------------------------
    /// <summary>
    /// �U���R���C�_�[�G���^�[�C�x���g�R�[��.
    /// </summary>
    /// <param name="other"> �ڋ߃R���C�_�[. </param>
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
    /// �U���q�b�g���R�[��.
    /// </summary>
    /// <param name="damage"> �H������_���[�W. </param>
    // ----------------------------------------------------------
    public void OnAttackHit(int damage, Vector3 attackPosition)
    {
        CurrentStatus.Hp -= damage;
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
    /// �p�[�e�B�N�����I��������j������.
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
    /// ���S���R�[��.
    /// </summary>
    // ----------------------------------------------------------
    void OnDie()
    {
        Debug.Log("���S");
        
        animator.SetBool("isDie", true);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// ���S�A�j���[�V�����I�����R�[��.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_DieEnd()
    {
        this.gameObject.SetActive(false);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// �U��Hit�A�j���[�V�����R�[��.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_AttackHit()
    {
        attackHitColliderCall.gameObject.SetActive(true);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// �U���A�j���[�V�����I�����R�[��.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_AttackEnd()
    {
        attackHitColliderCall.gameObject.SetActive(false);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // �Q�[���I�[�o�[�I�u�W�F�N�g.
    [SerializeField] GameObject gameOver = null;
    // �Q�[���N���A�I�u�W�F�N�g.
    [SerializeField] GameObject gameClear = null;

    // �v���C���[.
    [SerializeField] PlayerController player = null;

    // �G�̈ړ��^�[�Q�b�g���X�g.
    [SerializeField] List<Transform> enemyTargets = new List<Transform>();

    // �G�v���n�u���X�g.
    [SerializeField] List<GameObject> enemyPrefabList = new List<GameObject>();
    // �G�o���n�_���X�g.
    [SerializeField] List<Transform> enemyGateList = new List<Transform>();
    // �t�B�[���h��ɂ���G���X�g.
    List<EnemyBase> fieldEnemys = new List<EnemyBase>();

    //! �G���������t���O.
    bool isEnemySpawn = false;
    //! ���݂̓G���j��.
    int currentBossCount = 0;

    //! �{�X�v���n�u.
    [SerializeField] GameObject bossPrefab = null;

    // �{�X�o���t���O.
    bool isBossAppeared = false;

    void Start()
    {
        player.GameOverEvent.AddListener(OnGameOver);

        gameOver.SetActive(false);

        Init();

    }

    // --------------------------------------------------------------------- 
    /// <summary> 
    /// ����������. 
    /// </summary> 
    // ---------------------------------------------------------------------
    // --------------------------------------------------------------------- 
    /// <summary> 
    /// ����������. 
    /// </summary> 
    // ---------------------------------------------------------------------
    void Init()
    {
        // �G�̐����J�n.
        isEnemySpawn = true;
        StartCoroutine(EnemyCreateLoop());

        currentBossCount = 0;
        isBossAppeared = false;
    }

    // --------------------------------------------------------------------- 
    /// <summary>
    /// �G�������[�v�R���[�`��.
    /// </summary>
    // --------------------------------------------------------------------- 
    IEnumerator EnemyCreateLoop()
    {
        while (isEnemySpawn == true)
        {
            yield return new WaitForSeconds(2f);

            if (fieldEnemys.Count < 10)
            {
                CreateEnemy();
            }
            // 10�̈ȏ�|���Ă����琶�����~.
            if (currentBossCount > 10) isEnemySpawn = false;

            if (isEnemySpawn == false) break;
        }
    }

    // --------------------------------------------------------------------- 
    /// <summary>
    /// �{�X�̐���.
    /// </summary>
    // --------------------------------------------------------------------- 
    void CreateBoss()
    {
        if (isBossAppeared == true) return;

        Debug.Log("Boss���o��!!");

        var posNum = Random.Range(0, enemyGateList.Count);
        var pos = enemyGateList[posNum];

        var obj = Instantiate(bossPrefab, pos.position, Quaternion.identity);
        var enemy = obj.GetComponent<EnemyBase>();

        enemy.ArrivalEvent.AddListener(EnemyMove);
        enemy.DestroyEvent.AddListener(EnemyDestroy);

        isBossAppeared = true;
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �G���쐬.
    /// </summary>
    // ---------------------------------------------------------------------
    void CreateEnemy()
    {
        var num = Random.Range(0, enemyPrefabList.Count);
        var prefab = enemyPrefabList[num];

        var posNum = Random.Range(0, enemyGateList.Count);
        var pos = enemyGateList[posNum];

        var obj = Instantiate(prefab, pos.position, Quaternion.identity);
        var enemy = obj.GetComponent<EnemyBase>();

        enemy.ArrivalEvent.AddListener(EnemyMove);
        enemy.DestroyEvent.AddListener(EnemyDestroy);

        fieldEnemys.Add(enemy);
    }


    // ---------------------------------------------------------------------
    /// <summary>
    /// ���X�g���烉���_���Ƀ^�[�Q�b�g���擾.
    /// </summary>
    /// <returns> �^�[�Q�b�g. </returns>
    // ---------------------------------------------------------------------
    Transform GetEnemyMoveTarget()
    {
        if (enemyTargets == null || enemyTargets.Count == 0) return null;
        else if (enemyTargets.Count == 1) return enemyTargets[0];

        int num = Random.Range(0, enemyTargets.Count);
        return enemyTargets[num];
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �G�Ɏ��̖ړI�n��ݒ�.
    /// </summary>
    /// <param name="enemy"> �G. </param>
    // ---------------------------------------------------------------------
    void EnemyMove(EnemyBase enemy)
    {
        var target = GetEnemyMoveTarget();
        if (target != null) enemy.SetNextTarget(target);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �G�j�󎞂̃C�x���g.
    /// </summary>
    /// <param name="enemy"> �G. </param>
    // ---------------------------------------------------------------------
    void EnemyDestroy(EnemyBase enemy)
    {
        if (fieldEnemys.Contains(enemy) == true)
        {
            fieldEnemys.Remove(enemy);
        }
        Destroy(enemy.gameObject);


        if (enemy.IsBoss == false)
        {
            currentBossCount++;
            if (currentBossCount > 10)
            {
                CreateBoss();
            }
        }
        else
        {
            Debug.Log("GameClear!!");
            // �Q�[���N���A��\��.
            gameClear.SetActive(true);

            isEnemySpawn = false;
            // �t�B�[���h��̓G���폜�����X�g�����Z�b�g.
            foreach (EnemyBase e in fieldEnemys)
            {
                Destroy(e.gameObject);
            }
            fieldEnemys.Clear();
        }
    }


    // ---------------------------------------------------------------------
    /// <summary>
    /// �Q�[���I�[�o�[���Ƀv���C���[����Ă΂��.
    /// </summary>
    // ---------------------------------------------------------------------
    void OnGameOver()
    {
        // �Q�[���I�[�o�[��\��.
        gameOver.SetActive(true);
        // �v���C���[���\��.
        player.gameObject.SetActive(false);
        // �G�̍U���t���O������.
        foreach (EnemyBase enemy in fieldEnemys) enemy.IsBattle = false;
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// ���g���C�{�^���N���b�N�R�[���o�b�N.
    /// </summary>
    // ---------------------------------------------------------------------
    public void OnRetryButtonClicked()
    {
        // �v���C���[���g���C����.
        player.Retry();
        // �G�̃��g���C����.
        foreach (EnemyBase enemy in fieldEnemys)
        {
            Destroy(enemy.gameObject);
        }
        fieldEnemys.Clear();
        // �v���C���[��\��.
        player.gameObject.SetActive(true);
        // �Q�[���I�[�o�[���\��.
        gameOver.SetActive(false);
        // �Q�[���N���A���\��.
        gameClear.SetActive(false);

        Init();
    }
}
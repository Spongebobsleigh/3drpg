using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    // ゲームオーバーオブジェクト.
    [SerializeField] GameObject gameOver = null;
    // ゲームクリアオブジェクト.
    [SerializeField] GameObject gameClear = null;

    // プレイヤー.
    [SerializeField] PlayerController player = null;

    // 敵の移動ターゲットリスト.
    [SerializeField] List<Transform> enemyTargets = new List<Transform>();

    // 敵プレハブリスト.
    [SerializeField] List<GameObject> enemyPrefabList = new List<GameObject>();
    // 敵出現地点リスト.
    [SerializeField] List<Transform> enemyGateList = new List<Transform>();
    // フィールド上にいる敵リスト.
    List<EnemyBase> fieldEnemys = new List<EnemyBase>();

    //! 敵自動生成フラグ.
    bool isEnemySpawn = false;
    //! 現在の敵撃破数.
    int currentBossCount = 0;

    //! ボスプレハブ.
    [SerializeField] GameObject bossPrefab = null;

    // ボス出現フラグ.
    bool isBossAppeared = false;

    // ゲームクリア画面での時間表示テキスト.
    [SerializeField] TextMeshPro gameClearTimeText = null;
    // 通常時の画面に時間表示するためのテキスト.
    [SerializeField] TextMeshPro timerText = null;
    //! 現在の時間.
    float currentTime = 0;
    //! 時間計測フラグ.
    bool isTimer = false;

    void Start()
    {
        player.GameOverEvent.AddListener(OnGameOver);

        gameOver.SetActive(false);

        Init();

    }

    void Update()
    {
        if (isTimer == true)
        {
            currentTime += Time.deltaTime;
            if (currentTime > 999f) timerText.text = "999.9";
            else timerText.text = currentTime.ToString("000.0");
        }
    }

    // --------------------------------------------------------------------- 
    /// <summary> 
    /// 初期化処理. 
    /// </summary> 
    // ---------------------------------------------------------------------
    // --------------------------------------------------------------------- 
    /// <summary> 
    /// 初期化処理. 
    /// </summary> 
    // ---------------------------------------------------------------------
    void Init()
    {
        // 敵の生成開始.
        isEnemySpawn = true;
        StartCoroutine(EnemyCreateLoop());

        currentBossCount = 0;
        isBossAppeared = false;

        currentTime = 0;
        isTimer = true;
        timerText.text = "0:00";
    }

    // --------------------------------------------------------------------- 
    /// <summary>
    /// 敵生成ループコルーチン.
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
            // 10体以上倒していたら生成中止.
            if (currentBossCount > 10) isEnemySpawn = false;

            if (isEnemySpawn == false) break;
        }
    }

    // --------------------------------------------------------------------- 
    /// <summary>
    /// ボスの生成.
    /// </summary>
    // --------------------------------------------------------------------- 
    void CreateBoss()
    {
        if (isBossAppeared == true) return;

        Debug.Log("Bossが出現!!");

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
    /// 敵を作成.
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
    /// リストからランダムにターゲットを取得.
    /// </summary>
    /// <returns> ターゲット. </returns>
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
    /// 敵に次の目的地を設定.
    /// </summary>
    /// <param name="enemy"> 敵. </param>
    // ---------------------------------------------------------------------
    void EnemyMove(EnemyBase enemy)
    {
        var target = GetEnemyMoveTarget();
        if (target != null) enemy.SetNextTarget(target);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// 敵破壊時のイベント.
    /// </summary>
    /// <param name="enemy"> 敵. </param>
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
            isTimer = false;

            if (currentTime > 999f) gameClearTimeText.text = "Time : 999.9";
            else gameClearTimeText.text = "Time : " + currentTime.ToString("000.0");
            // ゲームクリアを表示.
            gameClear.SetActive(true);

            isEnemySpawn = false;
            // フィールド上の敵を削除しリストをリセット.
            foreach (EnemyBase e in fieldEnemys)
            {
                Destroy(e.gameObject);
            }
            fieldEnemys.Clear();
        }
    }


    // ---------------------------------------------------------------------
    /// <summary>
    /// ゲームオーバー時にプレイヤーから呼ばれる.
    /// </summary>
    // ---------------------------------------------------------------------
    void OnGameOver()
    {
        isTimer = false;
        // ゲームオーバーを表示.
        gameOver.SetActive(true);
        // プレイヤーを非表示.
        player.gameObject.SetActive(false);
        // 敵の攻撃フラグを解除.
        foreach (EnemyBase enemy in fieldEnemys) enemy.IsBattle = false;
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// リトライボタンクリックコールバック.
    /// </summary>
    // ---------------------------------------------------------------------
    public void OnRetryButtonClicked()
    {
        // プレイヤーリトライ処理.
        player.Retry();
        // 敵のリトライ処理.
        foreach (EnemyBase enemy in fieldEnemys)
        {
            Destroy(enemy.gameObject);
        }
        fieldEnemys.Clear();
        // プレイヤーを表示.
        player.gameObject.SetActive(true);
        // ゲームオーバーを非表示.
        gameOver.SetActive(false);
        // ゲームクリアを非表示.
        gameClear.SetActive(false);

        Init();
    }
}

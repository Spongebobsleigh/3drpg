using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ColliderCallReceiver))]
// ----------------------------------------------------------------------
/// <summary>
/// アイテム基底クラス.
/// </summary>
// ----------------------------------------------------------------------
public class ItemBase : MonoBehaviour
{
    [Header("Base Param")]
    // 取得時のエフェクトプレハブ.
    [SerializeField] GameObject effectParticle = null;
    // アイテムのレンダラー.
    [SerializeField] Renderer itemRenderer = null;

    // コライダーコール.
    ColliderCallReceiver colliderCall = null;
    // エフェクト実行済フラグ.
    bool isEffective = true;


    protected virtual void Start()
    {
        colliderCall = GetComponent<ColliderCallReceiver>();
        colliderCall.TriggerEnterEvent.AddListener(OnTriggerEnter);
    }

    // ----------------------------------------------------------------------
    /// <summary>
    /// アイテムのトリガーコライダーエンター時コール.
    /// </summary>
    /// <param name="col"> 侵入したコライダー. </param>
    // ----------------------------------------------------------------------
    void OnTriggerEnter(Collider col)
    {
        if (isEffective == false) return;

        if (col.gameObject.tag == "Player")
        {
            Debug.Log("アイテムを取得");

            // オーバーライド可能な処理を実行.
            ItemAction(col);
            isEffective = false;

            // エフェクト表示.
            if (effectParticle != null)
            {
                var pos = (itemRenderer == null) ? this.transform.position : itemRenderer.gameObject.transform.position;
                var obj = Instantiate(effectParticle, pos, Quaternion.identity);
                var particle = obj.GetComponent<ParticleSystem>();
                StartCoroutine(AutoDestroy(particle));
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    // ----------------------------------------------------------------------
    /// <summary>
    /// 自動破棄.
    /// </summary>
    /// <param name="particle"> パーティクル. </param>
    // ----------------------------------------------------------------------
    IEnumerator AutoDestroy(ParticleSystem particle)
    {
        // 先にレンダラーを消す.
        if (itemRenderer != null) itemRenderer.enabled = false;

        yield return new WaitUntil(() => particle.isPlaying == false);

        // 破棄.
        Destroy(gameObject);
    }

    // ----------------------------------------------------------------------
    /// <summary>
    /// アイテム取得時の処理（オーバーライド可能）.
    /// </summary>
    /// <param name="col"> 取得したコライダー. </param>
    // ----------------------------------------------------------------------
    protected virtual void ItemAction(Collider col)
    {

    }
}
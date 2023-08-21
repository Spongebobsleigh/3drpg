using UnityEngine;

// -------------------------------------------------------------
/// <summary>
/// 回復アイテムクラス.
/// </summary>
// -------------------------------------------------------------
public class Item_HealPod : ItemBase
{
    [Header("Item Param")]
    // 回復量.
    [SerializeField] int healPoint = 10;

    protected override void Start()
    {
        base.Start();
    }

    // -------------------------------------------------------------
    /// <summary>
    /// アイテム取得時処理.
    /// </summary>
    /// <param name="col"> 侵入してきたコライダー. </param>
    // -------------------------------------------------------------
    protected override void ItemAction(Collider col)
    {
        base.ItemAction(col);
        var player = col.gameObject.GetComponent<PlayerController>();
        player.OnHeal(healPoint);
    }
}
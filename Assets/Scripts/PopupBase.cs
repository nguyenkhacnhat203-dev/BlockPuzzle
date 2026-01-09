using UnityEngine;
using DG.Tweening;

public abstract class PopupBase : MonoBehaviour
{
    public GameObject Main;


 

    protected virtual void Start()
    {
        OnShow();
    }

    protected virtual void OnShow()
    {
        if (Main != null)
        {
            Main.transform.localScale = Vector3.zero;

            Main.transform.DOScale(new Vector3(1.25f, 1.25f, 1.25f), 0.35f).OnComplete(() =>
            {
                Main.transform.DOScale(Vector3.one, 0.25f);
            });
        }
    }

    public virtual void DestroyPopup()
    {
        AudioManager.Instance.Btn_Click();

        if (Main == null)
        {
            Destroy(gameObject);
            return;
        }

        Main.transform.DOScale(new Vector3(1.25f, 1.25f, 1.25f), 0.25f).OnComplete(() =>
        {
            Main.transform.DOScale(Vector3.zero, 0.25f).OnComplete(() =>
            {
                Destroy(gameObject);
                PopupManager.Instance.isShowPopup = false;
            });
        });
    }
}

using UnityEngine;

public class AlmanacEntry : MonoBehaviour {
    public AlmanacSelections content;
    public Sprite TheNewContentImage;
    public int ID;

    public void Start() {
        if (AlmanacProgress.EntryIsUnlocked(ID)) {
            transform.Find("Lock").gameObject.SetActive(false);

            // Center the text
            Transform text_transform = transform.Find("InnerText");
            transform.Find("InnerText").localPosition = new Vector3(0, 0, text_transform.localPosition.z);
        }
    }

    public void OnClick() {
        if (AlmanacProgress.EntryIsUnlocked(ID)) {
            content.Switch(TheNewContentImage);
        }
    }
}
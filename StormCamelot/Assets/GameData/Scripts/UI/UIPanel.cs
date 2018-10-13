using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPanel : MonoBehaviour
{
	public float transitionTimer;
	public RectTransform rectTransfrom;
	public bool StartUpPanel = false;

	void Awake ()
	{
		rectTransfrom = this.gameObject.GetComponent<RectTransform> ();
	}
}

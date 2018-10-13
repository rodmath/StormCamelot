using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class UIPanelManager : MonoBehaviour
{
	[Header("Key Panels")]
	public UIPanel startPanel;
	public UIPanel emptyPanel;

	public List<UIPanel> panels;

	private List<UIPanel> panelHistory;
	private UIPanel currentPanel;


	void Start()
	{
		SetupPanels();
	}



	private void SetupPanels()
	{
		panels = FindObjectsOfType<UIPanel>().ToList();
		panelHistory = new List<UIPanel>();
		//if no panels we're done
		if (panels.Count == 0)
			return;

		//put them all in the centre of the screen and make sure the one marked as first is showing
		UIPanel firstPanel = panels[0];
		int firstPanelCount = 0;

		foreach (UIPanel panel in panels)
		{
			panel.rectTransfrom.offsetMin = Vector2.zero;
			panel.rectTransfrom.offsetMax = Vector2.zero;

			if (panel.StartUpPanel)
			{
				firstPanelCount++;
				firstPanel = panel;
			}
		}

		//Give feedback on special cases
		if (firstPanelCount == 0)
			Debug.Log("No first UI Panel chosen");
		if (firstPanelCount > 1)
			Debug.Log("More than one first UI Panel");


		foreach (UIPanel panel in panels)
			panel.gameObject.SetActive(panel == firstPanel);

		currentPanel = firstPanel;
	}


	public void ShowPanel(UIPanel panelToShow)
	{
		//		Debug.Log ("Showing panel: " + panelToShow.name);

		//If it it the same as currently shown, do nothing
		if (panelToShow == currentPanel)
			return;

		//If it panel doesn't exist, do nothing
		if (!panels.Contains(panelToShow))
		{
			Debug.LogError("Trying to show panel which doesn't exist in the list of panels from startup?");
			return;
		}

		foreach (UIPanel panel in panels)
			panel.gameObject.SetActive(panel == panelToShow);

		panelHistory.Insert(0, currentPanel);
		currentPanel = panelToShow;
	}

	public void ShowPreviousPanel()
	{
		Debug.Log("Show previous panel");

		if (panelHistory.Count == 0)
		{
			Debug.Log("No previous panel to go back to");
			return;
		}

		UIPanel prev = panelHistory[0];
		ShowPanel(prev);

		//now remove the panel we are switching to and the one we currently have.
		panelHistory.RemoveRange(0, 2);
	}




	public void ShowStartPanel()
	{
		ShowPanel(startPanel);
	}

	public void ShowEmptyPanel()
	{
		ShowPanel(emptyPanel);
	}
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HideAllViews : MonoBehaviour 
{
    public List<GameObject> views = new List<GameObject>();

	[SerializeField]
	private GameObject header, panel, multiplayer, indicator;
	private CanvasGroup canvas;
	private Image panelImage, multiplayerImage, indicatorColor;
	private int step = 0;


	public void Start()
	{
		panelImage = panel.GetComponent<Image>();
		multiplayerImage = multiplayer.GetComponent<Image>();
		indicatorColor = indicator.GetComponent<Image>();
		canvas = multiplayer.GetComponent<CanvasGroup>();
	}


	public void HideThem()
	{
		foreach (var view in views)
		{
			view.SetActive(false);
		}
		header.SetActive(true);
		panelImage.enabled = true;
		multiplayerImage.enabled = true;
		canvas.alpha = 1f;
		canvas.interactable = true;
		indicatorColor.color = new Color32(200, 0, 0, 255);
		step = 1;
	}


	public void HideKoboldMenu()
	{
		//Debug.Log("start " + step);
		switch (step)
		{
			default:
				header.SetActive(true);
				panelImage.enabled = true;
				multiplayerImage.enabled = true;
				canvas.alpha = 1;
				canvas.interactable = true;
				indicatorColor.color = new Color32(200, 0, 0, 255);
				step=0;
				break;
			case 1:
				header.SetActive(false);
				panelImage.enabled = false;
				multiplayerImage.enabled = false;
				indicatorColor.color = new Color32(200, 200, 0, 255);
				break;
			case 2:
				canvas.alpha = 0;
				canvas.interactable = false;
				indicatorColor.color = new Color32(0, 200, 0, 255);
				break;
		}
		step++;
		//Debug.Log("end " + step);

	}

	
}
